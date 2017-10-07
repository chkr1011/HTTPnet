param([string]$version)

if ([string]::IsNullOrEmpty($version)) {$version = "0.0.1"}

$msbuild = "MSBuild.exe"
&$msbuild ..\Frameworks\HTTPnet.NetFramework\HTTPnet.NetFramework.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\Frameworks\HTTPnet.Netstandard\HTTPnet.Netstandard.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\Frameworks\HTTPnet.UniversalWindows\HTTPnet.UniversalWindows.csproj /t:Build /p:Configuration="Release"

Remove-Item .\NuGet -Force -Recurse
New-Item -ItemType Directory -Force -Path .\NuGet
.\NuGet.exe pack HTTPnet.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version