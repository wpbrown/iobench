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

// This is the main DLL file.

#include "stdafx.h"

#include "TcpHelper.h"
#include "MgTcpestats.h"
#include "MgTcpmib.h"

using namespace System::Runtime::InteropServices;

namespace ExxonMobil { namespace Shared { namespace IPHelper {

IEnumerable<MgMIB_TCPROW>^ TcpHelper::GetTcpTable()
{
	PMIB_TCPTABLE tcpTable = NULL;
	PMIB_TCPROW tcpRowIt;

    DWORD status;
	DWORD size = 0;

    status = ::GetTcpTable(tcpTable, &size, TRUE);
    if (status != ERROR_INSUFFICIENT_BUFFER) {
        throw gcnew Exception("get buffer size for table failed");
    }

    tcpTable = (PMIB_TCPTABLE) HeapAlloc(GetProcessHeap(), 0, size);
    if (tcpTable == NULL) {
        throw gcnew OutOfMemoryException("tcptable");
    }

	try
	{
		status = ::GetTcpTable(tcpTable, &size, TRUE);
		if (status != ERROR_SUCCESS) 
			throw gcnew Exception("gettcptable fail");

		List<MgMIB_TCPROW>^ list = gcnew List<MgMIB_TCPROW>(tcpTable->dwNumEntries);
		for (DWORD i = 0; i < tcpTable->dwNumEntries; i++) {
			tcpRowIt = &tcpTable->table[i];
			list->Add((MgMIB_TCPROW)Marshal::PtrToStructure((IntPtr)tcpRowIt, MgMIB_TCPROW::typeid));
		}

		return list;
	}
	finally
	{
		HeapFree(GetProcessHeap(), 0, tcpTable);
	}
}

}}}
