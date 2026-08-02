param(
  [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
  [string]$Runtime = 'win-x64'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
  dotnet run --project .\tools\ReleaseBuilder\ReleaseBuilder.csproj -c Release -- --configuration $Configuration --runtime $Runtime
} finally { Pop-Location }
