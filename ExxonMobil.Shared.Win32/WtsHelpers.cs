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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ExxonMobil.Shared.Win32
{
	public class WtsHelpers
	{
		public static IList<WTS_SESSION_INFO> EnumerateSessionInfos()
		{
			IntPtr buffer;
			int count;

			if (Win32Methods.WTSEnumerateSessions(IntPtr.Zero, 0, 1, out buffer, out count) == 0)
				throw new Win32Exception();
			
			try
			{
				return MarshalEx.MarshalStructArray<WTS_SESSION_INFO>(buffer, count);
			}
			finally
			{
				Win32Methods.WTSFreeMemory(buffer);
			}
		}

		public static WtsSession GetSession(int id)
		{
			WTS_SESSION_INFO info;
			info.SessionID = id;
			info.State = (WtsConnectionState)GetSessionInfoInt(id, WTS_INFO_CLASS.WTSConnectState);
			info.WinStationName = GetSessionInfoString(id, WTS_INFO_CLASS.WTSWinStationName);
			return new WtsSession(info);
		}

		public static IList<WtsSession> GetSessions()
		{
			return EnumerateSessionInfos().Select(x => new WtsSession(x)).ToList();
		}
		
		public static WtsSession GetActiveSession()
		{
			return GetSessions().SingleOrDefault(s => s.ConnectionState == WtsConnectionState.Active);
		}

		private static T GetSessionInfo<T>(int sessionId, WTS_INFO_CLASS infoClass, Func<IntPtr,int,T> getData)
		{
			IntPtr buffer;
			int bytes;
			
			if (!Win32Methods.WTSQuerySessionInformationW(IntPtr.Zero, sessionId, infoClass, out buffer, out bytes))
				throw new Win32Exception();

			try
			{
				return getData(buffer, bytes);
			}
			finally
			{
				Win32Methods.WTSFreeMemory(buffer);
			}
		}

		public static string GetSessionInfoString(int sessionId, WTS_INFO_CLASS infoClass)
		{
			return GetSessionInfo<string>(sessionId, infoClass, (b,c) => Marshal.PtrToStringUni(b));
		}

		public static uint GetSessionInfoInt(int sessionId, WTS_INFO_CLASS infoClass)
		{
			return GetSessionInfo<uint>(sessionId, infoClass, (b, c) => (uint)Marshal.ReadInt32(b));
		}

		public static ushort GetSessionInfoShort(int sessionId, WTS_INFO_CLASS infoClass)
		{
			return GetSessionInfo<ushort>(sessionId, infoClass, (b, c) => (ushort)Marshal.ReadInt16(b));
		}

		public static T GetSessionInfoStruct<T>(int sessionId, WTS_INFO_CLASS infoClass)
		{
			return GetSessionInfo<T>(sessionId, infoClass, (b,c) => (T) Marshal.PtrToStructure(b, typeof(T)));
		}
	}
}
