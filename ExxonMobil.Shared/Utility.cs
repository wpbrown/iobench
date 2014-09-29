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
using System.Reflection;

namespace ExxonMobil.Shared
{
	public static class Utility
	{
		public static string ToReflectedString(this object theObject)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			var members = new List<MemberInfo>();
			members.AddRange(theObject.GetType().GetProperties(flags));
			members.AddRange(theObject.GetType().GetFields(flags));
			members.Sort(new GenericComparer<MemberInfo>(x => x.Name));

			StringBuilder sb = new StringBuilder();
			string typeName = theObject.GetType().Name;
			sb.AppendLine(typeName);
			sb.AppendLine(string.Empty.PadRight(typeName.Length + 5, '='));

			foreach (var info in members)
			{
				object value;
				
				if (info is PropertyInfo)
				{
					value = ((PropertyInfo)info).GetValue(theObject, null);
				}
				else
				{
					value = ((FieldInfo)info).GetValue(theObject);
				}
				sb.AppendFormat("{0}: {1}{2}", info.Name, value != null ? value.ToString() : "null", Environment.NewLine);
			}

			return sb.ToString();
		}

        public static bool Contains(this string source, string needle, StringComparison comp) 
        {
            return source.IndexOf(needle, comp) >= 0; 
        } 
	}
}
