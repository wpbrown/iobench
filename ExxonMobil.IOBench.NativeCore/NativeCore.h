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
#pragma once

#ifdef IOBENCH_EXPORTS
#define IOBENCH_API __declspec(dllexport)
#else
#define IOBENCH_API __declspec(dllimport)
#endif

#define BENCHOP_WRITE 1
#define BENCHOP_READ  2
#define BENCHAP_SEQUENTIAL 1
#define BENCHAP_RANDOM     2

struct Status;
class FiboLfsr;

extern "C" {

IOBENCH_API BOOL Experimental_EnableRemotePrefetch(HANDLE hFile, BOOL isAsync);
IOBENCH_API BOOL DisableLocalBuffering(HANDLE hFile, BOOL isAsync);
IOBENCH_API BOOL PreallocZeroed(HANDLE hFile, LARGE_INTEGER liFileSize, BOOL isAsync);
IOBENCH_API BOOL AsynchronousOp(HANDLE hFile, DWORD op, DWORD ap, BOOL verify, DWORD blocks, DWORD blockSize, BOOL randomData, DWORD maxOutstanding, Status* status);
IOBENCH_API BOOL SynchronousOp(HANDLE hFile, DWORD op, DWORD ap, BOOL verify, DWORD blocks, DWORD blockSize, BOOL randomData, Status* status);

}

BOOL CallNtFsControlFile(HANDLE hFile, BOOL isAsync, ULONG IoControlCode, PVOID InputBuffer, ULONG InputBufferLength);
FiboLfsr SeedRandom(DWORD blocks);
void SetNextRandomOffset(PLARGE_INTEGER pliOffset, DWORD blockSize, FiboLfsr& lfsr);
void FillBuffer(PVOID pBuffer, DWORD dwBufferSize, PLARGE_INTEGER pliOffset, BOOL randomData);
BOOL VerifyBuffer(PVOID pBuffer, DWORD dwBufferSize, PLARGE_INTEGER pliOffset); 
void StartPerfCount(PLARGE_INTEGER pliStart);
void StopPerfCount(PLARGE_INTEGER pliStart, PULONGLONG duration);
void StopAndAccumPerfCount(PLARGE_INTEGER pliStart, PULONGLONG accumulator);

void SeedRandomData();
LONGLONG GetRandomData();