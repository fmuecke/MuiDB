@echo off
nuget pack MuiDB.Tool\MuiDB.Tool.csproj -Build -Symbols -Properties Configuration=Release -Tool -NoPackageAnalysis
if errorlevel 1 pause
nuget pack MuiDB\MuiDB.Schema.nuspec
if errorlevel 1 pause
