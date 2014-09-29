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
using System.Threading.Tasks;
using System.Threading;

namespace ExxonMobil.Shared
{
	public static class TaskEx
	{
		static readonly Task _sPreCompletedTask = GetCompletedTask();
		static readonly Task _sPreCanceledTask = GetPreCanceledTask();

		public static Task Delay(TimeSpan dueTime)
		{
			return Delay(dueTime, CancellationToken.None);
		}

		public static Task Delay(TimeSpan dueTime, CancellationToken cancellationToken)
		{
			return Delay((int)dueTime.TotalMilliseconds, cancellationToken);
		}

		public static Task Delay(int dueTimeMs)
		{
			return Delay(dueTimeMs, CancellationToken.None);
		}

		public static Task Delay(int dueTimeMs, CancellationToken cancellationToken)
		{
			if (dueTimeMs < -1)
				throw new ArgumentOutOfRangeException("dueTimeMs", "Invalid due time");
			if (cancellationToken.IsCancellationRequested)
				return _sPreCanceledTask;
			if (dueTimeMs == 0)
				return _sPreCompletedTask;

			var tcs = new TaskCompletionSource<object>();
			var ctr = new CancellationTokenRegistration();
			var timer = new Timer(delegate(object self)
			{
				ctr.Dispose();
				((Timer)self).Dispose();
				tcs.TrySetResult(null);
			});
			if (cancellationToken.CanBeCanceled)
				ctr = cancellationToken.Register(delegate
				{
					timer.Dispose();
					tcs.TrySetCanceled();
				});

			timer.Change(dueTimeMs, -1);
			return tcs.Task;
		}

		private static Task GetPreCanceledTask()
		{
			var source = new TaskCompletionSource<object>();
			source.TrySetCanceled();
			return source.Task;
		}

		private static Task GetCompletedTask()
		{
			var source = new TaskCompletionSource<object>();
			source.TrySetResult(null);
			return source.Task;
		}
	} 
}
