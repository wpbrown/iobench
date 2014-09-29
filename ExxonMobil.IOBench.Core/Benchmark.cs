// Copyright 2014 ExxonMobil Technical Computing Company
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using ExxonMobil.Shared.Win32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection;
using System.IO;

namespace ExxonMobil.IOBench.Core
{
    public abstract class Benchmark
    {
        private Benchmark()
        {
        }

		protected Benchmark(BenchmarkConfiguration config, bool enablePerfmon)
		{
			this.config = config;
			this.bytesTotal = config.FileSizeBytes;

			// Creating this benchmark with a configuration that requires special privileges.
			// Try to acquire privilege and fail if we can't get it.
			if (config.Preallocation == PreallocationType.Unzeroed)
			{
				AcquireManageVolumePrivilige();
			}

			if (enablePerfmon)
				InitPerformanceCounters();
		}

		private static void InitPerformanceCounters()
		{
            if (perfCountersInitialized)
				return;

            Task.Factory.StartNew(() =>
            {
                var category = new PerformanceCounterCategory("Process");
                var names = category.GetInstanceNames();

                var thisImageName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                var thisPid = Process.GetCurrentProcess().Id;

                string name = names.Where(n => n.StartsWith(thisImageName)).FirstOrDefault(n =>
                {
                    using (var pidCounter = new PerformanceCounter("Process", "ID Process", n, true))
                    {
                        var sample = pidCounter.NextSample();
                        return sample.RawValue == thisPid;
                    }
                });

                if (name == null)
                    throw new BenchmarkException("Failed to detect performance counters for " + thisImageName + ":" + thisPid);

                counterDataBytes = new PerformanceCounter("Process", "IO Data Bytes/sec", name, true);
                counterDataOps = new PerformanceCounter("Process", "IO Data Operations/sec", name, true);

                perfCountersInitialized = true;
            });
		}

		protected SafeFileHandle CreateFile(string filePath)
		{
			var attributes = Win32FileAttributes.Normal;
			if (config.Asynchronous) attributes |= Win32FileAttributes.Overlapped;
			if (config.NoBuffering) attributes |= Win32FileAttributes.NoBuffering;
			if (config.WriteThrough) attributes |= Win32FileAttributes.WriteThrough;
			if (!config.NoOperationHints)
			{
				if (config.AccessPattern == AccessPattern.Sequential) attributes |= Win32FileAttributes.SequentialScan;
				else if (config.AccessPattern == AccessPattern.Random) attributes |= Win32FileAttributes.RandomAccess;
			}

			var access = config.IsRead ? Win32FileAccess.GenericRead : Win32FileAccess.GenericWrite;
			var disposition = config.IsRead ? Win32FileCreationDisposition.OpenAlways : Win32FileCreationDisposition.CreateAlways;

			createFileTime.Start();
			var fileHandle = Win32Methods.CreateFile(
				filePath,
				access,
				config.IsRead ? Win32FileShare.Read : Win32FileShare.None,
				IntPtr.Zero,
				disposition,
				attributes,
				IntPtr.Zero);
			int err = 0;
			if (fileHandle.IsInvalid)
				err = Marshal.GetLastWin32Error();
			createFileTime.Stop();
			if (fileHandle.IsInvalid)
				throw new Win32Exception(err);

			if (config.DisableLocalBuffering)
				if (!NativeCore.DisableLocalBuffering(fileHandle, config.Asynchronous))
					throw new BenchmarkException("Failed to send file system control code to disable local buffering.") 
					{ HelpText = "Disable local buffering is only valid for remote files." };

			if (config.EnableRemotePrefetch)
				if (!NativeCore.Experimental_EnableRemotePrefetch(fileHandle, config.Asynchronous))
					throw new BenchmarkException("Failed to enable remote prefetch.");

			return fileHandle;
		}

		private static void AcquireManageVolumePrivilige()
		{
			lock (typeof(Benchmark))
			{
				if (!requestedManageVolumePrivilege)
				{
					requestedManageVolumePrivilege = true;
					try
					{
						Win32Helpers.EnablePrivilege(Win32Methods.SE_MANAGE_VOLUME_NAME);
					}
					catch (Exception e)
					{
						throw new BenchmarkException("Cannot acquire Manage Volume privilege required for fast preallocation.", e)
						{
							HelpText = "You must have the user right 'Manage the files on a volume' to acquire the neccessary privilege " +
 								"for fast preallocation. By default, administrators have this right. In Windows 7 you must elevate this " +
								"process if UAC is enabled."
						};
					}
				}
			}
		}

		static Benchmark()
		{
			long frequency;
			Win32Methods.QueryPerformanceFrequency(out frequency);
			tickFrequency = 10000000.0 / frequency;
		}

		public Task Start()
		{
			return Task.Factory.StartNew(() => StartTask(CancellationToken.None), TaskCreationOptions.LongRunning);
		}

        public Task Start(CancellationToken token)
        {
			token.Register(() => status.Canceled = true);
			return Task.Factory.StartNew(() => StartTask(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

		private void StartTask(CancellationToken token)
		{
			this.Run();
			token.ThrowIfCancellationRequested();
		}

		public static Benchmark Create(BenchmarkConfiguration config)
        {
            var benchmark = new FileBenchmark(config);
            return benchmark;
        }

        public int CompletedSynchronously
        {
            get { return status.CompletedSync; }
        }

        public int CompletedAsynchronously
        {
            get { return status.CompletedAsync; }
        }

        public long BytesTransferred
        {
            get { return (long)status.BlocksTransferred * (long)config.BlockSizeBytes; }
        }

		public long BytesTotal
        {
            get { return bytesTotal; }
        }

        public int BlocksTransferred
        {
            get { return status.BlocksTransferred; }
        }

		public long AverageBytesTransferredPerSec
		{
			get {
				long value;
				long elapsedms = (config.FilePerBlock ? wallTime : transferTime).ElapsedMilliseconds;
				if (elapsedms == 0)
					value = 0;
				else
					value = BytesTransferred * 1000 / elapsedms;
				return value; 
			}
		}

        public bool InstantCountersAvailable
        {
            get { return perfCountersInitialized; }
        }

		public long InstantBytesTransferredPerSec
		{
			get
			{
				if (counterDataBytes == null)
					throw new InvalidOperationException("Can't get instant performance. Perfmon is not enabled for this benchmark.");
				return (long)counterDataBytes.NextValue();
			}
		}

		public int InstantDataOperationsPerSec
		{
			get
			{
				if (counterDataOps == null)
					throw new InvalidOperationException("Can't get instant performance. Perfmon is not enabled for this benchmark.");
				return (int)counterDataOps.NextValue();
			}
		}

		public double PercentComplete
		{
			get { return (double)status.BlocksTransferred / (double)config.Blocks; }
		}

        public TimeSpan ReadWriteFileTime
        {
            get {
                return PerfCountToTimeSpan(ref status.ReadWriteFilePerfCounts);
            }
        }

        public TimeSpan QueryCompletionPortTime
        {
            get
            {
                return PerfCountToTimeSpan(ref status.GetQueuedCompletionStatusExPerfCounts);
            }
        }

		public TimeSpan TransferTime
		{
			get
			{
				return transferTime.Elapsed;
			}
		}

        public TimeSpan WallTime
        {
            get
            {
                return wallTime.Elapsed;
            }
        }

		public TimeSpan PreallocationTime
		{
			get
			{
				return preallocTime.Elapsed;
			}
		}

		public TimeSpan CreateFileTime
		{
			get
			{
				return createFileTime.Elapsed;
			}
		}

		protected abstract void Run();

        private TimeSpan PerfCountToTimeSpan(ref long countSource)
        {
            long count = Interlocked.Read(ref countSource);
            double ticks = count * tickFrequency;
            return new TimeSpan((long)ticks);
        }

		protected BenchmarkConfiguration config;
        internal NativeCoreStatus status;

		private static PerformanceCounter counterDataBytes;
		private static PerformanceCounter counterDataOps;
        private static bool perfCountersInitialized;

		protected Stopwatch transferTime = new Stopwatch();
		protected Stopwatch preallocTime = new Stopwatch();
		protected Stopwatch createFileTime = new Stopwatch();
        protected Stopwatch wallTime = new Stopwatch();

		private long bytesTotal;
        private static readonly double tickFrequency;
		private static bool requestedManageVolumePrivilege = false;
    }
}
