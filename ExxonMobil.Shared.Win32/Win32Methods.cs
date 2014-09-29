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
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace ExxonMobil.Shared.Win32
{
	public static class Win32Methods
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(
			string lpFileName,
			Win32FileAccess dwDesiredAccess,
			Win32FileShare dwShareMode,
			IntPtr securityAttrs,
			Win32FileCreationDisposition dwCreationDisposition,
			Win32FileAttributes dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, IntPtr lpNewFilePointer, SeekOrigin origin);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetEndOfFile(SafeFileHandle hFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetFileValidData(SafeFileHandle hFile, long ValidDataLength);

		[DllImport("kernel32.dll")]
		public static extern bool GetFileSizeEx(SafeFileHandle hFile, out long lpFileSize);

		[DllImport("kernel32.dll")]
		public static extern bool QueryPerformanceFrequency(out long value);

		[DllImport("kernel32.dll")]
		public static extern bool QueryPerformanceCounter(out long value);

		[DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr StrFormatByteSize(ulong size, StringBuilder buffer, int bufferSize);

		[DllImport("advapi32.dll")]
		public static extern bool InitiateSystemShutdown([MarshalAs(UnmanagedType.LPStr)] string lpMachinename, [MarshalAs(UnmanagedType.LPStr)] string lpMessage, Int32 dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

		[DllImport("kernel32.dll")]
		public static extern uint WTSGetActiveConsoleSessionId();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetGuiResources(IntPtr hProcess, Win32GuiResources uiFlags);

		[DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
		public static extern uint NetDfsGetClientInfo(string DfsEntryPath, string ServerName, string ShareName, uint Level, out IntPtr Buffer);

		[DllImport("Netapi32.dll")]
		public static extern uint NetApiBufferFree(IntPtr Buffer);

        [DllImport("Netapi32.dll", CharSet = CharSet.Auto)]
        public static extern uint DsAddressToSiteNames(string ComputerName, uint EntryCount, SOCKET_ADDRESS[] SocketAddresses, out IntPtr SiteNames);

		[DllImportAttribute("mpr.dll", CharSet = CharSet.Unicode)]
		public static extern uint WNetGetConnectionW([InAttribute()] string lpLocalName, StringBuilder lpRemoteName, ref uint lpnLength);

		[DllImport("Wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool WTSQuerySessionInformationW(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr buffer, out int bytesReturned);

		[DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern Int32 WTSEnumerateSessions(IntPtr hServer, int reserved, int version, out IntPtr sessionInfo, out int count);

		[DllImport("wtsapi32.dll")]
		public static extern void WTSFreeMemory(IntPtr memory);

		#region Priviliges
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool AdjustTokenPrivileges(HandleRef TokenHandle, bool DisableAllPrivileges, TokenPrivileges NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool OpenProcessToken(HandleRef ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

		[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool LookupPrivilegeValue([MarshalAs(UnmanagedType.LPTStr)] string lpSystemName, [MarshalAs(UnmanagedType.LPTStr)] string lpName, out LUID lpLuid);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FlushFileBuffers(SafeFileHandle handle);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
		public static extern bool CloseHandle(HandleRef handle);

		public const int SE_PRIVILEGE_ENABLED = 0x00000002;
		public const int TOKEN_QUERY = 0x00000008;
		public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
		public const int TOKEN_DUPLICATE = 0x00000002;


		public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
		public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
		public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
		public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
		public const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";
		public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
		public const string SE_TCB_NAME = "SeTcbPrivilege";
		public const string SE_SECURITY_NAME = "SeSecurityPrivilege";
		public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
		public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
		public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
		public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
		public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
		public const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
		public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
		public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
		public const string SE_BACKUP_NAME = "SeBackupPrivilege";
		public const string SE_RESTORE_NAME = "SeRestorePrivilege";
		public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
		public const string SE_DEBUG_NAME = "SeDebugPrivilege";
		public const string SE_AUDIT_NAME = "SeAuditPrivilege";
		public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
		public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
		public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
		public const string SE_UNDOCK_NAME = "SeUndockPrivilege";
		public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
		public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
		public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
		public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
		public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
		public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
		public const string SE_RELABEL_NAME = "SeRelabelPrivilege";
		public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
		public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
		public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
		#endregion Priviliges

		public const uint DFS_STORAGE_STATE_ONLINE = 0x00000002;
		public const uint DFS_STORAGE_STATE_ACTIVE = 0x00000004;

		public const uint ERROR_SUCCESS = 0x00000000;
		public const uint NERR_DfsNoSuchVolume = 0x00000A66;
		public const uint NERR_DfsNoSuchShare  = 0x00000A69;
		public const uint NERR_DfsNoSuchServer = 0x00000A71;
	}
}
