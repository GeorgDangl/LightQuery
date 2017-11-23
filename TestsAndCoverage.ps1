$testProjects = Get-ChildItem  -Path "$PSScriptRoot\test" | ?{ $_.PSIsContainer } | Select-Object Name | foreach { $_.Name }
$testRuns = 1;

& dotnet restore
& dotnet build -c Debug

# Get the most recent OpenCover NuGet package from the dotnet nuget packages
$nugetOpenCoverPackage = Join-Path -Path $env:USERPROFILE -ChildPath "\.nuget\packages\OpenCover"
$latestOpenCover = Join-Path -Path ((Get-ChildItem -Path $nugetOpenCoverPackage | Sort-Object Fullname -Descending)[0].FullName) -ChildPath "tools\OpenCover.Console.exe"
# Get the most recent OpenCoverToCoberturaConverter from the dotnet nuget packages
$nugetCoberturaConverterPackage = Join-Path -Path $env:USERPROFILE -ChildPath "\.nuget\packages\OpenCoverToCoberturaConverter"
$latestCoberturaConverter = Join-Path -Path (Get-ChildItem -Path $nugetCoberturaConverterPackage | Sort-Object Fullname -Descending)[0].FullName -ChildPath "tools\OpenCoverToCoberturaConverter.exe"

$oldResults = Get-ChildItem -Path "$PSScriptRoot\results_*.testresults"
if ($oldResults) {
    Remove-Item $oldResults
}

If (Test-Path "$PSScriptRoot\OpenCover.coverageresults"){
	Remove-Item "$PSScriptRoot\OpenCover.coverageresults"
}

If (Test-Path "$PSScriptRoot\Cobertura.coverageresults"){
	Remove-Item "$PSScriptRoot\Cobertura.coverageresults"
}

foreach ($testProject in $testProjects){
    # Arguments for running dotnet
    $dotnetArguments = "xunit -nobuild -xml \""$PSScriptRoot\results_$testRuns.testresults\"""

    "Running tests with OpenCover"
    & $latestOpenCover `
        -register:user `
        -target:dotnet.exe `
        -targetdir:$PSScriptRoot\test\$testProject `
        "-targetargs:$dotnetArguments" `
        -returntargetcode `
        -output:"$PSScriptRoot\OpenCover.coverageresults" `
        -mergeoutput `
		-oldstyle `
        -excludebyattribute:System.CodeDom.Compiler.GeneratedCodeAttribute `
        "-filter:+[LightQuery*]* -[*.Tests]* -[*.Tests.*]*"

	$testRuns++
}

& cd "$PSScriptRoot"

"Prepending framework to test method name for better CI visualization"
$resultsGlobPattern = "results_*.testresults"
$prependFrameworkScript = ".\AppendxUnitFramework.ps1"
& $prependFrameworkScript $resultsGlobPattern "$PSScriptRoot"

"Running TypeScript tests for ng-lightquery"
cd "$PSScriptRoot/src/ng-lightquery"
& "./test.ps1"

"Converting coverage reports to Cobertura format"
& $latestCoberturaConverter `
    -input:"$PSScriptRoot\OpenCover.coverageresults" `
    -output:"$PSScriptRoot\Cobertura.coverageresults" `
    "-sources:$PSScriptRoot"
