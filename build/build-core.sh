#!/bin/sh

if [ -z "$MajorVersion" ]; then
  MajorVersion=1
fi
if [ -z "$MinorVersion" ]; then
  MinorVersion=0
fi
if [ -z "$PatchVersion" ]; then
  PatchVersion=$BUILD_NUMBER
fi
if [ -z "$RELEASE" ]; then
  UnstableTag="-unstable"
fi

Version=$MajorVersion.$MinorVersion.$PatchVersion.0
EnvVersion=$MajorVersion.$MinorVersion$PatchVersion
PackageVersion=$MajorVersion.$MinorVersion.$PatchVersion$UnstableTag

echo replace AssemblyVersion
find ./src -type f -name "AssemblyInfo.cs" -exec sed -i "s/AssemblyVersion(\"[^\"]\+\")/AssemblyVersion(\"1.0.0.0\")/g" {} +
echo replace AssemblyFileVersion
find ./src -type f -name "AssemblyInfo.cs" -exec sed -i "s/AssemblyFileVersion(\"[^\"]\+\")/AssemblyFileVersion(\"${Version}\")/g" {} +

echo replace Env

sed -i "s/ServiceStackVersion = [[:digit:]]\+.[[:digit:]]\+m/ServiceStackVersion = ${EnvVersion}m/g" ./src/ServiceStack.Text/Env.cs

echo replace project.json
sed -i "s/\"version\": \"[^\"]\+\"/\"version\": \"${Version}\"/g" ./src/ServiceStack.Text/project.json

echo replace package
sed -i "s/<version>[^<]\+/<version>${PackageVersion}/g" ./NuGet.Core/ServiceStack.Text.Core/servicestack.text.core.nuspec


#restore packages
#(cd ./src && dotnet restore)
#(cd ./tests/ServiceStack.Text.Tests.NetCore && dotnet restore)

#execute tests
#(cd ./tests/ServiceStack.Text.Tests.Nectore/ServiceStack.Text.Tests && dotnet run -c Release)

#nuget pack
#(cd ./NuGet.Core && ./nuget.exe pack ServiceStack.Text.Core/servicestack.text.core.nuspec -symbols)
