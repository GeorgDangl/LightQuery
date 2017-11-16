Copy-Item (Join-Path -Path $PSScriptRoot -ChildPath "../../README.md") "$PSScriptRoot/README.md"
If (Test-Path "$PSScriptRoot/coverage") {
    Remove-Item "$PSScriptRoot/coverage" -Recurse
}
If (Test-Path "$PSScriptRoot/karma-results.xml") {
    Remove-Item "$PSScriptRoot/karma-results.xml"
}
