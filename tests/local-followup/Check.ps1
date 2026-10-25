param(
 [string]$UnityRoot = 'C:/Program Files/Unity/Hub/Editor/6000.6.6f1/Editor',
 [string]$PackageAssemblies = 'C:/Users/Peter/Terra-Unity/Library/ScriptAssemblies'
)
$ErrorActionPreference = 'Stop'
$taskRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
& "$UnityRoot/Data/DotNetSdk/dotnet.exe" run --project "$PSScriptRoot/PlanChecks.csproj"
if ($LASTEXITCODE -ne 0) { throw 'Plan checks failed' }
& "$UnityRoot/Data/DotNetSdk/dotnet.exe" build "$PSScriptRoot/RuntimeCompile.csproj" --nologo -v:q "-p:UnityRoot=$UnityRoot" "-p:PackageAssemblies=$PackageAssemblies"
if ($LASTEXITCODE -ne 0) { throw 'WebGL source compile failed' }
