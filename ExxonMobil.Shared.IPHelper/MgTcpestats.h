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

//
// Please don't change the order of this enum. The order defined in this
// enum needs to match the order in EstatsToTcpObjectMappingTable.
//
public enum struct MgTCP_ESTATS_TYPE : DWORD {
    TcpConnectionEstatsSynOpts,
    TcpConnectionEstatsData,
    TcpConnectionEstatsSndCong,
    TcpConnectionEstatsPath,
    TcpConnectionEstatsSendBuff,
    TcpConnectionEstatsRec,
    TcpConnectionEstatsObsRec,
    TcpConnectionEstatsBandwidth,
    TcpConnectionEstatsFineRtt,
};

//
// TCP_ESTATS_SYN_OPTS_ROS
//
// Define extended SYN-exchange information maintained for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_SYN_OPTS_ROS_v0 {
    BOOLEAN ActiveOpen;
    ULONG MssRcvd;
    ULONG MssSent;
};


//
// TCP_ESTATS_DATA_ROD
//
// Define extended data-transfer information for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_DATA_ROD_v0 {
    ULONG64 DataBytesOut;
    ULONG64 DataSegsOut;
    ULONG64 DataBytesIn;
    ULONG64 DataSegsIn;
    ULONG64 SegsOut;
    ULONG64 SegsIn;
    ULONG SoftErrors;
    ULONG SoftErrorReason;
    ULONG SndUna;
    ULONG SndNxt;
    ULONG SndMax;
    ULONG64 ThruBytesAcked;
    ULONG RcvNxt;
    ULONG64 ThruBytesReceived;
};

//
// TCP_ESTATS_SND_CONG_ROD
//
// Define extended sender-congestion information for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_SND_CONG_ROD_v0 {
    ULONG SndLimTransRwin;
    ULONG SndLimTimeRwin;
    SIZE_T SndLimBytesRwin;
    ULONG SndLimTransCwnd;
    ULONG SndLimTimeCwnd;
    SIZE_T SndLimBytesCwnd;
    ULONG SndLimTransSnd;
    ULONG SndLimTimeSnd;
    SIZE_T SndLimBytesSnd;
    ULONG SlowStart;
    ULONG CongAvoid;
    ULONG OtherReductions;
    ULONG CurCwnd;
    ULONG MaxSsCwnd;
    ULONG MaxCaCwnd;
    ULONG CurSsthresh;
    ULONG MaxSsthresh;
    ULONG MinSsthresh;
};

//
// TCP_ESTATS_SND_CONG_ROS
//
// Define static extended sender-congestion information for TCP connections.

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_SND_CONG_ROS_v0 {
    ULONG LimCwnd;
};

//
// TCP_ESTATS_PATH_ROD
//
// Define extended path-measurement information for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_PATH_ROD_v0 {
    ULONG FastRetran;
    ULONG Timeouts;
    ULONG SubsequentTimeouts;
    ULONG CurTimeoutCount;
    ULONG AbruptTimeouts;
    ULONG PktsRetrans;
    ULONG BytesRetrans;
    ULONG DupAcksIn;
    ULONG SacksRcvd;
    ULONG SackBlocksRcvd;
    ULONG CongSignals;
    ULONG PreCongSumCwnd;
    ULONG PreCongSumRtt;
    ULONG PostCongSumRtt;
    ULONG PostCongCountRtt;
    ULONG EcnSignals;
    ULONG EceRcvd;
    ULONG SendStall;
    ULONG QuenchRcvd;
    ULONG RetranThresh;
    ULONG SndDupAckEpisodes;
    ULONG SumBytesReordered;
    ULONG NonRecovDa;
    ULONG NonRecovDaEpisodes;
    ULONG AckAfterFr;
    ULONG DsackDups;
    ULONG SampleRtt;
    ULONG SmoothedRtt;
    ULONG RttVar;
    ULONG MaxRtt;
    ULONG MinRtt;
    ULONG SumRtt;
    ULONG CountRtt;
    ULONG CurRto;
    ULONG MaxRto;
    ULONG MinRto;
    ULONG CurMss;
    ULONG MaxMss;
    ULONG MinMss;
    ULONG SpuriousRtoDetections;
};

//
// TCP_ESTATS_SEND_BUFF_ROD
//
// Define extended output-queuing information for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_SEND_BUFF_ROD_v0 {
    SIZE_T CurRetxQueue;
    SIZE_T MaxRetxQueue;
    SIZE_T CurAppWQueue;
    SIZE_T MaxAppWQueue;
};


//
// TCP_ESTATS_REC_ROD
//
// Define extended local-receiver information for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_REC_ROD_v0 {
    ULONG CurRwinSent;
    ULONG MaxRwinSent;
    ULONG MinRwinSent;
    ULONG LimRwin;
    ULONG DupAckEpisodes;
    ULONG DupAcksOut;
    ULONG CeRcvd;
    ULONG EcnSent;
    ULONG EcnNoncesRcvd;
    ULONG CurReasmQueue;
    ULONG MaxReasmQueue;
    SIZE_T CurAppRQueue;
    SIZE_T MaxAppRQueue;
    UCHAR WinScaleSent;
};


//
// TCP_ESTATS_OBS_REC_ROD
//
// Define extended remote-receiver information for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_OBS_REC_ROD_v0 {
    ULONG CurRwinRcvd;
    ULONG MaxRwinRcvd;
    ULONG MinRwinRcvd;
    UCHAR WinScaleRcvd;
};

//
// TCP_ESTATS_BW_ROD
//
// Define bandwidth estimation statistics for TCP connections.
//
// Bandwidth and Instability metrics are expressed as bits per second.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_BANDWIDTH_ROD_v0 {
    ULONG64 OutboundBandwidth;
    ULONG64 InboundBandwidth;
    ULONG64 OutboundInstability;
    ULONG64 InboundInstability;
    BOOLEAN OutboundBandwidthPeaked;
    BOOLEAN InboundBandwidthPeaked;
};

//
// TCP_ESTATS_FINE_RTT_ROD
//
// Define fine-grained RTT estimation statistics for TCP connections.
//

[StructLayout(LayoutKind::Sequential)] 
public value struct MgTCP_ESTATS_FINE_RTT_ROD_v0 {
    ULONG RttVar;
    ULONG MaxRtt;
    ULONG MinRtt;
    ULONG SumRtt;
};

