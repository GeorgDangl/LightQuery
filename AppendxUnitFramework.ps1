Param(
    [string]$globPattern = "testRuns_*.testresults",
    [string]$searchDirectory = $PSScriptRoot
)

# Find all files matching the glob pattern
$testResultFiles = Get-ChildItem -Path $searchDirectory -Filter $globPattern -Recurse

function getFrameworkNameFromFilename {
    Param($file)
    
    $frameworkName = $testResultFile.Name.Substring(0, $testResultFile.Name.Length - $testResultFile.Extension.Length)
    $frameworkName = $frameworkName.Substring($frameworkName.LastIndexOf("-") + 1)
    return $frameworkName;
}

function transformTestResultFile {
    Param($testResultFile)
    $frameworkName = getFrameworkNameFromFilename $testResultFile
    $xmlContent = [xml](Get-Content $testResultFile)
    $testNodes = $xmlContent.SelectNodes("//test/@type")
    foreach ($testNode in $testNodes){
        $testNode.Value = $frameworkName + "+" + $testNode.Value
    }
    $xmlContent.Save($testResultFile.FullName)
}

foreach ($testResultFile in $testResultFiles) {
    transformTestResultFile $testResultFile
}

