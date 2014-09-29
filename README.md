IOBench
=======
<pre>This tool reads and writes data with minimal overhead. It is not a file copy
tool. For write ops data is generated on the fly with a simple pattern. For
reads ops data is either discarded (or optionally verified to match the 
written data). This eliminates one device from the benchmarking processes and 
allows one to focus on the system under test. 

Usage: iobench [options] <file_path>
       iobench -nao <remote_host> [remote_port] [local_port]

Options:
 -as    Perform asynchronous IO. By default transfers will be synchronous.
 -mo=#  Maximum number of outstanding asynchronous IO operations (default: 8)
 -fs=#  File size in MB (default: 1024). Use -fs or blockSize * blockCount to 
        specify the final size of the file. The -bc and -fs options can not be 
        combined. For fr,fw operations this is the total size of all files.
 -bc=#  Number of blocks to transfer (default: 1024).
 -bs=#  Block size in kB (default: 1024). Must be a multiple of 4.

 -nb    Specifies FILE_FLAG_NO_BUFFERING.
 -wt    Specifise FILE_FLAG_WRITE_THROUGH.
 -dlb   Specifies IOCTL_LMR_DISABLE_LOCAL_BUFFERING. Only valid for remote
        files.
 -nf    Do not call FlushFileBuffers. Called by default on write operations.
 -erp   EXPERIMENTAL: Enable Remote Prefetch. Only valid with remote reads.
 
 -rv    Read verification. On read operations, data will be verified to match
        the data written by a write operation. This works with only with data
        created with this tool.
 -rnd   Write random data. By default the file is filled with sequential 64bit 
        numbers. Files written with this flag cannot be verified with the -rv 
        flag.
 -pa    Preallocate space. By default file is expanded by the OS on demand.
        The OS will zero-fill the space for security reasons so this operation 
        can be time consuming. -fpa can be used for instant preallocation.
 -fpa   Fast preallocate space. Requires 'Manage the files on a volume' user
        right on the local machine (e.g. local admin). File must be local.
 -rf=X  File to write results to. Results are written in TSV format. If file
        already exists the results are appended.
 -tag=X An identifier to give the results row in the results file.
 -op=X  The operation to perform (default: sw). Valid operations:
             sr	 Sequential Read.
             sw	 Sequential Write.
             rr	 Random Read.
             rw	 Random Write.
             fr	 Multi-file Read.  
             fw	 Multi-file Write.
        Multi-file operations write each block to a seperate file. The file
        provided is appended with a .0000000 pattern. fr and fw can not be
        combined with -as, -pa, or -fpa.
 -noh   Do not use operation hints (i.e. FILE_FLAG_SEQUENTIAL_SCAN).
 -na    Enable advanced network analysis. Requires local admin rights. Only
        valid with remote transfers. 
        [=#] Optionally specify the local port. If the port is not specified
        IOBench will choose the only SMB connection to the server; if there
        is more than one, it will fail.
 -nao   Run network analysis only and do not run a transfer. Requires local 
        admin rights. This option can only be used by itself (see "Usage"). 

Output:
ReadWriteFile Time  Time spent in calls to ReadFile() or WriteFile().
Wait CompPort Time  Time spent in calls to GetQueuedCompletionStatusEx().
                    Only applies to asynchronous IO.
Transfer Wall Time  Total time spent reading or writing inclusive of time
                    spent in FlushFileBuffers.
CreateFile Time     Time spent in calls to CreateFile().
Preallocation Time  Total time spent preallocating file. Includes call to
                    SetFileSize() and forced zeroing by writing one block to 
                    the end of the file. Fast preallocate uses 
                    SetFileValidData().
Completed Async     ReadFile() or WriteFile() calls that reported finishing
                    asynchronously by return value. Some asynchronous calls
                    may finish synchronously but still report asynchronous
                    completion. Typically this is indicated by a low 
                    percentage of time in Wait CompPort time for an 
                    asynchronous transfer.
Completed Sync      ReadFile() or WriteFile() calls that reported finishing
                    synchronously by return value.
Avg Goodput         Rate of file data transfer averaged over entire Transfer
                    Wall Time. Does not represent TCP/SMB overhead and thus
                    will not match network utilization for remote transfers.

Examples:
* Mimick robocopying a 1GB file to a file server
  iobench -op=sw -as -mo=8 -fs=1024 -bs=1024 -dlb -nf \\server\share\file.bin

* Mimick robocopying a 1GB file from a file server
  iobench -op=sr -as -mo=8 -fs=1024 -bs=1024 -dlb -erp \\server\share\file.bin

* Mimick robocopying a 1000 8KB files to a file server
  iobench -op=fw -bc=1000 -bs=8 -dlb -nf \\server\share\file.bin</pre>
