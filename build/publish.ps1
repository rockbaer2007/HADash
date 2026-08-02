[CmdletBinding()]
param(
    [string]$Runtime = 'win-x64',
    [string]$CommitMessage,
    [switch]$SkipLocalPackage,
    [switch]$AllowDirty,
    [switch]$Yes
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
Push-Location $root

function Invoke-Native {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Befehl fehlgeschlagen ($LASTEXITCODE): $FilePath $($Arguments -join ' ')"
    }
}

function Get-ProjectVersion {
    [xml]$props = Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props')
    $version = [string]$props.Project.PropertyGroup.Version
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw 'In Directory.Build.props wurde keine Version gefunden.'
    }
    return $version.Trim()
}

function Test-CommandExists {
    param([string]$Name)
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

try {
    if (-not (Test-CommandExists 'git')) {
        throw 'Git wurde nicht gefunden. Installiere Git für Windows und öffne danach ein neues Terminal.'
    }

    if (-not (Test-CommandExists 'dotnet')) {
        throw 'Das .NET SDK wurde nicht gefunden. Installiere das .NET 8 SDK.'
    }

    Invoke-Native git rev-parse --is-inside-work-tree | Out-Null

    $version = Get-ProjectVersion
    $tag = "v$version"
    $branch = (& git branch --show-current).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($branch)) {
        throw 'Der aktuelle Git-Branch konnte nicht ermittelt werden.'
    }

    $remoteUrl = (& git remote get-url origin 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($remoteUrl)) {
        throw "Es ist kein Git-Remote namens 'origin' eingerichtet."
    }

    Write-Host ''
    Write-Host 'HADash Release-Automatik' -ForegroundColor Cyan
    Write-Host "Version : $version"
    Write-Host "Tag     : $tag"
    Write-Host "Branch  : $branch"
    Write-Host "Remote  : $remoteUrl"
    Write-Host ''

    $status = (& git status --porcelain)
    if ($status) {
        if ($CommitMessage) {
            Write-Host 'Lokale Änderungen werden übernommen ...' -ForegroundColor Yellow
            Invoke-Native git add --all
            Invoke-Native git commit -m $CommitMessage
        }
        elseif (-not $AllowDirty) {
            throw @"
Das Repository enthält noch nicht committete Änderungen.

Nutze entweder:
  build\publish.cmd -CommitMessage "Release $tag"

oder committe die Änderungen vorher manuell.
"@
        }
    }

    if (-not $SkipLocalPackage) {
        Write-Host 'Lokales Portable-Paket wird erstellt ...' -ForegroundColor Cyan
        & (Join-Path $PSScriptRoot 'package.ps1') -Configuration Release -Runtime $Runtime
        if ($LASTEXITCODE -ne 0) {
            throw 'Die lokale Paketerstellung ist fehlgeschlagen.'
        }
    }

    Invoke-Native git fetch origin --tags

    $headSha = (& git rev-parse HEAD).Trim()
    $localTagSha = (& git rev-list -n 1 $tag 2>$null)
    $localTagExists = $LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($localTagSha)

    $remoteTagLine = (& git ls-remote --tags origin "refs/tags/$tag" 2>$null)
    $remoteTagExists = $LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($remoteTagLine)

    if ($remoteTagExists) {
        throw "Der Tag '$tag' existiert bereits auf GitHub. Erhöhe zuerst die Version in Directory.Build.props."
    }

    if ($localTagExists) {
        $localTagSha = $localTagSha.Trim()
        if ($localTagSha -ne $headSha) {
            throw "Der lokale Tag '$tag' zeigt nicht auf den aktuellen Commit. Lösche oder korrigiere ihn zuerst."
        }
        Write-Host "Lokaler Tag '$tag' ist bereits korrekt vorhanden." -ForegroundColor DarkGreen
    }
    else {
        Write-Host "Tag '$tag' wird erstellt ..." -ForegroundColor Cyan
        Invoke-Native git tag -a $tag -m "HADash $tag"
    }

    if (-not $Yes) {
        Write-Host ''
        $answer = Read-Host "Branch '$branch' und Tag '$tag' jetzt zu GitHub pushen? [J/N]"
        if ($answer -notmatch '^(j|ja|y|yes)$') {
            Write-Host 'Veröffentlichung abgebrochen. Es wurde nichts gepusht.' -ForegroundColor Yellow
            exit 2
        }
    }

    Write-Host "Branch '$branch' wird gepusht ..." -ForegroundColor Cyan
    Invoke-Native git push origin $branch

    Write-Host "Tag '$tag' wird gepusht ..." -ForegroundColor Cyan
    Invoke-Native git push origin $tag

    Write-Host ''
    Write-Host 'Fertig.' -ForegroundColor Green
    Write-Host "GitHub Actions erstellt jetzt automatisch den Release '$tag' und lädt das Portable-ZIP hoch."
    Write-Host 'Öffne auf GitHub den Bereich Actions oder Releases, um den Fortschritt zu sehen.'
}
finally {
    Pop-Location
}
