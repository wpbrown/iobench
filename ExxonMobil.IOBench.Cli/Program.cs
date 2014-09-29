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
using ExxonMobil.IOBench.Core;
using System.Threading.Tasks;
using System.Threading;
using ExxonMobil.Shared.Logging;
using System.IO;
using System.Reflection;
using ExxonMobil.Shared.Cli;
using System.Net;

namespace ExxonMobil.IOBench.Cli
{
	class Program
	{
        static Program()
        {
            // In release builds, we use the folder structure created by the Build-IOBench script to
            // automatically select the correct platform specific dlls.
#if !DEBUG
            PlatformLibraryLoader.Configure();
#endif
        }

		static int Main()
		{
			logger = new ColorConsoleLogger();

			try
			{
				Console.WriteLine();
				Console.WriteLine(GetHeadline());
				Console.WriteLine();

				ConsoleArguments arguments;
				try { arguments = new ConsoleArguments(); }
				catch (ArgumentException e) { throw new IOBenchCliException("Argument parsing issue.", e) { HelpText = e.Message }; }
				
				if (arguments.NoArguments)
				{
					PrintUsage();
					return 1;
				}

				if (arguments.Named.ContainsKey(NetworkAnalysisOnlyOption))
					RunNetworkAnalysisOnly(arguments);
				else
					RunBenchmark(arguments);

				return 0;
			}
			catch (AggregateException e)
			{
				foreach (var innerException in e.InnerExceptions)
					HandleException(innerException);
			}
			catch (Exception e)
			{
				HandleException(e);
			}

			return 1;
		}

		private static void RunNetworkAnalysisOnly(ConsoleArguments arguments)
		{
			enableNetworkAnalysis = true;

			if (arguments.Named.Count > 1)
				throw new IOBenchCliException("-" + NetworkAnalysisOnlyOption + " cannot be combined with other options.");

			if (arguments.Anonymous.Count > 3)
				throw new IOBenchCliException("-" + NetworkAnalysisOnlyOption + " only accepts three arguments.");
			
			string host = arguments.Anonymous.First();
			ushort remotePort = NetworkAnalysis.SmbPort;
            ushort localPort = 0;

			if (arguments.Anonymous.Count > 1 && !ushort.TryParse(arguments.Anonymous[1], out remotePort))
				throw new IOBenchCliException("Invalid port: " + arguments.Anonymous[1]);
            if (arguments.Anonymous.Count > 2 && !ushort.TryParse(arguments.Anonymous[2], out localPort))
                throw new IOBenchCliException("Invalid port: " + arguments.Anonymous[2]);

			var networkAnalysis = new NetworkAnalysis(logger);
			networkAnalysis.Start(host, remotePort, localPort);

			bool stop = false;
			Console.CancelKeyPress += (s, e) => { stop = true; e.Cancel = true; };

			InitDisplay();
            try
            {
                while (!stop)
                {
                    UpdateNetworkAnalysisDisplay(networkAnalysis);
                    ResetDisplay();
                    Thread.Sleep(500);
                }
            }
            finally
            {
                EndDisplay();
            }

			networkAnalysis.Stop();
		}

		const string NetworkAnalysisOnlyOption = "nao";

		private static void RunBenchmark(ConsoleArguments arguments)
		{
			enableTransferDetails = true;
			var config = ProcessBenchmarkArgs(arguments);

			if (!config.Validate(logger))
				return;

			var networkAnalysis = enableNetworkAnalysis ? new NetworkAnalysis(logger) : null;
			var benchmark = Benchmark.Create(config);
			if (enableNetworkAnalysis)
				networkAnalysis.StartWithPath(config.FilePath, localPort: networkAnalysisLocalPort);

			var cts = new CancellationTokenSource();
			Console.CancelKeyPress += (s, e) => { cts.Cancel(); e.Cancel = true; };

			var benchmarkTask = benchmark.Start(cts.Token);

			InitDisplay();
            try
            {
                while (!benchmarkTask.IsCompleted)
                {
                    UpdateBenchmarkDisplay(benchmark);
                    if (enableNetworkAnalysis)
                        UpdateNetworkAnalysisDisplay(networkAnalysis);

                    ResetDisplay();
                    Thread.Sleep(500);
                }
                UpdateBenchmarkDisplay(benchmark);
                if (enableNetworkAnalysis)
                    UpdateNetworkAnalysisDisplay(networkAnalysis);
            }
            finally
            {
                EndDisplay();
            }

			if (enableNetworkAnalysis)
				networkAnalysis.Stop();

			if (benchmarkTask.IsFaulted)
				throw benchmarkTask.Exception;

			if (benchmarkTask.IsCanceled)
			{
				logger.Log("Benchmark canceled by user.", Category.Exception);
				return;
			}

			if (resultFilePath != null)
				WriteResults(benchmark, config);
		}

		private static void ResetDisplay()
		{
			if (CursorYOrigin == -1)
			{
				CursorYOrigin = Console.CursorTop - DisplayHeight;
			}
			Console.SetCursorPosition(0, CursorYOrigin);
		}

		private static void WriteResults(Benchmark benchmark, BenchmarkConfiguration config)
		{
			var info = new FileInfo(resultFilePath);
			bool writeHeader = false;
			TextWriter writer;
			if (info.Exists)
			{
				if (info.Length == 0)
					writeHeader = true;
				writer = info.AppendText();
			}
			else
			{
				writer = info.CreateText();
				writeHeader = true;
			}
			using (writer)
			{
				if (writeHeader)
					writer.WriteLine("Tag\tAccess Pattern\tOperation\tMulti-file\tBlocks\tBlockSizeKB\tAsyncMax\tReadVerified\tAsynch\tNoBuffering\tWriteThrough\tDisableLocalBuffering\tPreallocated\t" +
						             "ReadWriteFile Time\tWait CompPort Time\tTransfer Wall Time\tCreateFile Time\tPreallocation Time");

				writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}",
					config.Name.Replace('\t', ' '),
					config.AccessPattern,
					config.Operation,
					config.FilePerBlock,
					config.Blocks,
					config.BlockSizeBytes / 1024,
					config.AsyncMaxBlocksOutstanding,
					config.IsRead ? (config.ReadVerify ? "Verified":"Unverified") : "N/A",
					config.Asynchronous ? "Async":"Sync",
					config.NoBuffering ? "NoBuffering" : "Buffering",
					config.WriteThrough ? "WriteThrough" : "NoWriteThrough",
					config.DisableLocalBuffering ? "DisableLocalBuffering" : "N/A",
					config.IsWrite ? (config.Preallocation != PreallocationType.None).ToString() : "N/A",
					benchmark.ReadWriteFileTime.TotalMilliseconds,
					benchmark.QueryCompletionPortTime.TotalMilliseconds,
					benchmark.TransferTime.TotalMilliseconds,
					benchmark.CreateFileTime.TotalMilliseconds,
					benchmark.PreallocationTime.TotalMilliseconds);
			}
		}

		private static void HandleException(Exception exception)
		{
			if (exception is ExceptionWithHelp)
			{
				var ewh = (ExceptionWithHelp)exception;
				var issueType = exception.GetType().Name.Replace("Exception", "");
				logger.Log(issueType + " Issue: " + exception.Message, Category.Exception);
				if (!String.IsNullOrEmpty(ewh.HelpText))
					logger.Log("Help Info: " + ewh.HelpText, Category.Info);
			}
			else
			{
				logger.Log("An unexpected issue stopped the program.", Category.Exception);
				logger.Log("Type: " + exception.GetType().FullName, Category.Exception);
				logger.Log("Message: " + exception.Message, Category.Exception);
			}
			//if (exception.InnerException != null)
			//{
			//    logger.Log("Inner exception:", Category.Exception);
			//    logger.Log("Type: " + exception.InnerException.GetType().FullName, Category.Exception);
			//    logger.Log("Message: " + exception.InnerException.Message, Category.Exception);
			//}
			try
			{
				string file = Path.GetTempPath() + "iobench-exception.txt";
				File.AppendAllText(file,
					Environment.NewLine + DateTime.Now + Environment.NewLine +
					exception.ToString()
					);
				logger.Log("Wrote exception information to: \"" + file + "\".", Category.Info);
			}
			catch (Exception)
			{
				logger.Log("Failed to write exception information to file.", Category.Exception);
			}
		}

		private static void EndDisplay()
		{
			Console.SetCursorPosition(0, CursorYOrigin + DisplayHeight);
			Console.CursorVisible = true;
		}

		private static void InitDisplay()
		{
			DisplayHeight = (enableTransferDetails ? TransferDisplayHeight : 0) +
							(enableNetworkAnalysis ? NetworkDisplayHeight : 0);
			DisplayWidth = enableNetworkAnalysis ? NetworkDisplayWidth : TransferDisplayWidth;

			if (Console.BufferWidth < DisplayWidth)
			{
				Console.BufferWidth = DisplayWidth;
				Console.WindowWidth = Console.BufferWidth;
			}

			if (Console.BufferHeight < DisplayHeight + 5)
			{
				Console.BufferHeight = DisplayHeight + 5;
				Console.WindowHeight = Console.BufferHeight;
			}

			Console.CursorVisible = false;
		}

		private const int TransferDisplayHeight = 7;
		private const int TransferDisplayWidth = 90;
		private const int NetworkDisplayHeight = 35;
		private const int NetworkDisplayWidth = 120;
		private static int CursorYOrigin = -1;
		private static int DisplayHeight = -1;
		private static int DisplayWidth = -1;

		private static ILogger logger;
		private static string resultFilePath;
		private static bool enableNetworkAnalysis;
		private static bool enableTransferDetails;
        private static ushort networkAnalysisLocalPort;

		private static void UpdateBenchmarkDisplay(Benchmark benchmark)
		{
			string text;

			text = String.Format(DataSizeFormatter.Default,
				 "Size: {0,-13:FS} Blocks: {1,-10} Transferred: {2,-10:FS} Percent: {3,-8:p1}",
				 benchmark.BytesTotal,
				 benchmark.BlocksTransferred,
				 benchmark.BytesTransferred,
				 benchmark.PercentComplete);
			Console.WriteLine(text);

			long bytesPerSecond = benchmark.AverageBytesTransferredPerSec;

            var instantText = benchmark.InstantCountersAvailable ? 
                String.Format("{0:0.0 'MiB/s'} ({1,-15:0 'Op/s)'}", 
                                (double)benchmark.InstantBytesTransferredPerSec / (1024 * 1024), 
                                benchmark.InstantDataOperationsPerSec) 
                : "Loading Perf Counters...";

			text = String.Format(
				"ReadWriteFile Time: {0,-16}   Completed Async: {5}\n" +
				"Wait CompPort Time: {1,-16}   Completed Sync:  {6}\n" +
				"Transfer Wall Time: {2,-16}   Avg Goodput:     {7:0.0 'MiB/s'} ({8,-15:0.0 'Mbit/s)'}\n" +
				"CreateFile Time:    {3,-16}   Instant Goodput: {9}\n" +
				"Preallocation Time: {4,-16} \n",
				benchmark.ReadWriteFileTime,
				benchmark.QueryCompletionPortTime,
				benchmark.TransferTime,
				benchmark.CreateFileTime,
				benchmark.PreallocationTime,
				benchmark.CompletedAsynchronously,
				benchmark.CompletedSynchronously,
				(double)bytesPerSecond / (1024 * 1024),
				(double)bytesPerSecond * 8 / 1000000,
                instantText);
			Console.WriteLine(text);
		}

		private static string UpdateNetworkAnalysisDisplay(NetworkAnalysis networkAnalysis)
		{
			networkAnalysis.Update();

			string text;
			
			text = String.Format(DataSizeFormatter.Default,
				"* Rec *********************************************** * ObsRec **************** * SendBuff *************************\n" +
				"CurRwinSent:    {0,-10:FS} EcnSent:       {7,-10    }  CurRwinRcvd:  {14,-10:FS}  CurRetxQueue {18,-10:FS  }\n" +
				"MaxRwinSent:    {1,-10:FS} EcnNoncesRcvd: {8,-10    }  MaxRwinRcvd:  {15,-10:FS}  MaxRetxQueue {19,-10:FS  }\n" +
				"MinRwinSent:    {2,-10:FS} CurReasmQueue: {9,-10:FS }  MinRwinRcvd:  {16,-10:FS}  CurAppWQueue {20,-10:FS  }\n" +
				"LimRwin:        {3,-10:FS} MaxReasmQueue: {10,-10:FS}  WinScaleRcvd: {17,-10   }  MaxAppWQueue {21,-10:FS  }\n" +
				"DupAckEpisodes: {4,-10   } CurAppRQueue:  {11,-10:FS}\n" +
				"DupAcksOut:     {5,-10   } MaxAppRQueue:  {12,-10:FS}\n" +
				"CeRcvd:         {6,-10:FS} WinScaleSent:  {13,-10   }\n",
				networkAnalysis.Estats.RecRod.CurRwinSent,
				networkAnalysis.Estats.RecRod.MaxRwinSent,
				networkAnalysis.Estats.RecRod.MinRwinSent,
				networkAnalysis.Estats.RecRod.LimRwin,
				networkAnalysis.Estats.RecRod.DupAckEpisodes,
				networkAnalysis.Estats.RecRod.DupAcksOut,
				networkAnalysis.Estats.RecRod.CeRcvd,
				networkAnalysis.Estats.RecRod.EcnSent,
				networkAnalysis.Estats.RecRod.EcnNoncesRcvd,
				networkAnalysis.Estats.RecRod.CurReasmQueue,
				networkAnalysis.Estats.RecRod.MaxReasmQueue,
				networkAnalysis.Estats.RecRod.CurAppRQueue,
				networkAnalysis.Estats.RecRod.MaxAppRQueue,
				networkAnalysis.Estats.RecRod.WinScaleSent,
				networkAnalysis.Estats.ObsRecRod.CurRwinRcvd,
				networkAnalysis.Estats.ObsRecRod.MaxRwinRcvd,
				networkAnalysis.Estats.ObsRecRod.MinRwinRcvd,
				networkAnalysis.Estats.ObsRecRod.WinScaleRcvd,
				networkAnalysis.Estats.SendBuffRod.CurRetxQueue,
				networkAnalysis.Estats.SendBuffRod.MaxRetxQueue,
				networkAnalysis.Estats.SendBuffRod.CurAppWQueue,
				networkAnalysis.Estats.SendBuffRod.MaxAppWQueue);
			Console.WriteLine(text);
			text = String.Format(DataSizeFormatter.Default,
				"* SndCong ********************************************************************  * Bandwidth ************************ \n" +
				"SndLimTransRwin: {0,-10   } SndLimTransSnd:  {6,-10   } CurCwnd:     {12,-10:FS} OutboundBandwidth       {18,-13:0.0 'Mbit/s'}\n" +
				"SndLimTimeRwin:  {1,-10   } SndLimTimeSnd:   {7,-10   } MaxSsCwnd:   {13,-10:FS} InboundBandwidth        {19,-13:0.0 'Mbit/s'}\n" +
				"SndLimBytesRwin: {2,-10:FS} SndLimBytesSnd:  {8,-10:FS} MaxCaCwnd:   {14,-10:FS} OutboundInstability     {20,-13:0.0 'Mbit/s'}\n" +
				"SndLimTransCwnd: {3,-10   } SlowStart:       {9,-10   } CurSsthresh: {15,-10:FS} InboundInstability      {21,-13:0.0 'Mbit/s'}\n" +
				"SndLimTimeCwnd:  {4,-10   } CongAvoid:       {10,-10  } MaxSsthresh: {16,-10:FS} OutboundBandwidthPeaked {22,-10}\n" +
				"SndLimBytesCwnd: {5,-10:FS} OtherReductions: {11,-10  } MinSsthresh: {17,-10:FS} InboundBandwidthPeaked  {23,-10}\n",
				networkAnalysis.Estats.SndCongRod.SndLimTransRwin,
				networkAnalysis.Estats.SndCongRod.SndLimTimeRwin,
				networkAnalysis.Estats.SndCongRod.SndLimBytesRwin,
				networkAnalysis.Estats.SndCongRod.SndLimTransCwnd,
				networkAnalysis.Estats.SndCongRod.SndLimTimeCwnd,
				networkAnalysis.Estats.SndCongRod.SndLimBytesCwnd,
				networkAnalysis.Estats.SndCongRod.SndLimTransSnd,
				networkAnalysis.Estats.SndCongRod.SndLimTimeSnd,
				networkAnalysis.Estats.SndCongRod.SndLimBytesSnd,
				networkAnalysis.Estats.SndCongRod.SlowStart,
				networkAnalysis.Estats.SndCongRod.CongAvoid,
				networkAnalysis.Estats.SndCongRod.OtherReductions,
				networkAnalysis.Estats.SndCongRod.CurCwnd,
				networkAnalysis.Estats.SndCongRod.MaxSsCwnd,
				networkAnalysis.Estats.SndCongRod.MaxCaCwnd,
				networkAnalysis.Estats.SndCongRod.CurSsthresh,
				networkAnalysis.Estats.SndCongRod.MaxSsthresh,
				networkAnalysis.Estats.SndCongRod.MinSsthresh,
				(double)networkAnalysis.Estats.BandwidthRod.OutboundBandwidth / 1000000,
				(double)networkAnalysis.Estats.BandwidthRod.InboundBandwidth / 1000000,
				(double)networkAnalysis.Estats.BandwidthRod.OutboundInstability / 1000000,
				(double)networkAnalysis.Estats.BandwidthRod.InboundInstability / 1000000,
				Convert.ToBoolean(networkAnalysis.Estats.BandwidthRod.OutboundBandwidthPeaked),
				Convert.ToBoolean(networkAnalysis.Estats.BandwidthRod.InboundBandwidthPeaked));
			Console.WriteLine(text);

			text = String.Format(DataSizeFormatter.Default,
				"* Path *************************************************************************************************************\n" +
				"FastRetran         {0,-10      } CongSignals      {10,-10     } SndDupAckEpisodes  {20,-10     } MinRtt       {30,-10     }\n" +
				"Timeouts           {1,-10      } PreCongSumCwnd   {11,-10:FS  } SumBytesReordered  {21,-10     } SumRtt       {31,-10     }\n" +
				"SubsequentTimeouts {2,-10      } PreCongSumRtt    {12,-10     } NonRecovDa         {22,-10     } CountRtt     {32,-10     }\n" +
				"CurTimeoutCount    {3,-10      } PostCongSumRtt   {13,-10     } NonRecovDaEpisodes {23,-10     } CurRto       {33,-10     }\n" +
				"AbruptTimeouts     {4,-10      } PostCongCountRtt {14,-10     } AckAfterFr         {24,-10     } MaxRto       {34,-10     }\n" +
				"PktsRetrans        {5,-10      } EcnSignals       {15,-10     } DsackDups          {25,-10     } MinRto       {35,-10     }\n" +
				"BytesRetrans       {6,-10:FS   } EceRcvd          {16,-10     } SampleRtt          {26,-10     } CurMss       {36,-10:FS  }\n" +
				"DupAcksIn          {7,-10      } SendStall        {17,-10     } SmoothedRtt        {27,-10     } MaxMss       {37,-10:FS  }\n" +
				"SacksRcvd          {8,-10      } QuenchRcvd       {18,-10     } RttVar             {28,-10     } MinMss       {38,-10:FS  }\n" +
				"SackBlocksRcvd     {9,-10      } RetranThresh     {19,-10     } MaxRtt             {29,-10     } SpuriousRto' {39,-10     }\n",
				networkAnalysis.Estats.PathRod.FastRetran,
				networkAnalysis.Estats.PathRod.Timeouts,
				networkAnalysis.Estats.PathRod.SubsequentTimeouts,
				networkAnalysis.Estats.PathRod.CurTimeoutCount,
				networkAnalysis.Estats.PathRod.AbruptTimeouts,
				networkAnalysis.Estats.PathRod.PktsRetrans,
				networkAnalysis.Estats.PathRod.BytesRetrans,
				networkAnalysis.Estats.PathRod.DupAcksIn,
				networkAnalysis.Estats.PathRod.SacksRcvd,
				networkAnalysis.Estats.PathRod.SackBlocksRcvd,
				networkAnalysis.Estats.PathRod.CongSignals,
				networkAnalysis.Estats.PathRod.PreCongSumCwnd,
				networkAnalysis.Estats.PathRod.PreCongSumRtt,
				networkAnalysis.Estats.PathRod.PostCongSumRtt,
				networkAnalysis.Estats.PathRod.PostCongCountRtt,
				networkAnalysis.Estats.PathRod.EcnSignals,
				networkAnalysis.Estats.PathRod.EceRcvd,
				networkAnalysis.Estats.PathRod.SendStall,
				networkAnalysis.Estats.PathRod.QuenchRcvd,
				networkAnalysis.Estats.PathRod.RetranThresh,
				networkAnalysis.Estats.PathRod.SndDupAckEpisodes,
				networkAnalysis.Estats.PathRod.SumBytesReordered,
				networkAnalysis.Estats.PathRod.NonRecovDa,
				networkAnalysis.Estats.PathRod.NonRecovDaEpisodes,
				networkAnalysis.Estats.PathRod.AckAfterFr,
				networkAnalysis.Estats.PathRod.DsackDups,
				networkAnalysis.Estats.PathRod.SampleRtt,
				networkAnalysis.Estats.PathRod.SmoothedRtt,
				networkAnalysis.Estats.PathRod.RttVar,
				networkAnalysis.Estats.PathRod.MaxRtt,
				networkAnalysis.Estats.PathRod.MinRtt,
				networkAnalysis.Estats.PathRod.SumRtt,
				networkAnalysis.Estats.PathRod.CountRtt,
				networkAnalysis.Estats.PathRod.CurRto,
				networkAnalysis.Estats.PathRod.MaxRto,
				networkAnalysis.Estats.PathRod.MinRto,
				networkAnalysis.Estats.PathRod.CurMss,
				networkAnalysis.Estats.PathRod.MaxMss,
				networkAnalysis.Estats.PathRod.MinMss,
				networkAnalysis.Estats.PathRod.SpuriousRtoDetections);
			Console.WriteLine(text);

			text = String.Format(DataSizeFormatter.Default,
				"* Data *************************************************************************************************************\n" +
				"DataBytesOut {0,-12:FS   } SegsOut         {4,-10      } SndUna         {8,-12      } RcvNxt            {12,-10     }\n" +
				"DataSegsOut  {1,-12      } SegsIn          {5,-10      } SndNxt         {9,-12      } ThruBytesReceived {13,-10:FS  }\n" +
				"DataBytesIn  {2,-12:FS   } SoftErrors      {6,-10      } SndMax         {10,-12     }\n" +
				"DataSegsIn   {3,-12      } SoftErrorReason {7,-10      } ThruBytesAcked {11,-12:FS  }\n",
				networkAnalysis.Estats.DataRod.DataBytesOut,
				networkAnalysis.Estats.DataRod.DataSegsOut,
				networkAnalysis.Estats.DataRod.DataBytesIn,
				networkAnalysis.Estats.DataRod.DataSegsIn,
				networkAnalysis.Estats.DataRod.SegsOut,
				networkAnalysis.Estats.DataRod.SegsIn,
				networkAnalysis.Estats.DataRod.SoftErrors,
				networkAnalysis.Estats.DataRod.SoftErrorReason,
				networkAnalysis.Estats.DataRod.SndUna,
				networkAnalysis.Estats.DataRod.SndNxt,
				networkAnalysis.Estats.DataRod.SndMax,
				networkAnalysis.Estats.DataRod.ThruBytesAcked,
				networkAnalysis.Estats.DataRod.RcvNxt,
				networkAnalysis.Estats.DataRod.ThruBytesReceived);
			Console.WriteLine(text);
			return text;
		}

		static BenchmarkConfiguration ProcessBenchmarkArgs(ConsoleArguments args)
		{
			var config = new BenchmarkConfiguration();

			if (args.Anonymous.Count != 1)
				throw new IOBenchCliException("Benchmark only takes one argumnet: file path.");

			config.FilePath = args.Anonymous.First();

			bool blockCountSet = false;
			long fileSizeBytes = 0;

			foreach (var arg in args.Named)
			{
				var key = arg.Key.ToLower();
				var val = arg.Value.ToLower();
				uint intVal;

				switch (key)
				{
					case "as":
						config.Asynchronous = true;
						break;
					case "mo":
						if (!uint.TryParse(val, out intVal))
							throw new IOBenchCliException("Invalid max outstanding: " + val);
						config.AsyncMaxBlocksOutstanding = (int)intVal;
						break;
					case "bc":
						if (!uint.TryParse(val, out intVal))
							throw new IOBenchCliException("Invalid block count: " + val);
						config.Blocks = (int)intVal;
						blockCountSet = true;
						break;
					case "bs":
						if (!uint.TryParse(val, out intVal))
							throw new IOBenchCliException("Invalid block size: " + val);
						config.BlockSizeBytes = (int)(intVal * 1024);
						break;
					case "fs":
						if (!uint.TryParse(val, out intVal))
							throw new IOBenchCliException("Invalid file size: " + val);
						fileSizeBytes = (long)intVal * 1024 * 1024;
						break;
					case "dlb":
						config.DisableLocalBuffering = true;
						break;
					case "nb":
						config.NoBuffering = true;
						break;
					case "rv":
						config.ReadVerify = true;
						break;
					case "rnd":
						config.WriteDataType = WriteDataType.Random;
						break;
					case "wt":
						config.WriteThrough = true;
						break;
					case "nf":
						config.DontFlushBuffers = true;
						break;
					case "erp":
						config.EnableRemotePrefetch = true;
						break;
					case "noh":
						config.NoOperationHints = true;
						break;
					case "pa":
						config.Preallocation = PreallocationType.Zeroed;
						break;
					case "fpa":
						config.Preallocation = PreallocationType.Unzeroed;
						break;
					case "rf":
						resultFilePath = val;
						break;
					case "tag":
						if (String.IsNullOrWhiteSpace(val))
							throw new IOBenchCliException("Invalid tag.");
						config.Name = val;
						break;
					case "na":
						enableNetworkAnalysis = true;
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            if (!ushort.TryParse(val, out networkAnalysisLocalPort))
                                throw new IOBenchCliException("Invalid local port: " + val);
                        }
						break;
					case "op":
						switch (val)
						{
							case "rr":
								config.AccessPattern = AccessPattern.Random;
								config.Operation = BenchmarkOperation.Read;
								break;
							case "rw":
								config.AccessPattern = AccessPattern.Random;
								config.Operation = BenchmarkOperation.Write;
								break;
							case "sr":
								config.AccessPattern = AccessPattern.Sequential;
								config.Operation = BenchmarkOperation.Read;
								break;
							case "sw":
								config.AccessPattern = AccessPattern.Sequential;
								config.Operation = BenchmarkOperation.Write;
								break;
							case "fr":
								config.AccessPattern = AccessPattern.Sequential;
								config.Operation = BenchmarkOperation.Read;
								config.FilePerBlock = true;
								break;
							case "fw":
								config.AccessPattern = AccessPattern.Sequential;
								config.Operation = BenchmarkOperation.Write;
								config.FilePerBlock = true;
								break;
							default:
								throw new IOBenchCliException("Invalid operation (rr,rw,sr,sw,fr,fw): " + val);
						}
						break;
					default:
						throw new IOBenchCliException("Invalid option: " + key);
				}
			}

			if (fileSizeBytes > 0)
			{
				if (blockCountSet)
					throw new IOBenchCliException("Only one of block count or file size can be specified.");

				if (fileSizeBytes % config.BlockSizeBytes != 0)
					throw new IOBenchCliException("File size must be a multiple of the block size.");

				config.Blocks = (int)(fileSizeBytes / config.BlockSizeBytes);
			}

			return config;
		}

		static void PrintUsage()
		{
			Console.WriteLine(Properties.Resources.HelpText);
		}

		static string GetHeadline()
		{
			string headline = String.Empty;
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
			if (attributes.Length > 0)
			{
				var titleAttribute = attributes.Cast<AssemblyTitleAttribute>().First();
				headline = titleAttribute.Title;
			}
			attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
			if (attributes.Length > 0)
			{
				var versionAttribute = attributes.Cast<AssemblyInformationalVersionAttribute>().First();
				headline += " " + versionAttribute.InformationalVersion;
			}
            headline += " (" + (Environment.Is64BitProcess ? "64" : "32") + "-bit)";

			return headline;
		}

	}
}
