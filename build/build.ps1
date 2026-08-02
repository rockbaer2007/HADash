param(
  [ValidateSet('Debug','Release')][string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
  dotnet restore .\HADash.sln
  dotnet build .\HADash.sln -c $Configuration --no-restore
} finally { Pop-Location }
