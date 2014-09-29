$ErrorActionPreference = "Stop"

function BuildSln($path, $config, $platform) {
    msbuild $path /t:Build "/p:Configuration=$config" "/p:Platform=$platform"
    if ($LastExitCode -ne 0) { throw "Build Failed." }
}

$Configuration = 'Release'
$iobenchDir = split-path -parent $MyInvocation.MyCommand.Definition
$packageDir = "$env:USERPROFILE\Desktop\IOBench-Package"

BuildSln $iobenchDir\ExxonMobil.IOBench.sln $Configuration "x86"
BuildSln $iobenchDir\ExxonMobil.IOBench.sln $Configuration "x64"

if (Test-Path $packageDir) {
    if (!$packageDir) { throw "safety check!" }
    rm $packageDir\* -Recurse -Force
} else {
    mkdir $packageDir
}

mkdir $packageDir\x86
mkdir $packageDir\x64

# MSIL Assemblies
copy $iobenchDir\ExxonMobil.IOBench.Cli\bin\$Configuration\iobench.exe $packageDir
copy $iobenchDir\ExxonMobil.IOBench.Cli\bin\$Configuration\iobench.exe.config $packageDir
copy $iobenchDir\ExxonMobil.IOBench.Cli\bin\$Configuration\ExxonMobil.IOBench.Core.dll $packageDir
copy $iobenchDir\ExxonMobil.IOBench.Cli\bin\$Configuration\ExxonMobil.Shared.dll $packageDir
copy $iobenchDir\ExxonMobil.IOBench.Cli\bin\$Configuration\ExxonMobil.Shared.Logging.dll $packageDir
copy $iobenchDir\ExxonMobil.IOBench.Cli\bin\$Configuration\ExxonMobil.Shared.Win32.dll $packageDir

#x86 Dlls
copy $iobenchDir\$Configuration\ExxonMobil.IOBench.NativeCore.dll $packageDir\x86
copy $iobenchDir\$Configuration\ExxonMobil.Shared.IPHelper.dll $packageDir\x86
copy $env:VS110COMNTOOLS\..\..\VC\redist\x86\Microsoft.VC110.CRT\*.dll $packageDir\x86

#x64 Dlls
copy $iobenchDir\x64\$Configuration\ExxonMobil.IOBench.NativeCore.dll $packageDir\x64
copy $iobenchDir\x64\$Configuration\ExxonMobil.Shared.IPHelper.dll $packageDir\x64
copy $env:VS110COMNTOOLS\..\..\VC\redist\x64\Microsoft.VC110.CRT\*.dll $packageDir\x64
