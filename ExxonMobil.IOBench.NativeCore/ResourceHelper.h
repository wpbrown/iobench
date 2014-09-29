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

#include <Windows.h>

typedef VOID (WINAPI* PFNENSURECLEANUP)(UINT_PTR);

template<class TYPE, PFNENSURECLEANUP pfn, UINT_PTR tInvalid = NULL> 
class CEnsureCleanup {
public:
   CEnsureCleanup() { m_t = tInvalid; }

   CEnsureCleanup(TYPE t) : m_t((UINT_PTR) t) { }

   ~CEnsureCleanup() { Cleanup(); }

   BOOL IsValid() { return(m_t != tInvalid); }
   BOOL IsInvalid() { return(!IsValid()); }

   TYPE operator=(TYPE t) { 
      Cleanup(); 
      m_t = (UINT_PTR) t;
      return(*this);  
   }

   operator TYPE() { 
      return (TYPE) m_t;
   }

   void Cleanup() { 
      if (IsValid()) {
         pfn(m_t); 
         m_t = tInvalid;
      }
   }
   
private:
   UINT_PTR m_t;           
};

#define MakeCleanupClass(className, tData, pfnCleanup) \
   typedef CEnsureCleanup<tData, (PFNENSURECLEANUP) pfnCleanup> className;

#define MakeCleanupClassX(className, tData, pfnCleanup, tInvalid) \
   typedef CEnsureCleanup<tData, (PFNENSURECLEANUP) pfnCleanup, \
   (INT_PTR) tInvalid> className;

MakeCleanupClass(CEnsureCloseHandle,        HANDLE,    CloseHandle);
MakeCleanupClassX(CEnsureCloseFile,         HANDLE,    CloseHandle, 
   INVALID_HANDLE_VALUE);
MakeCleanupClass(CEnsureLocalFree,          HLOCAL,    LocalFree);
MakeCleanupClass(CEnsureGlobalFree,         HGLOBAL,   GlobalFree);
MakeCleanupClass(CEnsureRegCloseKey,        HKEY,      RegCloseKey);
MakeCleanupClass(CEnsureCloseServiceHandle, SC_HANDLE, CloseServiceHandle);
MakeCleanupClass(CEnsureCloseWindowStation, HWINSTA,   CloseWindowStation);
MakeCleanupClass(CEnsureCloseDesktop,       HDESK,     CloseDesktop);
MakeCleanupClass(CEnsureUnmapViewOfFile,    PVOID,     UnmapViewOfFile);
MakeCleanupClass(CEnsureFreeLibrary,        HMODULE,   FreeLibrary);

class CEnsureReleaseRegion {
public:
   CEnsureReleaseRegion(PVOID pv = NULL) : m_pv(pv) { }
   ~CEnsureReleaseRegion() { Cleanup(); }

   PVOID operator=(PVOID pv) { 
      Cleanup(); 
      m_pv = pv; 
      return(m_pv); 
   }
   operator PVOID() { return(m_pv); }
   operator PBYTE() { return((PBYTE)m_pv); }
   void Cleanup() { 
      if (m_pv != NULL) { 
         VirtualFree(m_pv, 0, MEM_RELEASE); 
         m_pv = NULL; 
      } 
   }
   
private:
   PVOID m_pv;
};

template<class TYPE>
class CEnsureHeapFree {
public:
   CEnsureHeapFree(PVOID pv = NULL, HANDLE hHeap = GetProcessHeap()) 
      : m_pv(pv), m_hHeap(hHeap) { }
   ~CEnsureHeapFree() { Cleanup(); }

   PVOID operator=(PVOID pv) { 
      Cleanup(); 
      m_pv = pv; 
      return(m_pv); 
   }
   operator TYPE() { return((TYPE)m_pv); }
   void Cleanup() { 
      if (m_pv != NULL) { 
         HeapFree(m_hHeap, 0, m_pv); 
         m_pv = NULL; 
      } 
   }
   
private:
   HANDLE m_hHeap;
   PVOID m_pv;
};