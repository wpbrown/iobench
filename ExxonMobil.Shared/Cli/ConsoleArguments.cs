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
using System.Text;
using System.Linq;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace ExxonMobil.Shared.Cli
{
	public class ConsoleArguments
	{
		private Dictionary<string, string> named = new Dictionary<string, string>();
		private List<string> anonymous = new List<string>();

		static private Regex matchNamed = new Regex(@"^-(\w*)(?:[=:](.*))?");

		public Dictionary<string, string> Named
		{
			get { return named; }
		}

		public List<string> Anonymous
		{
			get { return anonymous; }
		}

		public int ArgumentCount
		{
			get { return named.Count + anonymous.Count; }
		}

		public bool NoArguments
		{
			get { return ArgumentCount == 0; }
		}

		public bool ContainsHelpFlag()
		{
			var rawArgs = Environment.GetCommandLineArgs();
			var helpFlags = new string[] { "/?", "/h", "/help", "-?", "-h", "-help", "--help" };
			foreach (var flag in helpFlags)
			{
				if (rawArgs.Contains(flag))
					return true;
			}
			return false;
		}

		public ConsoleArguments()
		{
			var rawArgs = Environment.GetCommandLineArgs();
			foreach (string arg in rawArgs.Skip(1))
			{
				Match match = matchNamed.Match(arg);
				if (match.Success)
				{
					var key = match.Groups[1].Value.ToLower();
					if (named.ContainsKey(key))
						throw new ArgumentException("Same console argument specified more than once.", key);
					named.Add(key, match.Groups[2].Value);
				}
				else
				{
					anonymous.Add(arg);
				}
			}
		}
	}
}

