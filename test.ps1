$testProjects = "LightQuery.Client.Tests", "LightQuery.Client.Tests.Integration", "LightQuery.EntityFrameworkCore.Tests", "LightQuery.EntityFrameworkCore.Tests.Integration", "LightQuery.Shared.Tests", "LightQuery.Tests", "LightQuery.Tests.Integration"
$testRuns = 1;

& dotnet restore
& dotnet build -c Debug
	
$oldResults = Get-ChildItem -Path "$PSScriptRoot\results_*.testresults"
if ($oldResults) {
    Remove-Item $oldResults
}
	
foreach ($testProject in $testProjects){
    & cd "$PSScriptRoot\test\$testProject"
	
    & dotnet.exe xunit `
        -nobuild `
        -parallel all `
        -xml $PSScriptRoot\results_$testRuns.testresults   
                 
        $testRuns++
}

& cd "$PSScriptRoot"

"Prepending framework to test method name for better CI visualization"
$resultsGlobPattern = "results_*.testresults"
$prependFrameworkScript = ".\AppendxUnitFramework.ps1"
& $prependFrameworkScript $resultsGlobPattern "$PSScriptRoot"
