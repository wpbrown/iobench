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

namespace ExxonMobil.IOBench.Core.Fluent
{
	public static class BenchmarkConfigurationFluent
	{
		public static BenchmarkConfiguration Read(this BenchmarkConfiguration bc)
		{
			bc.Operation = BenchmarkOperation.Read;
			return bc;
		}

		public static BenchmarkConfiguration Write(this BenchmarkConfiguration bc)
		{
			bc.Operation = BenchmarkOperation.Write;
			return bc;
		}

		public static BenchmarkConfiguration Synchronously(this BenchmarkConfiguration bc)
		{
			bc.Asynchronous = false;
			return bc;
		}

		public static BenchmarkConfiguration Asynchronously(this BenchmarkConfiguration bc)
		{
			bc.Asynchronous = true;
			return bc;
		}

		public static BenchmarkConfiguration One(this BenchmarkConfiguration bc)
		{
			bc.FilePerBlock = false;
			return bc;
		}

		public static BenchmarkConfiguration Many(this BenchmarkConfiguration bc, int count)
		{
			bc.FilePerBlock = true;
			bc.Blocks = count;
			return bc;
		}

		public static BenchmarkConfiguration File(this BenchmarkConfiguration bc, string path)
		{
			bc.FilePath = path;
			return bc;
		}

		public static BenchmarkConfiguration WithBlockSize(this BenchmarkConfiguration bc, int bytes)
		{
			bc.BlockSizeBytes = bytes;
			return bc;
		}

		public static BenchmarkConfiguration Blocks(this BenchmarkConfiguration bc, int blocks)
		{
			bc.Blocks = blocks;
			return bc;
		}

		public static BenchmarkConfiguration Verified(this BenchmarkConfiguration bc)
		{
			bc.ReadVerify = true;
			return bc;
		}

		public static BenchmarkConfiguration Sequentially(this BenchmarkConfiguration bc)
		{
			bc.AccessPattern = AccessPattern.Sequential;
			return bc;
		}

		public static BenchmarkConfiguration Randomly(this BenchmarkConfiguration bc)
		{
			bc.AccessPattern = AccessPattern.Random;
			return bc;
		}

		public static BenchmarkConfiguration Preallocated(this BenchmarkConfiguration bc)
		{
			bc.Preallocation = PreallocationType.Zeroed;
			return bc;
		}
		

		public static BenchmarkConfiguration FastPreallocated(this BenchmarkConfiguration bc)
		{
			bc.Preallocation = PreallocationType.Unzeroed;
			return bc;
		}
	}
}
