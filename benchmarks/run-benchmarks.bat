@echo off
set proj=ServiceStack.Text.VersionCompareBenchmarks
set curdate=
for /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set curdate=%%c-%%a-%%b)
for /f "usebackq tokens=1,2 delims==" %%a in (`wmic os get LocalDateTime /VALUE 2^>NUL`) do (if '.%%a.'=='.LocalDateTime.' set ldt=%%b)
set curdate=%ldt:~0,4%-%ldt:~4,2%-%ldt:~6,2%
echo %proj%\ServiceStack.Text.VersionCompareBenchmarks.csproj %curdate%

mkdir Results

rmdir /s /q bin
rmdir /s /q obj
dotnet restore %proj%\ServiceStack.Text.VersionCompareBenchmarks.csproj && dotnet build -c Release %proj%\ServiceStack.Text.VersionCompareBenchmarks.csproj
%proj%\bin\Release\net46\ServiceStack.Text.VersionCompareBenchmarks.exe
copy BenchmarkDotNet.Artifacts\results\JsonDeserializationBenchmarks-report-github.md Results\JsonDeserialization-%curdate%.md
copy BenchmarkDotNet.Artifacts\results\ParseBuiltinBenchmarks-report-github.md Results\ParseBuiltin-%curdate%.md

rmdir /s /q bin
rmdir /s /q obj
dotnet restore %proj%\ServiceStack.Text.VersionCompareBenchmarks.BaseLine.csproj && dotnet build -c Release %proj%\ServiceStack.Text.VersionCompareBenchmarks.BaseLine.csproj
%proj%\bin\Release\net46\ServiceStack.Text.VersionCompareBenchmarks.BaseLine.exe
copy BenchmarkDotNet.Artifacts\results\JsonDeserializationBenchmarks-report-github.md Results\JsonDeserialization-baseline-%curdate%.md
copy BenchmarkDotNet.Artifacts\results\ParseBuiltinBenchmarks-report-github.md Results\ParseBuiltin-baseline-%curdate%.md
