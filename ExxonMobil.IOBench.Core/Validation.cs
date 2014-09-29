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

namespace ExxonMobil.IOBench.Core
{
	class Validation
	{
		public ILogger Logger { get; set; }
		public string Tag { get; set; }
		public int IssueCount { get; private set; }

		public Validation(ILogger logger = null, string tag = null)
		{
			if (logger == null)
				logger = NullLogger.Default;
			this.Logger = logger;
			this.Tag = tag;
		}

		public bool HasIssues
		{
			get { return IssueCount > 0; }
		}

		public void FailIf(Func<bool> condition, string message)
		{
			if (condition())
			{
				IssueCount++;
				Logger.Log((Tag == null ? String.Empty : Tag) + message, Category.Exception);
			}
		}
	}
}
