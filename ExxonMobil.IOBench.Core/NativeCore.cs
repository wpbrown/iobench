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
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using ExxonMobil.Shared.Win32;

namespace ExxonMobil.IOBench.Core
{
    static class NativeCore
    {
		[DllImport("ExxonMobil.IOBench.NativeCore.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern bool DisableLocalBuffering(SafeFileHandle hFile, bool async);

		[DllImport("ExxonMobil.IOBench.NativeCore.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern bool Experimental_EnableRemotePrefetch(SafeFileHandle hFile, bool async);

		[DllImport("ExxonMobil.IOBench.NativeCore.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool PreallocZeroed(SafeFileHandle hFile, long fileSize, bool async);

		[DllImport("ExxonMobil.IOBench.NativeCore.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern bool AsynchronousOp(SafeFileHandle hFile, BenchmarkOperation operation, AccessPattern accessPattern, bool verify, int blocks, int blockSize, bool randomData, int maxOutstanding, IntPtr status);

		[DllImport("ExxonMobil.IOBench.NativeCore.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern bool SynchronousOp(SafeFileHandle hFile, BenchmarkOperation operation, AccessPattern accessPattern, bool verify, int blocks, int blockSize, bool randomData, IntPtr status);

		public static void ThrowException()
		{
			var win32ex = new Win32Exception();
			if (win32ex.NativeErrorCode == ERROR_CRC)
				throw new BenchmarkException("Data verification failed.") {
					HelpText = "The data being read is not in the form written by this tool. Files being verified should be " +
						"written with an iobench write operation."
				};

			throw new BenchmarkException("A failure occured in the native I/O routine (" + win32ex.NativeErrorCode.ToString("X") + ").", win32ex)
			{
				HelpText = "Examine the exception log for more information."
			};
		}

		public static void PreallocateUnzerod(SafeFileHandle fileHandle, long sizeBytes)
		{
			if (!Win32Methods.SetFileValidData(fileHandle, sizeBytes))
			{
				var win32ex = new Win32Exception();
				throw new BenchmarkException("Could not set valid data length (fast preallocate).", win32ex)
				{
					HelpText = "The Manage Volume privilige was acquired for the current id on the local machine. " +
						"This is not neccessarily valid for the volume on which the file is stored."
				};
			}
		}

		public static void PreallocateZerod(SafeFileHandle fileHandle, long sizeBytes, bool isAsync)
		{
			if (!NativeCore.PreallocZeroed(fileHandle, sizeBytes, isAsync))
			{
				var win32ex = new Win32Exception();
				throw new BenchmarkException("Zeroed preallocation failed.", win32ex);
			}
		}

		public static void SetFileSize(SafeFileHandle fileHandle, long sizeBytes)
		{
			if (!Win32Methods.SetFilePointerEx(fileHandle, sizeBytes, IntPtr.Zero, System.IO.SeekOrigin.Begin))
				throw new Win32Exception();
			if (!Win32Methods.SetEndOfFile(fileHandle))
				throw new Win32Exception();
		}

		private const int ERROR_CRC = 0x00000017;
    }

	[StructLayout(LayoutKind.Sequential)]
	struct NativeCoreStatus
	{
		public volatile bool Canceled;

		public volatile int BlocksTransferred;
		public volatile int CompletedAsync;
		public volatile int CompletedSync;

		public long ReadWriteFilePerfCounts;
		public long GetQueuedCompletionStatusExPerfCounts;
	}
}
