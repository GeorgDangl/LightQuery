# Originally from https://github.com/dotnet/docfx/issues/1752#issuecomment-329711278
# Generating documentation requires Visual Studio 2017 to be installed, also on the CI server.

$VisualStudioVersion = "15.0";
$DotnetSDKVersion = "2.0.0";

# Get dotnet paths
$MSBuildExtensionsPath = "C:\Program Files\dotnet\sdk\" + $DotnetSDKVersion;
$MSBuildSDKsPath = $MSBuildExtensionsPath + "\SDKs";

# Get Visual Studio install path
$VSINSTALLDIR =  $(Get-ItemProperty "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7").$VisualStudioVersion;

# Add Visual Studio environment variables
$env:VisualStudioVersion = $VisualStudioVersion;
$env:VSINSTALLDIR = $VSINSTALLDIR;

# Add dotnet environment variables
$env:MSBuildExtensionsPath = $MSBuildExtensionsPath;
$env:MSBuildSDKsPath = $MSBuildSDKsPath;

# Install latest docfx.console
& ./nuget.exe install docfx.console
$docFxPackageContentPath = Join-Path -Path ((Get-ChildItem -Path "$PSScriptRoot" -Filter "docfx.console*" | Sort-Object Fullname -Descending)[0].FullName) -ChildPath "content"
If (Test-Path $docFxPackageContentPath){
	Remove-Item $docFxPackageContentPath -Recurse
}

$outputPath = Join-Path -Path (Get-Item $PSScriptRoot).Parent.FullName -ChildPath "generated_docs"

If (Test-Path $outputPath){
	Remove-Item $outputPath -Recurse
}

If (Test-Path "$PSScriptRoot\shared"){
	Remove-Item "$PSScriptRoot\shared" -Recurse
}

If (Test-Path "$PSScriptRoot\shared-aspnetcore"){
	Remove-Item "$PSScriptRoot\shared-aspnetcore" -Recurse
}

# Build our docs
Write-Host "`n[Build our docs]" -ForegroundColor Green

$latestDocFx = Join-Path -Path ((Get-ChildItem -Path "$PSScriptRoot" -Filter "docfx.console*" | Sort-Object Fullname -Descending)[0].FullName) -ChildPath "tools\docfx.exe"
& $latestDocFX "$PSScriptRoot/docfx.json"
