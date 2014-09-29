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
#pragma once

#include <Windows.h>

using namespace System::Runtime::InteropServices;
using namespace System::Net;

public enum struct MgMIB_TCP_STATE : DWORD {
    MIB_TCP_STATE_CLOSED     =  1,
    MIB_TCP_STATE_LISTEN     =  2,
    MIB_TCP_STATE_SYN_SENT   =  3,
    MIB_TCP_STATE_SYN_RCVD   =  4,
    MIB_TCP_STATE_ESTAB      =  5,
    MIB_TCP_STATE_FIN_WAIT1  =  6,
    MIB_TCP_STATE_FIN_WAIT2  =  7,
    MIB_TCP_STATE_CLOSE_WAIT =  8,
    MIB_TCP_STATE_CLOSING    =  9,
    MIB_TCP_STATE_LAST_ACK   = 10,
    MIB_TCP_STATE_TIME_WAIT  = 11,
    MIB_TCP_STATE_DELETE_TCB = 12,
};

[StructLayout(LayoutKind::Sequential)] 
public value struct MgMIB_TCPROW {
    MgMIB_TCP_STATE State;
    DWORD dwLocalAddr;
    DWORD dwLocalPort;
    DWORD dwRemoteAddr;
    DWORD dwRemotePort;

	property IPAddress^ LocalAddress {
		IPAddress^ get() { return gcnew IPAddress(dwLocalAddr); }
	}

	property IPAddress^ RemoteAddress {
		IPAddress^ get() { return gcnew IPAddress(dwRemoteAddr); }
	}

	property unsigned short LocalPort {
		unsigned short get() { return IPAddress::NetworkToHostOrder((short)dwLocalPort); }
	}

	property unsigned short RemotePort {
		unsigned short get() { return IPAddress::NetworkToHostOrder((short)dwRemotePort); }
	}

	virtual System::String^ ToString() override
	{
		return State.ToString() + " " + LocalAddress + ":" + LocalPort + " " + RemoteAddress + ":" + RemotePort; 
	}
};