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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ExxonMobil.IOBench.Cli
{
    static class PlatformLibraryLoader
    {
        public const string Path32Bit = "x86";
        public const string Path64Bit = "x64";

        public static void Configure()
        {
            platformPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Environment.Is64BitProcess ? Path64Bit : Path32Bit);
            if (!SetDllDirectoryW(platformPath))
            {
                var ex = new Win32Exception();
                Console.WriteLine(ex);
                throw ex;
            }
           AppDomain.CurrentDomain.AssemblyResolve += CustomResolve;
        }

        static Assembly CustomResolve(object sender, ResolveEventArgs args)
        {
            string baseName = args.Name.Substring(0, args.Name.IndexOf(','));
            if (baseName.EndsWith(".resources"))
                return null;

            string fullPath = Path.Combine(platformPath, baseName + ".dll");
            return Assembly.LoadFile(fullPath);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectoryW(string lpPathName);

        private static string platformPath;
    }
}
