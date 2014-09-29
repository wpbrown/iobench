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
using System.Globalization;
using ExxonMobil.Shared.Win32;

namespace ExxonMobil.IOBench.Core
{
	public class DataSizeFormatter : IFormatProvider, ICustomFormatter
	{
		public static readonly DataSizeFormatter Default = new DataSizeFormatter();

		public object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (!formatProvider.Equals(this)) return null;
			
			if (String.IsNullOrWhiteSpace(format) || !format.StartsWith("FS")) return null;

			ulong value;
			try
			{
				value = Convert.ToUInt64(arg);
			}
			catch (Exception)
			{
				return null;
			}

			return Format(value);
		}

		public static string Format(ulong size)
		{
			var buffer = new StringBuilder(32);
			Win32Methods.StrFormatByteSize(size, buffer, buffer.Capacity);
			return buffer.ToString().Replace("B", "iB");
		}
	}
}


