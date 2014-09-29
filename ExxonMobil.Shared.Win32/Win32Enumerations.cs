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

namespace ExxonMobil.Shared.Win32
{
    [Flags]
    public enum Win32FileAttributes : uint
    {
        Readonly = 0x00000001,
        Hidden = 0x00000002,
        System = 0x00000004,
        Directory = 0x00000010,
        Archive = 0x00000020,
        Device = 0x00000040,
        Normal = 0x00000080,
        Temporary = 0x00000100,
        SparseFile = 0x00000200,
        ReparsePoint = 0x00000400,
        Compressed = 0x00000800,
        Offline = 0x00001000,
        NotContentIndexed = 0x00002000,
        Encrypted = 0x00004000,
        WriteThrough = 0x80000000,
        Overlapped = 0x40000000,
        NoBuffering = 0x20000000,
        RandomAccess = 0x10000000,
        SequentialScan = 0x08000000,
        DeleteOnClose = 0x04000000,
        BackupSemantics = 0x02000000,
        PosixSemantics = 0x01000000,
        OpenReparsePoint = 0x00200000,
        OpenNoRecall = 0x00100000,
        FirstPipeInstance = 0x00080000
    }

    [Flags]
	public enum Win32FileAccess : uint
    {
        Delete = 0x10000,
        ReadControl = 0x20000,
        WriteDAC = 0x40000,
        WriteOwner = 0x80000,
        Synchronize = 0x100000,

        StandardRightsRequired = 0xF0000,
        StandardRightsRead = ReadControl,
        StandardRightsWrite = ReadControl,
        StandardRightsExecute = ReadControl,
        StandardRightsAll = 0x1F0000,
        SpecificRightsAll = 0xFFFF,

        AccessSystemSecurity = 0x1000000,   

        MaximumAllowed = 0x2000000,      

        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000
    }

    [Flags]
	public enum Win32FileShare : uint
    {
        None = 0x00000000,
        Read = 0x00000001,
        Write = 0x00000002,
        Delete = 0x00000004
    }

	public enum Win32FileCreationDisposition : uint
    {
        New = 1,
        CreateAlways = 2,
        OpenExisting = 3,
        OpenAlways = 4,
        TruncateExisting = 5
    }

	public enum Win32GuiResources : uint
	{
		GdiObjects = 0,
		GdiObjectsPeak = 2,
		UserObjects = 1,
		UserObjectsPeak = 4
	}

	public enum WTS_INFO_CLASS
	{
		WTSInitialProgram = 0,
		WTSApplicationName = 1,
		WTSWorkingDirectory = 2,
		WTSOEMId = 3,
		WTSSessionId = 4,
		WTSUserName = 5,
		WTSWinStationName = 6,
		WTSDomainName = 7,
		WTSConnectState = 8,
		WTSClientBuildNumber = 9,
		WTSClientName = 10,
		WTSClientDirectory = 11,
		WTSClientProductId = 12,
		WTSClientHardwareId = 13,
		WTSClientAddress = 14,
		WTSClientDisplay = 15,
		WTSClientProtocolType = 16,
		WTSIdleTime = 17,
		WTSLogonTime = 18,
		WTSIncomingBytes = 19,
		WTSOutgoingBytes = 20,
		WTSIncomingFrames = 21,
		WTSOutgoingFrames = 22,
		WTSClientInfo = 23,
		WTSSessionInfo = 24,
		WTSSessionInfoEx = 25,
		WTSConfigInfo = 26,
		WTSValidationInfo = 27,
		WTSSessionAddressV4 = 28,
		WTSIsRemoteSession = 29 
	}

	public enum WtsConnectionState
	{
		Active,
		Connected,
		ConnectQuery,
		Shadow,
		Disconnected,
		Idle,
		Listen,
		Reset,
		Down,
		Init
	}

	public enum SecurityImpersonationLevel
	{
		/// <summary>
		/// The server process cannot obtain identification information about the client, 
		/// and it cannot impersonate the client. It is defined with no value given, and thus, 
		/// by ANSI C rules, defaults to a value of zero. 
		/// </summary>
		SecurityAnonymous = 0,

		/// <summary>
		/// The server process can obtain information about the client, such as security identifiers and privileges, 
		/// but it cannot impersonate the client. This is useful for servers that export their own objects, 
		/// for example, database products that export tables and views. 
		/// Using the retrieved client-security information, the server can make access-validation decisions without 
		/// being able to use other services that are using the client's security context. 
		/// </summary>
		SecurityIdentification = 1,

		/// <summary>
		/// The server process can impersonate the client's security context on its local system. 
		/// The server cannot impersonate the client on remote systems. 
		/// </summary>
		SecurityImpersonation = 2,

		/// <summary>
		/// The server process can impersonate the client's security context on remote systems. 
		/// NOTE: Windows NT:  This impersonation level is not supported.
		/// </summary>
		SecurityDelegation = 3,
	}

	public enum TokenType
	{
		TokenPrimary = 1,
		TokenImpersonation
	}

	public enum TokenInformationClass
	{
		TokenUser = 1,
		TokenGroups,
		TokenPrivileges,
		TokenOwner,
		TokenPrimaryGroup,
		TokenDefaultDacl,
		TokenSource,
		TokenType,
		TokenImpersonationLevel,
		TokenStatistics,
		TokenRestrictedSids,
		TokenSessionId,
		TokenGroupsAndPrivileges,
		TokenSessionReference,
		TokenSandBoxInert,
		TokenAuditPolicy,
		TokenOrigin,
		TokenElevationType,
		TokenLinkedToken,
		TokenElevation,
		TokenHasRestrictions,
		TokenAccessInformation,
		TokenVirtualizationAllowed,
		TokenVirtualizationEnabled,
		TokenIntegrityLevel,
		TokenUIAccess,
		TokenMandatoryPolicy,
		TokenLogonSid,
		TokenIsAppContainer,
		TokenCapabilities,
		TokenAppContainerSid,
		TokenAppContainerNumber,
		TokenUserClaimAttributes,
		TokenDeviceClaimAttributes,
		TokenRestrictedUserClaimAttributes,
		TokenRestrictedDeviceClaimAttributes,
		TokenDeviceGroups,
		TokenRestrictedDeviceGroups,
		TokenSecurityAttributes,
		TokenIsRestricted,
		MaxTokenInfoClass
	} 
}
