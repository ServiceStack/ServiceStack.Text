SET MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\Preview\Community\MSBuild\15.0\Bin\MSBuild.exe"

REM %MSBUILD% build-sn.proj /target:NuGetPack /property:Configuration=Signed;RELEASE=true;PatchVersion=9
%MSBUILD% build-core.proj /target:NuGetPack /property:Configuration=Release;PatchVersion=41
%MSBUILD% build.proj /target:NuGetPack /property:Configuration=Release;PatchVersion=9
%MSBUILD% build-pcl.proj /target:NuGetPack /property:Configuration=Release;PatchVersion=9
%MSBUILD% build-sl5.proj /target:NuGetPack /property:Configuration=Release;PatchVersion=9

