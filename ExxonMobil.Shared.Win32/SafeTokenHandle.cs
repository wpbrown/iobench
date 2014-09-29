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
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace ExxonMobil.Shared.Win32.SafeHandles
{
	[SecurityCritical]
	public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal static SafeTokenHandle InvalidHandle
		{
			get
			{
				return new SafeTokenHandle(IntPtr.Zero);
			}
		}

		private SafeTokenHandle() : base(true)
		{
		}

		public SafeTokenHandle(IntPtr handle) : base(true)
		{
			base.SetHandle(handle);
		}

		[SecurityCritical]
		protected override bool ReleaseHandle()
		{
			return Win32Methods2.CloseHandle(this.handle);
		}
	}
}