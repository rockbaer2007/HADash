# HADash

**Home Assistant Dashboard Toolkit** – ein portables Windows-Werkzeug von UGSo.

> Status: **Public Preview v0.9.0-preview**  
> Die Kernfunktionen stammen aus dem bisher funktionierenden HADash-Stand. Build-, Packaging- und GitHub-Workflows sollten vor dem ersten stabilen Release auf einem Windows-Rechner und in GitHub Actions vollständig geprüft werden.

## Warum HADash entstanden ist

HADash entstand aus einem praktischen Bedarf: Einzelne Home-Assistant-Dashboard-Ansichten sollten unabhängig vom vollständigen Dashboard als Backup gesichert werden können. Zusätzlich sollten komplette Dashboards aus Home-Assistant-Backups sowie aus JSON- und YAML-Dateien wiederhergestellt und exportiert werden können.

## Funktionen

- `.dash`, `.yaml`, `.yml`, `.json`, `.txt`, `.lovelace` und beliebige Dateien über `*.*` öffnen
- JSON- und YAML-Inhalte automatisch erkennen
- Home-Assistant-Backupstrukturen auslesen
- Dashboard-Ansichten auflisten, durchsuchen und anzeigen
- einzelne Ansichten als TXT oder YAML exportieren
- aus einer ausgewählten Ansicht ein vollständiges neues Dashboard erzeugen
- komplette Dashboards als `.dash` oder `.yaml` speichern
- portable Einstellungen unter `config/user.config`
-  zuletzt verwendete Dateien sowie Deutsch, Englisch und Französisch
- separater Launcher mit Prüfung der benötigten .NET-Desktop-Runtime

## Voraussetzungen für Entwickler

- Windows 10 oder Windows 11
- Visual Studio 2022 mit **.NET-Desktopentwicklung**
- .NET 8 SDK
- .NET Framework 4.8 Developer Pack

## Projekt öffnen und bauen

```text
HADash.sln
```

In Visual Studio die NuGet-Pakete wiederherstellen und die Solution in `Release` neu erstellen.

Alternativ über die Eingabeaufforderung – ohne Änderung der PowerShell-Ausführungsrichtlinie:

```cmd
build\build.cmd
build\package.cmd
```

Das portable Paket wird unter `artifacts/packages/` erzeugt.

PowerShell ist weiterhin direkt möglich:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build\package.ps1
```

## Repository-Struktur

```text
src/                  Hauptanwendung und Launcher
tools/ReleaseBuilder  automatischer Paketgenerator
build/                Build-, Package- und Publish-Skripte
docs/                 Benutzer- und Entwicklerdokumentation
examples/             Beispieldateien
artifacts/             lokale Paket-Ausgaben
.github/               CI, Releases und Vorlagen
```

## Portable Konfiguration

Persönliche Einstellungen werden neben der Anwendung unter `config/user.config` gespeichert. Diese Datei ist absichtlich von Git ausgeschlossen, da sie lokale Pfade und zuletzt verwendete Dateien enthalten kann.

## Erster öffentlicher Release

Der derzeit empfohlene Ablauf ist:

1. Repository nach GitHub pushen.
2. GitHub-Actions-Build prüfen.
3. `build\package.cmd` lokal erfolgreich ausführen.
4. Portable ZIP auf einem zweiten Windows-PC testen.
5. Erst danach ein stabiles Tag wie `v1.0.0` erstellen.

Bis dahin sollte das Projekt als Preview gekennzeichnet bleiben.

## Repository-Beschreibung

> Portable Windows toolkit for extracting, managing and exporting Home Assistant Lovelace dashboards and views from YAML, JSON and backup files.

## Empfohlene GitHub Topics

`home-assistant`, `lovelace`, `dashboard`, `yaml`, `json`, `backup`, `windows`, `winforms`, `dotnet`, `portable`, `home-automation`, `export`, `converter`

## Lizenz

MIT License. Entwickelt von UGSo mit Hilfe von ChatGPT.


## Automatisches Veröffentlichen

Ein vollständiger Preview-Release wird mit einem Befehl vorbereitet und zu GitHub übertragen:

```cmd
build\publish.cmd
```

Das Skript liest die Version aus `Directory.Build.props`, baut das Portable-Paket, pusht den aktuellen Branch, erstellt und pusht den passenden Git-Tag. Der GitHub-Actions-Workflow erzeugt danach automatisch den GitHub-Release und lädt das ZIP als Release-Datei hoch.

Sind noch lokale Änderungen vorhanden, können sie direkt übernommen werden:

```cmd
build\publish.cmd -CommitMessage "Release v0.9.0-preview"
```

Details stehen in [`docs/developer/RELEASE_CHECKLIST.md`](docs/developer/RELEASE_CHECKLIST.md).

