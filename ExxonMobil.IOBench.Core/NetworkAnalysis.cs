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
using ExxonMobil.Shared.Logging;
using ExxonMobil.Shared.IPHelper;
using System.Net;
using System.IO;
using ExxonMobil.Shared.Win32;
using System.Security;
using System.ComponentModel;

namespace ExxonMobil.IOBench.Core
{
	public class NetworkAnalysis
	{
		private ILogger logger;
		private TcpEstatSession estatSession;

		public NetworkAnalysis(ILogger logger)
		{
			this.logger = logger;

			logger.Log("Network analysis enabled.");
		}

		public const ushort SmbPort = 445;
		public void StartWithPath(string filePath, ushort remotePort = SmbPort, ushort localPort = 0)
		{
			var di = new DirectoryInfo(Path.GetDirectoryName(filePath));
			if (!di.Exists)
				throw new DirectoryNotFoundException("Network analysis requires directory to exist.");

			var uri = ResolvePath(di.FullName);
			logger.Log("Resolved URI: " + uri.ToString());
			string hostname = uri.DnsSafeHost;

			Start(hostname, remotePort, localPort);
		}

		public void Start(string hostname, ushort remotePort = SmbPort, ushort localPort = 0)
		{
			var addresses = GetAddressesForHost(hostname);
			if (addresses.Count() > 1)
				logger.Log("Host has multiple IPs. Will search for connection in order.", Category.Warn);

			var connections = TcpHelper.GetTcpTable();

			MgMIB_TCPROW? match = null;
			foreach (var address in addresses)
			{
                logger.Log("Searching for connection" + (localPort == 0 ? "" : " from port " + localPort) + " to " + address + ":" + remotePort + "...");
				var possibleMatches = connections.Where(x => x.State == MgMIB_TCP_STATE.MIB_TCP_STATE_ESTAB &&
															x.RemoteAddress.Equals(address) &&
                                                            x.RemotePort == remotePort).ToList();
                if (localPort != 0)
                {
                    possibleMatches = possibleMatches.Where(x => x.LocalPort == localPort).ToList();
                }

				if (possibleMatches.Count > 1)
				{
                    if (remotePort == SmbPort)
						throw new NetworkAnalysisException("Could not determine SMB TCP connection. Multiple sessions connected to server.")
						{
							HelpText = "SMB only establishes one TCP connection per authenticated session to a server. If multiple sessions " +
									   "are attached to a server, the network analysis can not determine which TCP connection to monitor. The " +
									   "simplest solution is to close any explorer windows and disconnect shares to the server. You can also " +
									   "kill the existing connections to the server with Microsoft TCPView. Alternatively you can specify the " +
                                       "local port of the connection as an argument to the -na option."
						};
					else
						throw new NetworkAnalysisException("Could not determine TCP connection. Multiple connections exist to the specified remote server/port combination.");
				}
				if (possibleMatches.Count == 1)
				{
					match = possibleMatches.First();
					break;
				}
			}

			if (!match.HasValue)
				throw new NetworkAnalysisException("Could not find SMB TCP connection to server.");

			logger.Log("Connection: " + match.ToString());

			estatSession = new TcpEstatSession(match.Value);

			try
			{
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsSynOpts);
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsData);
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsRec);
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsObsRec);
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsSndCong);
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsPath);
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsSendBuff);
				estatSession.Enable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsBandwidth);
			}
			catch (SecurityException e)
			{
				throw new NetworkAnalysisException("Network analysis initialization failed.", e) { HelpText = "Network analysis requires local admin rights. The console must be elevated if UAC is enabled." };
			}

			estatSession.UpdateAllEnabled();
			estatSession.Disable(MgTCP_ESTATS_TYPE.TcpConnectionEstatsSynOpts);

			logger.Log("SynOpts.ActiveOpen: " + Convert.ToBoolean(Estats.SynOptsRos.ActiveOpen));
			logger.Log("SynOpts.MssRcvd: " + DataSizeFormatter.Format(Estats.SynOptsRos.MssRcvd));
			logger.Log("SynOpts.MssSent: " + DataSizeFormatter.Format(Estats.SynOptsRos.MssSent));
			logger.Log("SndCong.LimCwnd: " + DataSizeFormatter.Format(Estats.SndCongRos.LimCwnd));

			logger.Log("Network analysis started.");
		}

		private IEnumerable<IPAddress> GetAddressesForHost(string hostname)
		{
			string resolvedHostname = "<UNKNOWN>";
			IPAddress addressInput;
			IPHostEntry hostEntry = null;
			List<IPAddress> addresses;

			bool addressWasSpecified = IPAddress.TryParse(hostname, out addressInput);

			try
			{
				logger.Log("Resolving hostname...");
				hostEntry = Dns.GetHostEntry(hostname);
				resolvedHostname = hostEntry.HostName;
			}
			catch (Exception)
			{
				logger.Log("Resolving hostname failed.", Category.Exception);
				if (!addressWasSpecified) // if input was not an IP, we have to be able to resolve
					throw;
			}

			if (addressWasSpecified)
			{
				if (addressInput.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
					throw new NetworkAnalysisException("IPv6 address input not supported.");

				addresses = new List<IPAddress>() { addressInput };
			}
			else
			{
				addresses = hostEntry.AddressList.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
				if (addresses.Count == 0)
					throw new NetworkAnalysisException("No IPv4 addresses found for host.");
			}


			logger.Log("Hostname: " + resolvedHostname);
			logger.Log("IP: " + String.Join<IPAddress>(",", addresses));
		
			return addresses;
		}

		public TcpEstatSession Estats
		{
			get
			{
				if (estatSession == null)
					throw new InvalidOperationException("Analysis not running.");
				return estatSession;
			}
		}

		public void Stop()
		{
            if (estatSession != null)
            {
                estatSession.DisableAll();
                logger.Log("Network analysis stopped.");
                FinalizeAnalysis();
            }

		}

        private void FinalizeAnalysis()
        {
            if (estatSession.PathRod.CountRtt > 0)
            {
                var avgRtt = (double)estatSession.PathRod.SumRtt / estatSession.PathRod.CountRtt;
                logger.Log(String.Format("Avg TCP RTT: {0:0.0} ms", avgRtt));
            }

            var send = estatSession.DataRod.DataBytesOut > estatSession.DataRod.DataBytesIn;
            if (send)
            {
                if (Environment.Is64BitProcess)
                {
                    ulong totalMeasured = estatSession.SndCongRod.SndLimBytesCwnd + 
                                          estatSession.SndCongRod.SndLimBytesRwin + 
                                          estatSession.SndCongRod.SndLimBytesSnd;
                    if (totalMeasured > 0) {
                        var cwnd = (double)estatSession.SndCongRod.SndLimBytesCwnd / totalMeasured;
                        var rwin = (double)estatSession.SndCongRod.SndLimBytesRwin / totalMeasured;
                        var snd  = (double)estatSession.SndCongRod.SndLimBytesSnd / totalMeasured;
                        logger.Log(String.Format("Send Limits: {0:p1} CWND / {1:p1} RWIN / {2:p1} SND", cwnd, rwin, snd));
                    }
                }
            }
           
        }

		public void Update()
		{
            try
            {
                Estats.UpdateAllEnabled();
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ERROR_NOT_FOUND)
                    throw new NetworkAnalysisException("The connection was closed.", e);
                else
                    throw new NetworkAnalysisException("Update of network analysis data failed: " + e.Message, e);
            }
            catch (Exception e)
            {
                throw new NetworkAnalysisException("Update of network analysis data failed: " + e.Message, e);
            }
		}

        public const int ERROR_NOT_FOUND = 1168;

		public Uri ResolvePath(string path)
		{
			if (logger == null)
				logger = NullLogger.Default;

			logger.Log("Attempting to resolve path to server.");

			string uncPath;

			if (!DfsHelpers.IsPathUnc(path))
			{
				logger.Log("Getting mapped path for drive...");
				uncPath = DfsHelpers.GetMappedNetworkPathForDrive(path);
				logger.Log("Path: " + uncPath);
			}
			else
				uncPath = path;

			if (DfsHelpers.IsPathDfs(uncPath))
			{
				logger.Log("Resolving DFS path...");
				uncPath = DfsHelpers.ResolveDfsPath(uncPath, x => logger.Log("DFS Get: " + x));
				logger.Log("Path: " + uncPath);
			}

			return new Uri(uncPath);
		}	
	}
}
