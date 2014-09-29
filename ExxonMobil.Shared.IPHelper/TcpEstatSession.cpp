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
#include "StdAfx.h"
#include "TcpEstatSession.h"

using namespace System;

namespace ExxonMobil { namespace Shared { namespace IPHelper {

TcpEstatSession::TcpEstatSession(MgMIB_TCPROW tcpRow) :
	tcpRow(tcpRow)
{
	enabledTypes = new std::bitset<TcpConnectionEstatsMaximum>();
}

TcpEstatSession::~TcpEstatSession()
{
	DisableAll();
}

TcpEstatSession::!TcpEstatSession()
{
	DisableAll();
	delete enabledTypes;
	enabledTypes = nullptr;
}

void TcpEstatSession::EnableAll()
{
	if (enabledTypes->all())
		return;

	for (size_t i = 0; i < TcpConnectionEstatsMaximum; i++)
			Enable((MgTCP_ESTATS_TYPE)i); 
}

void TcpEstatSession::DisableAll()
{
	if (enabledTypes->none())
		return;

	for (size_t i = 0; i < TcpConnectionEstatsMaximum; i++)
		Disable((MgTCP_ESTATS_TYPE)i); 
}

void TcpEstatSession::Enable(MgTCP_ESTATS_TYPE statType)
{
	if (!enabledTypes->test(static_cast<size_t>(statType))) {
		_ToggleEstat(statType, true);
		enabledTypes->set(static_cast<size_t>(statType));
	}
}

void TcpEstatSession::Disable(MgTCP_ESTATS_TYPE statType)
{
	if (enabledTypes->test(static_cast<size_t>(statType)))  {
		_ToggleEstat(statType, false);
		enabledTypes->reset(static_cast<size_t>(statType));
	}
}

void TcpEstatSession::Update(MgTCP_ESTATS_TYPE statType)
{
	if (!enabledTypes->test(static_cast<size_t>(statType)))
		throw gcnew InvalidOperationException("Cannot update estat type that is not enabled");
	_GetEstats(statType);
}

void TcpEstatSession::UpdateAllEnabled()
{
	for (size_t i = 0; i < TcpConnectionEstatsMaximum; i++) {
		if (enabledTypes->test(i))
			_GetEstats((MgTCP_ESTATS_TYPE)i); 
	}
}

void TcpEstatSession::_ToggleEstat(MgTCP_ESTATS_TYPE statType, bool enable)
{
	TCP_ESTATS_TYPE type = static_cast<TCP_ESTATS_TYPE>(statType);
    TCP_BOOLEAN_OPTIONAL operation =
        enable ? TcpBoolOptEnabled : TcpBoolOptDisabled;
    ULONG status, size = 0;
    PUCHAR rw = NULL;
    TCP_ESTATS_DATA_RW_v0 dataRw;
    TCP_ESTATS_SND_CONG_RW_v0 sndRw;
    TCP_ESTATS_PATH_RW_v0 pathRw;
    TCP_ESTATS_SEND_BUFF_RW_v0 sendBuffRw;
    TCP_ESTATS_REC_RW_v0 recRw;
    TCP_ESTATS_OBS_REC_RW_v0 obsRecRw;
    TCP_ESTATS_BANDWIDTH_RW_v0 bandwidthRw;
    TCP_ESTATS_FINE_RTT_RW_v0 fineRttRw;

    switch (type) {
		case TcpConnectionEstatsData:
			dataRw.EnableCollection = enable;
			rw = (PUCHAR) & dataRw;
			size = sizeof (TCP_ESTATS_DATA_RW_v0);
			break;

		case TcpConnectionEstatsSndCong:
			sndRw.EnableCollection = enable;
			rw = (PUCHAR) & sndRw;
			size = sizeof (TCP_ESTATS_SND_CONG_RW_v0);
			break;

		case TcpConnectionEstatsPath:
			pathRw.EnableCollection = enable;
			rw = (PUCHAR) & pathRw;
			size = sizeof (TCP_ESTATS_PATH_RW_v0);
			break;

		case TcpConnectionEstatsSendBuff:
			sendBuffRw.EnableCollection = enable;
			rw = (PUCHAR) & sendBuffRw;
			size = sizeof (TCP_ESTATS_SEND_BUFF_RW_v0);
			break;

		case TcpConnectionEstatsRec:
			recRw.EnableCollection = enable;
			rw = (PUCHAR) & recRw;
			size = sizeof (TCP_ESTATS_REC_RW_v0);
			break;

		case TcpConnectionEstatsObsRec:
			obsRecRw.EnableCollection = enable;
			rw = (PUCHAR) & obsRecRw;
			size = sizeof (TCP_ESTATS_OBS_REC_RW_v0);
			break;

		case TcpConnectionEstatsBandwidth:
			bandwidthRw.EnableCollectionInbound = operation;
			bandwidthRw.EnableCollectionOutbound = operation;
			rw = (PUCHAR) & bandwidthRw;
			size = sizeof (TCP_ESTATS_BANDWIDTH_RW_v0);
			break;

		case TcpConnectionEstatsFineRtt:
			fineRttRw.EnableCollection = enable;
			rw = (PUCHAR) & fineRttRw;
			size = sizeof (TCP_ESTATS_FINE_RTT_RW_v0);
			break;

		default:
			return;
			break;
    }

	pin_ptr<MgMIB_TCPROW> row = &this->tcpRow;
    status = SetPerTcpConnectionEStats((PMIB_TCPROW) row, type, rw, 0, size, 0);

    if (status == ERROR_ACCESS_DENIED) {
		throw gcnew System::Security::SecurityException("SetPerTcpConnectionEStats");
	} else if (status != NO_ERROR) {
		// Don't throw if disabling on a connection that closed.
		if (!enable && status == ERROR_NOT_FOUND)
			return;

		throw gcnew System::ComponentModel::Win32Exception(status);
    }
}

void TcpEstatSession::_GetEstats(MgTCP_ESTATS_TYPE statType)
{
	TCP_ESTATS_TYPE type = static_cast<TCP_ESTATS_TYPE>(statType);
    ULONG rosSize = 0, rodSize = 0;
    ULONG winStatus;
    PUCHAR ros = NULL, rod = NULL;

	pin_ptr<MgTCP_ESTATS_SYN_OPTS_ROS_v0> ppSynOptsRos;
    pin_ptr<MgTCP_ESTATS_DATA_ROD_v0> ppDataRod;
    pin_ptr<MgTCP_ESTATS_SND_CONG_ROD_v0> ppSndCongRod;
	pin_ptr<MgTCP_ESTATS_SND_CONG_ROS_v0> ppSndCongRos;
    pin_ptr<MgTCP_ESTATS_PATH_ROD_v0> ppPathRod;
    pin_ptr<MgTCP_ESTATS_SEND_BUFF_ROD_v0> ppSendBuffRod;
    pin_ptr<MgTCP_ESTATS_REC_ROD_v0> ppRecRod;
    pin_ptr<MgTCP_ESTATS_OBS_REC_ROD_v0> ppObsRecRod;
    pin_ptr<MgTCP_ESTATS_BANDWIDTH_ROD_v0> ppBandwidthRod;
    pin_ptr<MgTCP_ESTATS_FINE_RTT_ROD_v0> ppFineRttRod;

    switch (type) {
		case TcpConnectionEstatsSynOpts:
			ppSynOptsRos = &SynOptsRos;
			rosSize = sizeof (MgTCP_ESTATS_SYN_OPTS_ROS_v0);
			ros = (PUCHAR)ppSynOptsRos;
			break;

		case TcpConnectionEstatsData:
			ppDataRod = &DataRod;
			rodSize = sizeof (MgTCP_ESTATS_DATA_ROD_v0);
			rod = (PUCHAR)ppDataRod;
			break;

		case TcpConnectionEstatsSndCong:
			ppSndCongRos = &SndCongRos;
			ppSndCongRod = &SndCongRod;
			rosSize = sizeof (MgTCP_ESTATS_SND_CONG_ROS_v0);
			rodSize = sizeof (MgTCP_ESTATS_SND_CONG_ROD_v0);
			ros = (PUCHAR)ppSndCongRos;
			rod = (PUCHAR)ppSndCongRod;
			break;

		case TcpConnectionEstatsPath:
			ppPathRod = &PathRod;
			rodSize = sizeof (MgTCP_ESTATS_PATH_ROD_v0);
			rod = (PUCHAR)ppPathRod;
			break;

		case TcpConnectionEstatsSendBuff:
			ppSendBuffRod = &SendBuffRod;
			rodSize = sizeof (MgTCP_ESTATS_SEND_BUFF_ROD_v0);
			rod = (PUCHAR)ppSendBuffRod;
			break;

		case TcpConnectionEstatsRec:
			ppRecRod = &RecRod;
			rodSize = sizeof (MgTCP_ESTATS_REC_ROD_v0);
			rod = (PUCHAR)ppRecRod;
			break;

		case TcpConnectionEstatsObsRec:
			ppObsRecRod = &ObsRecRod;
			rodSize = sizeof (MgTCP_ESTATS_OBS_REC_ROD_v0);
			rod = (PUCHAR)ppObsRecRod;
			break;

		case TcpConnectionEstatsBandwidth:
			ppBandwidthRod = &BandwidthRod;
			rodSize = sizeof (MgTCP_ESTATS_BANDWIDTH_ROD_v0);
			rod = (PUCHAR)ppBandwidthRod;
			break;

		case TcpConnectionEstatsFineRtt:
			ppFineRttRod = &FineRttRod;
			rodSize = sizeof (MgTCP_ESTATS_FINE_RTT_ROD_v0);
			rod = (PUCHAR)ppFineRttRod;
			break;

		default:
			throw gcnew Exception("Cannot get estat type");
			break;
    }

    if (rosSize != 0)
        memset(ros,0, rosSize);

    if (rodSize != 0)
		memset(rod,0, rodSize);

	pin_ptr<MgMIB_TCPROW> row = &this->tcpRow;
    winStatus = GetPerTcpConnectionEStats((PMIB_TCPROW) row,
											type,
											NULL, 0, 0,
											ros, 0, rosSize, rod, 0, rodSize);
    
    if (winStatus != NO_ERROR) {
        throw gcnew System::ComponentModel::Win32Exception(winStatus);
    }
}

}}}
