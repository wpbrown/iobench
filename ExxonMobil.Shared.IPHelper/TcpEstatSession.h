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

#include "MgTcpestats.h"
#include "MgTcpmib.h"
#include <bitset>

using namespace System;

namespace ExxonMobil { namespace Shared { namespace IPHelper {

public ref class TcpEstatSession
{
public:
	TcpEstatSession(MgMIB_TCPROW tcpRow);
	~TcpEstatSession();
	!TcpEstatSession();

	void EnableAll();
	void DisableAll();
	void Enable(MgTCP_ESTATS_TYPE statType);
	void Disable(MgTCP_ESTATS_TYPE statType);

	void Update(MgTCP_ESTATS_TYPE statType);
	void UpdateAllEnabled();

	MgTCP_ESTATS_SYN_OPTS_ROS_v0 SynOptsRos;
    MgTCP_ESTATS_DATA_ROD_v0 DataRod;
    MgTCP_ESTATS_SND_CONG_ROD_v0 SndCongRod;
	MgTCP_ESTATS_SND_CONG_ROS_v0 SndCongRos;
    MgTCP_ESTATS_PATH_ROD_v0 PathRod;
    MgTCP_ESTATS_SEND_BUFF_ROD_v0 SendBuffRod;
    MgTCP_ESTATS_REC_ROD_v0 RecRod;
    MgTCP_ESTATS_OBS_REC_ROD_v0 ObsRecRod;
    MgTCP_ESTATS_BANDWIDTH_ROD_v0 BandwidthRod;
    MgTCP_ESTATS_FINE_RTT_ROD_v0 FineRttRod;

private:
	void _ToggleEstat(MgTCP_ESTATS_TYPE statType, bool enable);
	void _GetEstats(MgTCP_ESTATS_TYPE statType);

	std::bitset<TcpConnectionEstatsMaximum>* enabledTypes;
	MgMIB_TCPROW tcpRow; 
};

}}}

