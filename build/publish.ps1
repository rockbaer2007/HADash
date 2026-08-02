[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$CommitMessage = "",

    [Parameter(Mandatory = $false)]
    [switch]$Yes,

    [Parameter(Mandatory = $false)]
    [switch]$SkipPackage
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

try {
    $utf8 = New-Object System.Text.UTF8Encoding($false)
    [Console]::OutputEncoding = $utf8
    $OutputEncoding = $utf8
}
catch {
    # Die Codierung ist fuer die Funktion des Skripts nicht kritisch.
}

function Write-Step {
    param([string]$Message)

    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)

    Write-Host $Message -ForegroundColor Green
}

function Write-WarningMessage {
    param([string]$Message)

    Write-Host $Message -ForegroundColor Yellow
}

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $false)]
        [string[]]$Arguments = @(),

        [Parameter(Mandatory = $false)]
        [switch]$IgnoreExitCode
    )

    & $FilePath @Arguments
    $exitCode = $LASTEXITCODE

    if (-not $IgnoreExitCode -and $exitCode -ne 0) {
        throw "Befehl fehlgeschlagen: $FilePath $($Arguments -join ' ')`nFehlercode: $exitCode"
    }

    return $exitCode
}

function Get-RepositoryRoot {
    $scriptDirectory = Split-Path -Parent $PSCommandPath
    return (Resolve-Path (Join-Path $scriptDirectory "..")).Path
}

function Get-XmlValue {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Document,

        [Parameter(Mandatory = $true)]
        [string]$ElementName
    )

    $node = $Document.SelectSingleNode("//Project/PropertyGroup/$ElementName")

    if ($null -eq $node) {
        return $null
    }

    $value = $node.InnerText

    if ([string]::IsNullOrWhiteSpace($value)) {
        return $null
    }

    return $value.Trim()
}

function Get-ProjectVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $propsPath = Join-Path $RepositoryRoot "Directory.Build.props"

    if (-not (Test-Path -LiteralPath $propsPath)) {
        throw "Directory.Build.props wurde nicht gefunden: $propsPath"
    }

    [xml]$props = Get-Content -LiteralPath $propsPath -Raw -Encoding UTF8

    $version = Get-XmlValue -Document $props -ElementName "Version"

    if (-not [string]::IsNullOrWhiteSpace($version)) {
        return $version
    }

    $versionPrefix = Get-XmlValue -Document $props -ElementName "VersionPrefix"
    $versionSuffix = Get-XmlValue -Document $props -ElementName "VersionSuffix"

    if ([string]::IsNullOrWhiteSpace($versionPrefix)) {
        throw @"
In Directory.Build.props wurde keine Versionsnummer gefunden.

Erwartet wird entweder:

<Version>0.9.1-preview</Version>

oder:

<VersionPrefix>0.9.1</VersionPrefix>
<VersionSuffix>preview</VersionSuffix>
"@
    }

    if ([string]::IsNullOrWhiteSpace($versionSuffix)) {
        return $versionPrefix
    }

    return "$versionPrefix-$versionSuffix"
}

function Test-CommandAvailable {
    param([string]$CommandName)

    return $null -ne (Get-Command $CommandName -ErrorAction SilentlyContinue)
}

function Test-GitTagExistsLocally {
    param([string]$Tag)

    & git show-ref --verify --quiet "refs/tags/$Tag"
    return $LASTEXITCODE -eq 0
}

function Test-GitTagExistsRemotely {
    param(
        [string]$Remote,
        [string]$Tag
    )

    $tagReference = "refs/tags/$Tag"

    $result = @(
        & git ls-remote --tags --refs $Remote $tagReference 2>$null |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    if ($LASTEXITCODE -ne 0) {
        throw "Remote-Tags konnten nicht von '$Remote' gelesen werden."
    }

    return $result.Count -gt 0
}

function Get-CurrentCommit {
    $commit = (& git rev-parse HEAD).Trim()

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) {
        throw "Der aktuelle Git-Commit konnte nicht ermittelt werden."
    }

    return $commit
}

function Get-TagCommit {
    param([string]$Tag)

    $commit = (& git rev-list -n 1 $Tag 2>$null)

    if ($LASTEXITCODE -ne 0) {
        return $null
    }

    return ($commit | Out-String).Trim()
}

function Confirm-Action {
    param([string]$Message)

    if ($Yes) {
        return $true
    }

    $answer = Read-Host "$Message [j/N]"
    return $answer -match "^(j|ja|y|yes)$"
}

$repositoryRoot = $null

try {
    $repositoryRoot = Get-RepositoryRoot
    Set-Location $repositoryRoot

    Write-Host ""
    Write-Host "HADash Release Publisher" -ForegroundColor White
    Write-Host "Repository: $repositoryRoot"

    Write-Step "Voraussetzungen pruefen"

    if (-not (Test-CommandAvailable "git")) {
        throw "Git wurde nicht gefunden."
    }

    if (-not (Test-CommandAvailable "dotnet")) {
        throw "Das .NET SDK wurde nicht gefunden."
    }

    if (-not (Test-Path (Join-Path $repositoryRoot ".git"))) {
        throw "Der Ordner ist kein Git-Repository: $repositoryRoot"
    }

    $version = Get-ProjectVersion -RepositoryRoot $repositoryRoot
    $tag = "v$version"

    Write-Host "Version : $version"
    Write-Host "Tag     : $tag"

    Write-Step "Git-Remote pruefen"

    $remoteNames = @(& git remote)

    if ($LASTEXITCODE -ne 0) {
        throw "Die Git-Remotes konnten nicht gelesen werden."
    }

    if ($remoteNames -notcontains "origin") {
        throw @"
Das Git-Remote 'origin' ist nicht eingerichtet.

Beispiel:
git remote add origin https://github.com/rockbaer2007/HADash.git
"@
    }

    $remoteUrl = (& git remote get-url origin).Trim()

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($remoteUrl)) {
        throw "Die URL des Remotes 'origin' konnte nicht gelesen werden."
    }

    Write-Host "Remote  : $remoteUrl"

    Write-Step "Aktuellen Branch pruefen"

    $branch = (& git branch --show-current).Trim()

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($branch)) {
        throw "Der aktuelle Git-Branch konnte nicht ermittelt werden."
    }

    Write-Host "Branch  : $branch"

    if ($branch -ne "main") {
        Write-WarningMessage "Achtung: Der aktuelle Branch ist '$branch' und nicht 'main'."

        if (-not (Confirm-Action "Trotzdem fortfahren?")) {
            throw "Veroeffentlichung wurde abgebrochen."
        }
    }

    Write-Step "Repository-Status pruefen"

    $status = (& git status --porcelain | Out-String).Trim()

    if (-not [string]::IsNullOrWhiteSpace($status)) {
        if ([string]::IsNullOrWhiteSpace($CommitMessage)) {
            throw @"
Das Repository enthaelt noch nicht committete Aenderungen.

Nutze beispielsweise:

build\publish.cmd -CommitMessage "Release $tag"

oder committe die Aenderungen vorher manuell.
"@
        }

        Write-Host "Nicht committete Aenderungen wurden gefunden."
        Write-Host ""
        & git status --short

        if (-not (Confirm-Action "Alle angezeigten Aenderungen committen?")) {
            throw "Veroeffentlichung wurde abgebrochen."
        }

        Write-Step "Aenderungen committen"

        Invoke-NativeCommand -FilePath "git" -Arguments @("add", "-A")
        Invoke-NativeCommand -FilePath "git" -Arguments @("commit", "-m", $CommitMessage)

        Write-Success "Commit wurde erstellt."
    }
    else {
        Write-Success "Das Repository ist sauber."
    }

    Write-Step "Remote-Stand abrufen"

    Invoke-NativeCommand -FilePath "git" -Arguments @("fetch", "origin", "--prune")

    $remoteTagExists = Test-GitTagExistsRemotely -Remote "origin" -Tag $tag

    if ($remoteTagExists) {
        throw @"
Der Tag '$tag' existiert bereits auf GitHub.

Erhoehe zuerst die Version in Directory.Build.props.
"@
    }

    if (-not $SkipPackage) {
        Write-Step "Portable-Paket erstellen"

        $packageCmd = Join-Path $repositoryRoot "build\package.cmd"

        if (-not (Test-Path $packageCmd)) {
            throw "package.cmd wurde nicht gefunden: $packageCmd"
        }

        Invoke-NativeCommand -FilePath "cmd.exe" -Arguments @("/d", "/c", "`"$packageCmd`"")

        $expectedPackage = Join-Path $repositoryRoot "artifacts\packages\HADash-$tag-Portable-win-x64.zip"
        $alternativePackage = Join-Path $repositoryRoot "artifacts\packages\HADash-$version-Portable-win-x64.zip"

        if (Test-Path $expectedPackage) {
            Write-Success "Paket erstellt: $expectedPackage"
        }
        elseif (Test-Path $alternativePackage) {
            Write-Success "Paket erstellt: $alternativePackage"
        }
        else {
            Write-WarningMessage "Der Build war erfolgreich, aber das erwartete ZIP wurde nicht automatisch gefunden."
        }
    }
    else {
        Write-WarningMessage "Paketerstellung wurde mit -SkipPackage uebersprungen."
    }

    Write-Step "Pruefen, ob nach dem Paketieren Aenderungen entstanden sind"

    $statusAfterPackage = (& git status --porcelain | Out-String).Trim()

    if (-not [string]::IsNullOrWhiteSpace($statusAfterPackage)) {
        throw @"
Nach der Paketerstellung sind neue nicht committete Dateien oder Aenderungen entstanden.

Pruefe:
git status

Build- und Paketdateien sollten normalerweise durch .gitignore ausgeschlossen sein.
"@
    }

    Write-Step "Branch zu GitHub pushen"

    Invoke-NativeCommand -FilePath "git" -Arguments @("push", "-u", "origin", $branch)

    $currentCommit = Get-CurrentCommit
    $localTagExists = Test-GitTagExistsLocally -Tag $tag

    if ($localTagExists) {
        $localTagCommit = Get-TagCommit -Tag $tag

        if ([string]::IsNullOrWhiteSpace($localTagCommit)) {
            throw "Der vorhandene lokale Tag '$tag' konnte nicht ausgewertet werden."
        }

        if ($localTagCommit -ne $currentCommit) {
            throw @"
Der lokale Tag '$tag' zeigt nicht auf den aktuellen Commit.

Tag-Commit:
$localTagCommit

Aktueller Commit:
$currentCommit

Loesche den lokalen Tag und starte erneut:

git tag -d $tag
"@
        }

        Write-Success "Der lokale Tag '$tag' zeigt bereits auf den aktuellen Commit."
    }
    else {
        Write-Step "Lokalen Release-Tag erstellen"

        Invoke-NativeCommand -FilePath "git" -Arguments @(
            "tag",
            "-a",
            $tag,
            "-m",
            "HADash $tag"
        )

        Write-Success "Tag '$tag' wurde erstellt."
    }

    Write-Step "Release-Tag zu GitHub pushen"

    Invoke-NativeCommand -FilePath "git" -Arguments @("push", "origin", $tag)

    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host " Veroeffentlichung erfolgreich gestartet" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Version : $version"
    Write-Host "Tag     : $tag"
    Write-Host "Branch  : $branch"
    Write-Host "Remote  : $remoteUrl"
    Write-Host ""
    Write-Host "Der Tag wurde zu GitHub gepusht."
    Write-Host "Der GitHub-Actions-Release-Workflow sollte nun starten."
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "Veroeffentlichung fehlgeschlagen." -ForegroundColor Red
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    exit 1
}
finally {
    if ($null -ne $repositoryRoot -and (Test-Path $repositoryRoot)) {
        Set-Location $repositoryRoot
    }
}

exit 0
