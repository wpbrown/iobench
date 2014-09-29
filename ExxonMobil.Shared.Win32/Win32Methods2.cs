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
using ExxonMobil.Shared.Win32.SafeHandles;

namespace ExxonMobil.Shared.Win32
{
	public static class Win32Methods2
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool OpenProcessToken(SafeProcessHandle ProcessHandle, uint DesiredAccess, out SafeTokenHandle TokenHandle);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool DuplicateToken(SafeTokenHandle ExistingTokenHandle, SecurityImpersonationLevel ImpersonationLevel, out SafeTokenHandle DuplicateTokenHandle);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool DuplicateTokenEx(SafeTokenHandle hExistingToken, uint dwDesiredAccess, SecurityAttributes lpTokenAttributes, SecurityImpersonationLevel ImpersonationLevel, TokenType TokenType, out SafeTokenHandle phNewToken);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern Boolean SetTokenInformation(SafeTokenHandle TokenHandle, TokenInformationClass TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength);

		[DllImport("userenv.dll", SetLastError = true)]
		public static extern bool CreateEnvironmentBlock(out SafeEnvBlockHandle lpEnvironment, SafeTokenHandle hToken, bool bInherit);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, SecurityAttributes lpProcessAttributes, SecurityAttributes lpThreadAttributes, 
												bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool CreateProcessAsUser(SafeTokenHandle hToken, string lpApplicationName, string lpCommandLine, SecurityAttributes lpProcessAttributes, SecurityAttributes lpThreadAttributes,
			                                          bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool CreateProcessWithLogonW(string lpUsername, string lpDomain, string lpPassword, uint dwLogonFlags, string lpApplicationName, string lpCommandLine, 
													      uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, uint dwLogonType, uint dwLogonProvider, out SafeTokenHandle phToken);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool ConvertStringSidToSid(string StringSid, out SafeLocalAllocHandle psid);

		[DllImport("advapi32.dll")]
		public static extern uint GetLengthSid(SafeLocalAllocHandle pSid);
		
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern SafeProcessHandle GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetExitCodeProcess(SafeProcessHandle hProcess, out int lpExitCode);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool TerminateProcess(SafeProcessHandle hProcess, int uExitCode);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int ResumeThread(SafeThreadHandle hThread);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern SafeJobHandle CreateJobObject(SecurityAttributes lpJobAttributes, string name);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool AssignProcessToJobObject(SafeJobHandle hJob, SafeProcessHandle hProcess);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool TerminateJobObject(SafeJobHandle hJob, int uExitCode);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetStdHandle(int whichHandle);

		[DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool LoadUserProfile(SafeTokenHandle hToken, PROFILEINFO lpProfileInfo);

		[DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool UnloadUserProfile(SafeTokenHandle hToken, IntPtr hProfile);

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern int WaitForSingleObject(SafeHandle handle, int timeout);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr WindowFromPoint(POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr ChildWindowFromPoint(IntPtr hWnd, POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr ChildWindowFromPointEx(IntPtr hWnd, POINT lpPoint, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		public const uint CWP_ALL = 0;
        public const uint CWP_SKIPDISABLED = 0x0002;
        public const uint CWP_SKIPINVISIBLE = 0x0001;
        public const uint CWP_SKIPTRANSPARENT = 0x0004;

		public const uint DELETE                           = 0x00010000;
		public const uint READ_CONTROL                     = 0x00020000;
		public const uint WRITE_DAC                        = 0x00040000;
		public const uint WRITE_OWNER                      = 0x00080000;
		public const uint SYNCHRONIZE                      = 0x00100000;

		public const uint STANDARD_RIGHTS_REQUIRED         = 0x000F0000;

		public const uint STANDARD_RIGHTS_READ             = READ_CONTROL;
		public const uint STANDARD_RIGHTS_WRITE            = READ_CONTROL;
		public const uint STANDARD_RIGHTS_EXECUTE          = READ_CONTROL;

		public const uint STANDARD_RIGHTS_ALL              = 0x001F0000;

		public const uint SPECIFIC_RIGHTS_ALL              = 0x0000FFFF;

		public const uint TOKEN_ASSIGN_PRIMARY     = 0x0001;
		public const uint TOKEN_DUPLICATE          = 0x0002;
		public const uint TOKEN_IMPERSONATE        = 0x0004;
		public const uint TOKEN_QUERY              = 0x0008;
		public const uint TOKEN_QUERY_SOURCE       = 0x0010;
		public const uint TOKEN_ADJUST_PRIVILEGES  = 0x0020;
		public const uint TOKEN_ADJUST_GROUPS      = 0x0040;
		public const uint TOKEN_ADJUST_DEFAULT     = 0x0080;
		public const uint TOKEN_ADJUST_SESSIONID   = 0x0100;

		public const uint TOKEN_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED |
											 TOKEN_ASSIGN_PRIMARY |
											 TOKEN_DUPLICATE |
											 TOKEN_IMPERSONATE |
											 TOKEN_QUERY |
											 TOKEN_QUERY_SOURCE |
											 TOKEN_ADJUST_PRIVILEGES |
											 TOKEN_ADJUST_GROUPS |
											 TOKEN_ADJUST_DEFAULT |
											 TOKEN_ADJUST_SESSIONID;  

		public const uint TOKEN_READ    = STANDARD_RIGHTS_READ | 
										  TOKEN_QUERY;
		public const uint TOKEN_WRITE   = STANDARD_RIGHTS_WRITE |
										  TOKEN_ADJUST_PRIVILEGES |
										  TOKEN_ADJUST_GROUPS |
										  TOKEN_ADJUST_DEFAULT;
		public const uint TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;

		public const uint CREATE_SUSPENDED = 0x00000004;
		public const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
		public const uint CREATE_NEW_CONSOLE         = 0x00000010;
		public const uint STILL_ACTIVE = 259;

		public const uint LOGON32_LOGON_INTERACTIVE       = 2;
		public const uint LOGON32_LOGON_NETWORK           = 3;
		public const uint LOGON32_LOGON_BATCH             = 4;
		public const uint LOGON32_LOGON_SERVICE           = 5;
		public const uint LOGON32_LOGON_UNLOCK            = 7;
		public const uint LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
		public const uint LOGON32_LOGON_NEW_CREDENTIALS   = 9;
		public const uint LOGON32_PROVIDER_DEFAULT        = 0;

		public const uint SE_GROUP_INTEGRITY = 0x00000020;
		public const uint STATUS_CONTROL_C_EXIT = 0xC000013A;

		public const uint STARTF_USESTDHANDLES = 0x00000100;

		public const int STD_INPUT_HANDLE = -10;
		public const int STD_OUTPUT_HANDLE = -11;
		public const int STD_ERROR_HANDLE = -12;

		public const int WAIT_TIMEOUT = 0x00000102;
		public const int WAIT_FAILED = unchecked((int)0xFFFFFFFF);

        public const int WM_CLOSE = 0x0010;

	}
}
