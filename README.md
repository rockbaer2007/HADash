# HADash

**Home Assistant Dashboard Toolkit** – ein portables Windows-Werkzeug von UGSo.

HADash entstand aus einem praktischen Bedarf: Einzelne Home-Assistant-Dashboard-Ansichten sollten separat als Backup gesichert werden können. Gleichzeitig sollten sich komplette Dashboards aus Home-Assistant-Backups sowie aus JSON- und YAML-Dateien wieder herauslösen und exportieren lassen.

## Funktionen

- `.dash`, `.yaml`, `.yml`, `.json`, `.txt`, `.lovelace` und beliebige Dateien über `*.*` öffnen
- JSON- und YAML-Inhalte automatisch erkennen
- Home-Assistant-Backupstrukturen auslesen
- Dashboard-Ansichten auflisten, durchsuchen und anzeigen
- einzelne Ansichten als TXT oder YAML exportieren
- aus einer Ansicht ein vollständiges neues Dashboard erzeugen
- komplette Dashboards als `.dash` oder `.yaml` speichern
- portable Einstellungen unter `config/user.config`
- Themes, zuletzt verwendete Dateien und mehrsprachige Einstellungen
- separater Launcher mit Prüfung der .NET-Desktop-Runtime

## Schnellstart für Entwickler

Voraussetzungen: Visual Studio 2022, .NET 8 SDK und .NET Framework 4.8 Developer Pack.

```powershell
./build/build.ps1
./build/package.ps1
```

Das portable Paket wird unter `artifacts/packages/` erzeugt.

## Release

Ein Git-Tag wie `v3.0.0` startet automatisch den GitHub-Release-Workflow und hängt das portable ZIP als Release-Asset an.

## Projektstruktur

```text
src/                 Hauptanwendung und Launcher
tools/ReleaseBuilder automatischer Paketgenerator
build/               Build-, Package- und Publish-Skripte
docs/                Benutzer- und Entwicklerdokumentation
examples/            Beispieldateien
artifacts/            lokale Build- und Paket-Ausgaben
.github/              CI, Releases und Vorlagen
```

## Lizenz

MIT License. Entwickelt von UGSo mit Hilfe von ChatGPT.
