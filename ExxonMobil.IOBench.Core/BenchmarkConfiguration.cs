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
using System.IO;
using ExxonMobil.Shared.Logging;
using System.Collections;
using ExxonMobil.Shared.Win32;

namespace ExxonMobil.IOBench.Core
{
    public class BenchmarkConfiguration
    {
		public BenchmarkConfiguration()
		{
			Asynchronous = false;
			AsyncMaxBlocksOutstanding = 8;
			Blocks = 1024; //1GB
			BlockSizeBytes = 1024 * 1024; //1MB
			NoBuffering = false;
			ReadVerify = false;
			Operation = BenchmarkOperation.Write;
			AccessPattern = AccessPattern.Sequential;
			WriteThrough = false;
			Preallocation = PreallocationType.None;
			WriteDataType = WriteDataType.Counter;
			Name = "Untitled";
		}

		public string Name { get; set; }
		public AccessPattern AccessPattern { get; set; }
		public BenchmarkOperation Operation { get; set; }
        public string FilePath { get; set; }
		public bool FilePerBlock { get; set; }

        public int Blocks { get; set; }
        public int BlockSizeBytes { get; set; }
        public int AsyncMaxBlocksOutstanding { get; set; }
        public bool ReadVerify { get; set; }

        public bool Asynchronous { get; set; }
		public bool DisableLocalBuffering { get; set; }
        public bool NoBuffering { get; set; }
        public bool WriteThrough { get; set; }
		public bool DontFlushBuffers { get; set; }
		public bool EnableRemotePrefetch { get; set; }
		public bool NoOperationHints { get; set; }

		public PreallocationType Preallocation { get; set; }
		public WriteDataType WriteDataType { get; set; }

		public long FileSizeBytes
		{
			get 
			{
				if (FilePerBlock)
					return (long)BlockSizeBytes;
				else
					return (long)Blocks * (long)BlockSizeBytes; 
			}
		}

		public bool IsRead { get { return Operation == BenchmarkOperation.Read; } }
		public bool IsWrite { get { return Operation == BenchmarkOperation.Write; } }

		public bool Validate(ILogger logger = null)
		{
			var v = new Validation(logger, "Configuration Issue: ");

			v.FailIf(() => String.IsNullOrWhiteSpace(FilePath), 
				"Invalid file path.");

			v.FailIf(() => FilePerBlock & AccessPattern != AccessPattern.Sequential, 
				"Multi-file operations must use sequential access pattern.");
			v.FailIf(() => FilePerBlock & Asynchronous,
				"Multi-file operations can not be asynchronous.");
			v.FailIf(() => FilePerBlock & Preallocation != PreallocationType.None,
				"Multi-file operations can not use preallocation.");

			v.FailIf(() => AccessPattern == AccessPattern.Random && 
				           (!VerifyPow2(Blocks) || Blocks < 4 || Blocks > 65536),
				"Random access operations must use a block count that is between 4 and 65536 and is a power of 2.");

			v.FailIf(() => AsyncMaxBlocksOutstanding < 1 || AsyncMaxBlocksOutstanding > 256,
				"Max outstanding asynchronous transfers must be between 1 and 256.");
			v.FailIf(() => Blocks < 0,
				"Block count must be >0.");
			v.FailIf(() => BlockSizeBytes < 4 * 1024 || BlockSizeBytes > 8 * 1024 * 1024,
				"Block size must be between 4kB and 8MB.");
			v.FailIf(() => BlockSizeBytes % (4 * 1024) != 0,
				"Block size must be a multiple of 4kB.");

			v.FailIf(() => !IsValidPath(FilePath),
				"Path must be to an existing file or a new file to create in an existing directory.");

			if (EnableRemotePrefetch)
				logger.Log("Experimental option \"EnableRemotePrefetch\" is in use.", Category.Warn);

			if (NoBuffering && IsNetworkPath(FilePath))
				logger.Log("Network transfer with -nb option. Performance will not be optimal.", Category.Warn);

			return !v.HasIssues;
		}

		private bool IsNetworkPath(string FilePath)
		{
			return DfsHelpers.IsPathUnc(FilePath) || DfsHelpers.IsPathRootedOnNetworkDrive(FilePath);
		}

		private static bool IsValidPath(string path)
		{
			if (Directory.Exists(path))
				return false;
			if (Directory.Exists(Path.GetDirectoryName(path)))
				return true;

			return false;
		}

		private static bool VerifyPow2(int blocks)
		{
			var bitArray = new BitArray(new int[] { blocks });
			return bitArray.Cast<bool>().Count(x => x) == 1;
		}
    }

	public enum AccessPattern : uint
	{
		Sequential = 1,
		Random     = 2
	}

	public enum BenchmarkOperation : uint
	{
		Write = 1,
		Read = 2
	}

	public enum PreallocationType
	{
		None,
		Zeroed,
		Unzeroed
	}

	public enum WriteDataType
	{
		Counter,
		Random
	}
}