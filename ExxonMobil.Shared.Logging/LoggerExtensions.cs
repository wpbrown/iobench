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
using System.Diagnostics;

namespace ExxonMobil.Shared.Logging
{
	public static class LoggerExtensions
	{
		public static void Log(this ILogger logger, string message)
		{
			logger.Log(message, Category.Info, Priority.None, 0);
		}

		public static void Log(this ILogger logger, string message, Category category)
		{
			logger.Log(message, category, Priority.None, 0);
		}

        public static void Log(this ILogger logger, string message, Category category, Priority priority)
        {
            logger.Log(message, category, priority, 0);
        }

        public static void Log(this ILogger logger, string message, int eventId)
        {
            logger.Log(message, Category.Info, Priority.None, eventId);
        }

        public static void Log(this ILogger logger, string message, Category category, int eventId)
        {
            logger.Log(message, category, Priority.None, eventId);
        }

		[Conditional("DEBUG")]
		public static void LogDebug(this ILogger logger, string message)
		{
			logger.Log(message, Category.Debug, Priority.None, 0);
		}
	}
}
