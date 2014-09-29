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
    public class EventLogLogger : ILogger
    {
        private EventLog log; 

        public EventLogLogger(EventLog log)
        {
            this.log = log;
        }

        public void Log(string message, Category category, Priority priority, int eventId)
        {
            EventLogEntryType et;
            if (category == Category.Exception)
                et = EventLogEntryType.Error;
            else if (category == Category.Warn)
                et = EventLogEntryType.Warning;
            else
                et = EventLogEntryType.Information;

            EventLog.WriteEntry(log.Source, message, et, eventId, (short)priority);
        }
    }
}
