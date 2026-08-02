param(
  [string]$Tag,
  [string]$Runtime = 'win-x64',
  [switch]$Push
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
  & .\build\build.ps1 -Configuration Release
  & .\build\package.ps1 -Configuration Release -Runtime $Runtime
  if ($Tag) {
    git tag -a $Tag -m "HADash $Tag"
    if ($Push) { git push origin main; git push origin $Tag }
  }
} finally { Pop-Location }
