cd src\LightQuery
& dotnet restore
& dotnet build -c Release
& dotnet pack -c Release --include-symbols
cd ..\..
