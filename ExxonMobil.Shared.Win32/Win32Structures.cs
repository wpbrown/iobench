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
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace ExxonMobil.Shared.Win32
{
	[StructLayout(LayoutKind.Sequential)]
	public class TokenPrivileges
	{
		public int PrivilegeCount = 1;
		public LUID Luid;
		public int Attributes;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LUID
	{
		public int LowPart;
		public int HighPart;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryStatusEx
	{
		public uint dwLength;
		public uint dwMemoryLoad;
		public ulong ullTotalPhys;
		public ulong ullAvailPhys;
		public ulong ullTotalPageFile;
		public ulong ullAvailPageFile;
		public ulong ullTotalVirtual;
		public ulong ullAvailVirtual;
		public ulong ullAvailExtendedVirtual;

		public void Initialize()
		{
			dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct DFS_INFO_3
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		public string EntryPath;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Comment;
		public uint State;
		public uint NumberOfStorages;
		public IntPtr Storage;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct DFS_STORAGE_INFO
	{
		public uint State;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string ServerName;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string ShareName;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WTS_SESSION_INFO
	{
		public int SessionID;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string WinStationName;
		public WtsConnectionState State;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WTSINFO
	{
		public WtsConnectionState State;
		public UInt32 SessionId;
		public UInt32 IncomingBytes;
		public UInt32 OutgoingBytes;
		public UInt32 IncomingFrames;
		public UInt32 OutgoingFrames;
		public UInt32 IncomingCompressedBytes;
		public UInt32 OutgoingCompressedBytes;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public String WinStationName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
		public String Domain;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20 + 1)]
		public String UserName;
		public Int64 ConnectTime;
		public Int64 DisconnectTime;
		public Int64 LastInputTime;
		public Int64 LogonTime;
		public Int64 CurrentTime;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WTSCLIENT {
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20 + 1)]
		public string ClientName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17 + 1)]
		public string Domain;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20 + 1)]
		public string UserName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260  + 1)]
		public string WorkDirectory;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260 + 1)]
		public string InitialProgram;
		public byte EncryptionLevel;       // security level of encryption pd
		public UInt32 ClientAddressFamily;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30 + 1)]
		public UInt16[] ClientAddress;
		public UInt16 HRes;
		public UInt16 VRes;
		public UInt16 ColorDepth;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260 + 1)]
		public string ClientDirectory;
		public UInt32 ClientBuildNumber;
		public UInt32 ClientHardwareId;    // client software serial number
		public UInt16 ClientProductId;     // client software product id
		public UInt16 OutBufCountHost;     // number of outbufs on host
		public UInt16 OutBufCountClient;   // number of outbufs on client
		public UInt16 OutBufLength;        // length of outbufs in bytes
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260 + 1)]
		public string DeviceId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WTS_CLIENT_ADDRESS {
		public UInt32 AddressFamily;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		public byte[] Address;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SecurityAttributes
	{
		public int nLength;
		public IntPtr lpSecurityDescriptor;
		public int bInheritHandle;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct STARTUPINFO
	{
		public Int32 cb;
		public string lpReserved;
		public string lpDesktop;
		public string lpTitle;
		public Int32 dwX;
		public Int32 dwY;
		public Int32 dwXSize;
		public Int32 dwYSize;
		public Int32 dwXCountChars;
		public Int32 dwYCountChars;
		public Int32 dwFillAttribute;
		public Int32 dwFlags;
		public Int16 wShowWindow;
		public Int16 cbReserved2;
		public IntPtr lpReserved2;
		public IntPtr hStdInput;
		public IntPtr hStdOutput;
		public IntPtr hStdError;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;
		public IntPtr hThread;
		public int dwProcessId;
		public int dwThreadId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SID_AND_ATTRIBUTES
	{
		public IntPtr Sid;
		public uint Attributes;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_MANDATORY_LABEL
	{
		public SID_AND_ATTRIBUTES Label;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class PROFILEINFO
	{
		public int dwSize;
		public int dwFlags;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpUserName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpProfilePath;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpDefaultPath;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpServerName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpPolicyPath;
		public IntPtr hProfile;

		public PROFILEINFO(string userName)
		{
			dwSize = Marshal.SizeOf(this);
			lpUserName = userName;
		}
	}

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SOCKET_ADDRESS
    {
        public IntPtr lpSockaddr;
        public uint iSockaddrLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct sockaddr_in
    {
        public short sin_family;
        public ushort sin_port;
        public uint sin_addr;
        public ulong sin_zero;
    }
}
