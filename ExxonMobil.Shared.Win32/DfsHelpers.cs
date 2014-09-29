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
using System.Diagnostics;
using System.IO;
using ExxonMobil.Shared.Win32;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace ExxonMobil.Shared.Win32
{
	public static class DfsHelpers
	{
		public static bool IsPathRootedOnNetworkDrive(string path)
		{
			if (path[1] == Path.VolumeSeparatorChar)
			{
				var drive = new DriveInfo(path[0].ToString());
				if (drive.IsReady == false)
					throw new IOException("Path is rooted on a drive that is not ready.");
				return drive.DriveType == DriveType.Network;
			}
			return false;
		}

		public static string GetMappedNetworkPathForDrive(string path)
		{
			if (!IsPathRootedOnNetworkDrive(path))
				throw new IOException("Path is not rooted on a mapped network drive.");

			string driveMappedPath = Win32Helpers.GetNetworkPathForDrive(path[0]);
			string pathWithoutRoot = path.Substring(2).TrimStart('\\');
			
			return Path.Combine(driveMappedPath, pathWithoutRoot);
		}

		public static bool IsPathUnc(string path)
		{
			return path.StartsWith(@"\\");
		}

		public static bool IsPathDfs(string path)
		{
			IntPtr pData;
			uint result = Win32Methods.NetDfsGetClientInfo(path, null, null, 1, out pData);
			if (result == Win32Methods.ERROR_SUCCESS)
			{
				Win32Methods.NetApiBufferFree(pData);
				return true;
			}
			if (result == Win32Methods.NERR_DfsNoSuchServer ||
				result == Win32Methods.NERR_DfsNoSuchShare  ||
				result == Win32Methods.NERR_DfsNoSuchVolume)
				return false;

			throw new Win32Exception();
		}

		public static string ResolveDfsPath(string path, Action<string> notifyGet = null)
		{
			Store store;

			if (notifyGet != null) notifyGet(path);

			while ((store = GetFirstOnlineStore(path)) != null)
			{
				path = path.ToLower().Replace(store.StoredPath.ToLower(), store.ServerPath);

				if (store.StoredPath.TrimEnd('\\') == store.ServerPath.TrimEnd('\\'))
					break;

				if (notifyGet != null) notifyGet(path);
			}

			path = Path.GetFullPath(path);

			return path;
		}

		private static Store GetFirstOnlineStore(string path)
		{
			string fullPath = Path.GetFullPath(path);

			if (File.Exists(fullPath))
			{
				fullPath = Path.GetDirectoryName(fullPath);
			}
			else if (!Directory.Exists(fullPath))
			{
				throw new DirectoryNotFoundException();
			}

			return GetFirstOnlineStore(new DirectoryInfo(fullPath));
		}

		private static Store GetFirstOnlineStore(DirectoryInfo directory)
		{
			if (directory == null)
				throw new ArgumentNullException("directory");

			if (!directory.Exists)
				throw new DirectoryNotFoundException();

			IntPtr pData;
			uint result = Win32Methods.NetDfsGetClientInfo(directory.FullName, null, null, 3, out pData);
			if (result != 0)
				return null;

			try
			{
				var info = (DFS_INFO_3)Marshal.PtrToStructure(pData, typeof(DFS_INFO_3));
				if (info.NumberOfStorages == 0)
					return null;

				var stores = Enumerable.Range(0, (int)info.NumberOfStorages).Select(i =>
				{
					IntPtr pStorage = new IntPtr(info.Storage.ToInt64() + i * Marshal.SizeOf(typeof(DFS_STORAGE_INFO)));
					return (DFS_STORAGE_INFO)Marshal.PtrToStructure(pStorage, typeof(DFS_STORAGE_INFO));
				}).ToList();

				DFS_STORAGE_INFO theStore = stores.FirstOrDefault(s => (s.State & Win32Methods.DFS_STORAGE_STATE_ACTIVE) == Win32Methods.DFS_STORAGE_STATE_ACTIVE);
				if (theStore.ServerName == null)
					theStore = stores.FirstOrDefault(s => (s.State & Win32Methods.DFS_STORAGE_STATE_ONLINE) == Win32Methods.DFS_STORAGE_STATE_ONLINE);
				if (theStore.ServerName == null)
					return null;

				string storedPath = info.EntryPath;
				if (!storedPath.StartsWith(@"\\"))
					storedPath = @"\\" + storedPath.TrimStart('\\');
				return new Store(storedPath, @"\\" + Path.Combine(theStore.ServerName, theStore.ShareName));
			}
			finally
			{
				Win32Methods.NetApiBufferFree(pData);
			}
		}

        public static string[] GetSiteNames(string directoryServer, IList<IPAddress> addresses)
        {
            var socketAddresses = new SOCKET_ADDRESS[addresses.Count];
            var sockaddr_in = new sockaddr_in();
            sockaddr_in.sin_family = (short)AddressFamily.InterNetwork;
            sockaddr_in.sin_port = 0;
            sockaddr_in.sin_zero = 0;

            IntPtr sockaddr_ins = Marshal.AllocHGlobal(addresses.Count * Marshal.SizeOf(sockaddr_in));
            try
            {
                var sockaddr_in_length = Marshal.SizeOf(sockaddr_in);
                for (int i = 0; i < addresses.Count; i++)
                {
                    var ipBytes = addresses[i].GetAddressBytes();
                    var ip = (uint)ipBytes [3] << 24;
                    ip += (uint)ipBytes [2] << 16;
                    ip += (uint)ipBytes [1] <<8;
                    ip += (uint)ipBytes [0];
                    sockaddr_in.sin_addr = ip;

                    var thisSockaddr_in = sockaddr_ins + (i * sockaddr_in_length);
                    Marshal.StructureToPtr(sockaddr_in, thisSockaddr_in, false);
                    socketAddresses[i].iSockaddrLength = (uint)sockaddr_in_length;
                    socketAddresses[i].lpSockaddr = thisSockaddr_in;
                }

                IntPtr result;
                string[] resultStrings;

                var returnValue = Win32Methods.DsAddressToSiteNames(directoryServer, (uint)addresses.Count, socketAddresses, out result);
                if (returnValue != 0)
                    throw new Win32Exception((int)returnValue);

                try
                {
                    resultStrings = new string[addresses.Count];
                    for (int i = 0; i < addresses.Count; i++)
                    {
                        IntPtr iStringPtr = Marshal.ReadIntPtr(result, i * IntPtr.Size);
                        resultStrings[i] = Marshal.PtrToStringUni(iStringPtr);
                    }
                }
                finally
                {
                    Win32Methods.NetApiBufferFree(result);
                }

                return resultStrings;
            }
            finally
            {
                Marshal.FreeHGlobal(sockaddr_ins);
            }
        }
	}

	class Store
	{
		public Store(string storedPath, string serverPath)
		{
			StoredPath = storedPath;
			ServerPath = serverPath;
		}

		public readonly string StoredPath;
		public readonly string ServerPath;
	}
}
