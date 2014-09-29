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
using System.Net;
using System.Net.Sockets;

namespace ExxonMobil.Shared.Win32
{
	public class WtsSession
	{
		internal WtsSession(WTS_SESSION_INFO sessionInfo)
		{
			Name = sessionInfo.WinStationName;
			ID = sessionInfo.SessionID;
			ConnectionState = sessionInfo.State;
		}

		public string Name { get; private set; }
		public int ID { get; private set; }
		public WtsConnectionState ConnectionState { get; private set; }

		private string user;
		public string UserName
		{
			get 
			{
 				if (user == null)
					user = WtsHelpers.GetSessionInfoString(ID, WTS_INFO_CLASS.WTSUserName);
				return user; 
			}
		}

		private string domain;
		public string DomainName
		{
			get 
			{
 				if (domain == null)
					domain = WtsHelpers.GetSessionInfoString(ID, WTS_INFO_CLASS.WTSDomainName);
				return domain; 
			}
		}

		private string remoteHostname;
		public string ClientName
		{
			get
			{
				if (remoteHostname == null)
					remoteHostname = WtsHelpers.GetSessionInfoString(ID, WTS_INFO_CLASS.WTSClientName);
				return remoteHostname;
			}
		}

		private IPAddress clientIPAddress;
		public IPAddress ClientIPAddress
		{
			get
			{
				if (clientIPAddress == null) {
					var wtsIPAddress = WtsHelpers.GetSessionInfoStruct<WTS_CLIENT_ADDRESS>(ID, WTS_INFO_CLASS.WTSClientAddress);
					var addressFamily =  (AddressFamily)wtsIPAddress.AddressFamily;
					int byteLength;
					if (addressFamily == AddressFamily.InterNetwork)
						byteLength = 4;
					else if (addressFamily == AddressFamily.InterNetworkV6)
						byteLength = 16;
					else 
						return null;
					
					clientIPAddress = new IPAddress(wtsIPAddress.Address.Skip(2).Take(byteLength).ToArray());					
				}
				return clientIPAddress;
			}
		}

		private bool? remoteConnection;
		public bool RemoteConnection
		{
			get
			{
				if (remoteConnection == null)
				{
					remoteConnection = WtsHelpers.GetSessionInfoShort(ID, WTS_INFO_CLASS.WTSClientProtocolType) != 0;
				}
				return remoteConnection.Value;
			}
		}
	}
}
