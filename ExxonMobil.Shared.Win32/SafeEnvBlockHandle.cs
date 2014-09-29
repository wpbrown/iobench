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
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace ExxonMobil.Shared.Win32
{
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	public sealed class SafeEnvBlockHandle : SafeHandleZeroOrMinusOneIsInvalid 
	{ 
		private SafeEnvBlockHandle() : 
			base(true) 
		{
		}

		public SafeEnvBlockHandle(IntPtr handle)
			: base(true)
		{
			base.SetHandle(handle);
		}
		
		protected override bool ReleaseHandle() {
			return DestroyEnvironmentBlock(handle); 
		} 
		
		[SuppressUnmanagedCodeSecurity] 
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)] 
		[DllImport("userenv.dll", SetLastError = true)]
		private static extern bool DestroyEnvironmentBlock(IntPtr handle);
	}
}
