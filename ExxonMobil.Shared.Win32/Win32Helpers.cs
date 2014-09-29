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
using System.ComponentModel;
using System.IO;

namespace ExxonMobil.Shared.Win32
{
	public static class Win32Helpers
	{
		public static string GetNetworkPathForDrive(char driveLetter)
		{
			var path = new StringBuilder(512);
			uint size = (uint)path.Capacity;
			uint result = Win32Methods.WNetGetConnectionW(driveLetter + ":", path, ref size);
			if (result != Win32Methods.ERROR_SUCCESS)
				throw new IOException("Cannot get network path for drive. '" + driveLetter + ":' does not exist.", new Win32Exception());

			return path.ToString();
		}

		public static void EnablePrivilege(string privilegeName)
		{
			SetPrivilege(privilegeName, Win32Methods.SE_PRIVILEGE_ENABLED);
		}

		public static void DisablePrivilege(string privilegeName)
		{
			SetPrivilege(privilegeName, 0);
		}

		private static void SetPrivilege(string privilegeName, int attrib)
		{
			IntPtr handle = IntPtr.Zero;
			LUID luid = default(LUID);
			IntPtr currentProcess = Win32Methods.GetCurrentProcess();
			if (!Win32Methods.OpenProcessToken(new HandleRef(null, currentProcess), Win32Methods.TOKEN_ADJUST_PRIVILEGES, out handle))
			{
				throw new Win32Exception();
			}
			try
			{
				if (!Win32Methods.LookupPrivilegeValue(null, privilegeName, out luid))
				{
					throw new Win32Exception();
				}
				TokenPrivileges tokenPrivileges = new TokenPrivileges();
				tokenPrivileges.Luid = luid;
				tokenPrivileges.Attributes = attrib;
				Win32Methods.AdjustTokenPrivileges(new HandleRef(null, handle), false, tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
				if (Marshal.GetLastWin32Error() != 0)
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				Win32Methods.CloseHandle(new HandleRef(null, handle));
			}
		}
	}
}
