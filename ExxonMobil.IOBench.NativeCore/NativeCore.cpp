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
#include "stdafx.h"
#include "NativeCore.h"
#include "ResourceHelper.h"
#include "Status.h"
#include "FiboLfsr.h"

#include <stdlib.h>
#include <time.h>
#include <crtdbg.h>
#include <Windows.h>
#include <stack>
#include <winternl.h>
#include <random>

BOOL AsynchronousOp(HANDLE hFile, DWORD op, DWORD ap, BOOL verify, DWORD blocks, DWORD blockSize, BOOL randomData, DWORD maxOutstanding, Status* status)
{
	BOOL bOk;
	LARGE_INTEGER liCurrentFileOffset = { 0 };
	DWORD nTransfersInProgress = 0;
	DWORD currentBlock = 0;
	LARGE_INTEGER liPerfCount;
	std::stack<BYTE> reqIdxStack;

	CEnsureHeapFree<LPOVERLAPPED> cefOverlappeds = 
		HeapAlloc(GetProcessHeap(), 0, sizeof(OVERLAPPED) * maxOutstanding);
	CEnsureHeapFree<LPOVERLAPPED_ENTRY> cefOverlappedEntries = 
		HeapAlloc(GetProcessHeap(), 0, sizeof(OVERLAPPED_ENTRY) * maxOutstanding);
	CEnsureReleaseRegion erpBuffer = VirtualAlloc(NULL, blockSize * maxOutstanding, MEM_COMMIT, PAGE_READWRITE);
	if ((PVOID)erpBuffer == NULL)
		return FALSE;
	CEnsureCloseHandle hIOCP = CreateIoCompletionPort(hFile, NULL, 0, 0);
	if (hIOCP.IsInvalid())
		return FALSE;

	FiboLfsr lfsr;
	if (ap == BENCHAP_RANDOM)
		lfsr = SeedRandom(blocks);
	if (randomData)
		SeedRandomData();

	for (BYTE i = 0; i < maxOutstanding; ++i)
		reqIdxStack.push(i);

	while ((currentBlock < blocks || nTransfersInProgress) && !status->Canceled)
	{
		while (currentBlock < blocks && nTransfersInProgress < maxOutstanding)
		{
			// Make new requests
			DWORD currentReqIdx = reqIdxStack.top(); 
			reqIdxStack.pop();
			LPOVERLAPPED currentReq = cefOverlappeds + currentReqIdx;
			PVOID currentBuffer = (PBYTE)erpBuffer + (currentReqIdx * blockSize);
			if (op == BENCHOP_WRITE)
				FillBuffer(currentBuffer, blockSize, &liCurrentFileOffset, randomData);
			currentReq->Internal = 0;
			currentReq->InternalHigh = 0;
			currentReq->Offset = liCurrentFileOffset.LowPart;
			currentReq->OffsetHigh = liCurrentFileOffset.HighPart;
			currentReq->hEvent = 0;

			StartPerfCount(&liPerfCount);
			if (op == BENCHOP_WRITE)
				bOk = WriteFile(hFile, currentBuffer, blockSize, NULL, currentReq);
			else
				bOk = ReadFile(hFile, currentBuffer, blockSize, NULL, currentReq);
			StopAndAccumPerfCount(&liPerfCount, &status->ReadWriteFilePerfCounts);
			if (bOk)
				++(status->CompletedSync);
			else if (GetLastError() != ERROR_IO_PENDING)
				return FALSE;
			else
				++(status->CompletedAsync);

			if (ap == BENCHAP_SEQUENTIAL)
				liCurrentFileOffset.QuadPart += blockSize;
			else
				SetNextRandomOffset(&liCurrentFileOffset, blockSize, lfsr);
			nTransfersInProgress++;
			++currentBlock;
		}

		// Process completed requests
		ULONG entriesRemoved = 0;
		StartPerfCount(&liPerfCount);
		if (!GetQueuedCompletionStatusEx(hIOCP, cefOverlappedEntries, maxOutstanding, &entriesRemoved, INFINITE, FALSE))
			return FALSE;
		StopAndAccumPerfCount(&liPerfCount, &status->GetQueuedCompletionStatusExPerfCounts);

		for (ULONG i = 0; i < entriesRemoved; ++i)
		{
			OVERLAPPED_ENTRY& entry = cefOverlappedEntries[i];
			if (entry.dwNumberOfBytesTransferred != blockSize || entry.lpOverlapped->Internal != 0)
			{
				// Safe to explictly truncate Internal on 64-bit.
				SetLastError((DWORD)entry.lpOverlapped->Internal);
				return FALSE;
			}
			// Safely truncate pointer arithmetic to BYTE. MaxOutstanding is never larger than BYTE range.
			BYTE reqIdx = (BYTE)(entry.lpOverlapped - cefOverlappeds);	
			if (op == BENCHOP_READ && verify)
			{ 
				LARGE_INTEGER liOffset;
				liOffset.LowPart = entry.lpOverlapped->Offset;
				liOffset.HighPart = entry.lpOverlapped->OffsetHigh;			
				PVOID buffer = (PBYTE)erpBuffer + (reqIdx * blockSize);
				if (!VerifyBuffer(buffer, blockSize, &liOffset))
				{
					SetLastError(ERROR_CRC);
					return FALSE;
				}
			}
			reqIdxStack.push(reqIdx);
		}

		nTransfersInProgress -= entriesRemoved;
		status->BlocksTransferred += entriesRemoved;
	}

	return TRUE;
}

FiboLfsr SeedRandom(DWORD blocks)
{
	UCHAR bitWidth = 0;
	while (blocks >>= 1) ++bitWidth;
	return FiboLfsr(bitWidth);
}

void SetNextRandomOffset(PLARGE_INTEGER pliOffset, DWORD blockSize, FiboLfsr& lfsr)
{
	DWORD block = lfsr.Next();
	pliOffset->QuadPart = (long)blockSize * block;
}

BOOL SynchronousOp(HANDLE hFile, DWORD op, DWORD ap, BOOL verify, DWORD blocks, DWORD blockSize, BOOL randomData, Status* status)
{
	BOOL bOk;
	DWORD currentBlock = 0;
	
	DWORD nBytesTransferred = 0;
	LARGE_INTEGER liCurrentFileOffset = { 0 };
	LARGE_INTEGER liPerfCount;

	CEnsureReleaseRegion erpBuffer = VirtualAlloc(NULL, blockSize, MEM_COMMIT, PAGE_READWRITE);
	if ((PVOID)erpBuffer == NULL)
		return FALSE;

	if (INVALID_SET_FILE_POINTER == SetFilePointer(hFile, 0, NULL, FILE_BEGIN))
		return FALSE;

	FiboLfsr lfsr;
	if (ap == BENCHAP_RANDOM)
		lfsr = SeedRandom(blocks);
	if (randomData)
		SeedRandomData();

	while (currentBlock < blocks && !status->Canceled)
	{
		if (op == BENCHOP_WRITE)
			FillBuffer(erpBuffer, blockSize, &liCurrentFileOffset, randomData);

		StartPerfCount(&liPerfCount);
		if (op == BENCHOP_WRITE)
			bOk = WriteFile(hFile, erpBuffer, blockSize, &nBytesTransferred, NULL);
		else
			bOk = ReadFile(hFile, erpBuffer, blockSize, &nBytesTransferred, NULL);
		StopAndAccumPerfCount(&liPerfCount, &status->ReadWriteFilePerfCounts);
		if (!bOk || nBytesTransferred != blockSize)
			return FALSE;

		if (op == BENCHOP_READ && verify && !VerifyBuffer(erpBuffer, blockSize, &liCurrentFileOffset))
		{
			SetLastError(ERROR_CRC);
			return FALSE;
		}

		if (ap == BENCHAP_SEQUENTIAL)
			liCurrentFileOffset.QuadPart += blockSize;
		else
		{
			SetNextRandomOffset(&liCurrentFileOffset, blockSize, lfsr);
			if (!SetFilePointerEx(hFile, liCurrentFileOffset, NULL, FILE_BEGIN))
				return FALSE;
		}
		++currentBlock;

		++(status->BlocksTransferred);
		++(status->CompletedSync);
	}

	return TRUE;
}

const DWORD PreallocBufferSize = 4 * 1024;

#define IOCTL_LMR_DISABLE_LOCAL_BUFFERING 0x140390 // DeviceIoControl sends wrong IRP for this control code despite what MSDN says
#define FSCTL_LMR_GET_HINT_SIZE           0x1401C4 // magic
#define FSCTL_LMR_GET_HINT_SIZE_INPUT     0x1607   // blackmagic 

typedef NTSTATUS (NTAPI *NtFsControlFilePtr) 
	(
    IN HANDLE FileHandle,
    IN HANDLE Event OPTIONAL,
    IN PIO_APC_ROUTINE ApcRoutine OPTIONAL,
    IN PVOID ApcContext OPTIONAL,
    OUT PIO_STATUS_BLOCK IoStatusBlock,
    IN ULONG IoControlCode,
    IN PVOID InputBuffer OPTIONAL,
    IN ULONG InputBufferLength,
    OUT PVOID OutputBuffer OPTIONAL,
    IN ULONG OutputBufferLength
    );

static NtFsControlFilePtr pNtFsControlFile = NULL;

BOOL DisableLocalBuffering(HANDLE hFile, BOOL isAsync)
{
	return CallNtFsControlFile(hFile, isAsync, IOCTL_LMR_DISABLE_LOCAL_BUFFERING, NULL, 0);
}

BOOL CallNtFsControlFile(HANDLE hFile, BOOL isAsync, ULONG IoControlCode, PVOID InputBuffer, ULONG InputBufferLength)
{
	if (pNtFsControlFile == NULL)
	{
		CEnsureFreeLibrary hModule = LoadLibrary(L"ntdll.dll");
		if (hModule.IsInvalid())
			return FALSE;
		pNtFsControlFile = (NtFsControlFilePtr)GetProcAddress(hModule, "NtFsControlFile");
		if (pNtFsControlFile == NULL)
			return FALSE;
	}

	IO_STATUS_BLOCK ioStatusBlock = { 0 };
	if (!NT_SUCCESS(pNtFsControlFile(hFile, NULL, NULL, NULL, &ioStatusBlock, IoControlCode, InputBuffer, InputBufferLength, NULL, 0)))
		return FALSE;

	if (isAsync && WAIT_OBJECT_0 != WaitForSingleObject(hFile, INFINITE))
		return FALSE;
		
	if (!NT_SUCCESS(ioStatusBlock.Status))
	{
		SetLastError(HRESULT_FROM_NT(ioStatusBlock.Status));
		return FALSE;
	}

	return TRUE;
}

BOOL Experimental_EnableRemotePrefetch(HANDLE hFile, BOOL isAsync)
{
	// simulate call to BasepEnableRemotePrefetch 
	// kernel32.dll (6.1.7600.16816) +8DCFB
	WORD inBuffer = FSCTL_LMR_GET_HINT_SIZE_INPUT;
	return CallNtFsControlFile(hFile, isAsync, FSCTL_LMR_GET_HINT_SIZE, &inBuffer, 2);
}

BOOL PreallocZeroed(HANDLE hFile, LARGE_INTEGER liFileSize, BOOL isAsync)
{
	BOOL bOk;
	LARGE_INTEGER liWriteOffset;
	liWriteOffset.QuadPart = liFileSize.QuadPart - PreallocBufferSize;

	CEnsureReleaseRegion erpBuffer = VirtualAlloc(NULL, PreallocBufferSize, MEM_COMMIT, PAGE_READWRITE); // 4KB
	ZeroMemory(erpBuffer, PreallocBufferSize);

	if (isAsync)
	{
		OVERLAPPED overlapped = { 0 };
		overlapped.Offset = liWriteOffset.LowPart;
		overlapped.OffsetHigh = liWriteOffset.HighPart;
		bOk = WriteFile(hFile, erpBuffer, PreallocBufferSize, NULL, &overlapped);

		if (!bOk)
		{
			if (GetLastError() != ERROR_IO_PENDING)
				return FALSE;
			if (WAIT_OBJECT_0 != WaitForSingleObject(hFile, INFINITE))
				return FALSE;
		}

		if (overlapped.Internal != 0 || overlapped.InternalHigh != PreallocBufferSize)
		{
			// Safe to explictly truncate Internal on 64-bit.
			SetLastError((DWORD)overlapped.Internal);
			return FALSE;
		}
	}
	else
	{
		DWORD nBytesWritten;
		if (!SetFilePointerEx(hFile, liWriteOffset, NULL, FILE_BEGIN))
			return FALSE;
		bOk = WriteFile(hFile, erpBuffer, PreallocBufferSize, &nBytesWritten, NULL);
		if (!bOk || nBytesWritten != PreallocBufferSize)
			return FALSE;
	}

	return TRUE;
}

void FillBuffer(PVOID pBuffer, DWORD dwBufferSize, PLARGE_INTEGER pliOffset, BOOL randomData) 
{
	_ASSERT(dwBufferSize % sizeof(LONGLONG) == 0);
	LONGLONG recordIndex = pliOffset->QuadPart / sizeof(LONGLONG);
	PLONGLONG pllBuff = (PLONGLONG)pBuffer;
	PVOID pEnd = (PBYTE)pBuffer + dwBufferSize;

	if (randomData) {
		for	(; pllBuff < pEnd; ++pllBuff)
			*pllBuff = GetRandomData();
	} else {
		for	(; pllBuff < pEnd; ++pllBuff, ++recordIndex)
			*pllBuff = recordIndex;
	}
}

BOOL VerifyBuffer(PVOID pBuffer, DWORD dwBufferSize, PLARGE_INTEGER pliOffset) 
{
	_ASSERT(dwBufferSize % sizeof(LONGLONG) == 0);
	LONGLONG recordIndex = pliOffset->QuadPart / sizeof(LONGLONG);
	PLONGLONG pllBuff = (PLONGLONG)pBuffer;
	PVOID pEnd = (PBYTE)pBuffer + dwBufferSize;
	for	(; pllBuff < pEnd; ++pllBuff, ++recordIndex)
		if (*pllBuff != recordIndex) 
			return FALSE;
	return TRUE;
}

void StartPerfCount(PLARGE_INTEGER pliStart)
{
	QueryPerformanceCounter(pliStart);
}

void StopPerfCount(PLARGE_INTEGER pliStart, PULONGLONG duration)
{
	LARGE_INTEGER liStop;
	QueryPerformanceCounter(&liStop);
	*duration = liStop.QuadPart - pliStart->QuadPart; // need force atomic?
}

void StopAndAccumPerfCount(PLARGE_INTEGER pliStart, PULONGLONG accumulator)
{
	ULONGLONG liDuration;
	StopPerfCount(pliStart, &liDuration);
	*accumulator += liDuration; // need force atomic?
}

/* Random Data */

std::tr1::mt19937_64 mtEngine;

void SeedRandomData()
{
	mtEngine = std::tr1::mt19937_64(time(NULL));
}

LONGLONG GetRandomData()
{
	return mtEngine();
}
