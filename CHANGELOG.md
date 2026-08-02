# Changelog

## 0.9.4-preview

- Theme-System vollständig entfernt.
- Theme-Auswahl aus den Programmeinstellungen entfernt.
- Anwendung verwendet ausschließlich die Windows-Standarddarstellung.
- Darstellungseinstellungen enthalten weiterhin Schrift- und Icongröße.
- YAML-Syntaxfarben verwenden feste, gut lesbare Standardfarben.

## 0.9.3-preview

- Buttons in den Themes Hell, Blau und Home Assistant konsequent auf weiße Flächen gesetzt.
- Automatische kontrastreiche Button-Textfarbe ergänzt.
- Hover-, Pressed- und Rahmenfarben für helle Buttonflächen angepasst.
- Dark Theme bleibt dunkel mit hellgrauem Rahmen.

## [0.9.2-preview] - 2026-08-02

### Geändert

- Buttons im hellen Theme sind jetzt weiß und klar von den Icons unterscheidbar.
- Buttons im blauen Theme sind jetzt ebenfalls weiß.
- Hover- und gedrückte Zustände bleiben in beiden Themes sichtbar.
- Release-Publisher vollständig robust überarbeitet.
- Versionsauslesung unterstützt `Version` sowie `VersionPrefix`/`VersionSuffix`.
- Tag-Prüfung verursacht bei noch nicht vorhandenem Tag keinen Abbruch mehr.
- `publish.cmd` verwendet eine sichere einzeilige PowerShell-Ausführung.


## Unveröffentlicht

### Build und Release

- Vollautomatische Veröffentlichung über `build\publish.cmd`.
- Version, Branch, Remote und Git-Status werden vor dem Push geprüft.
- Lokale Änderungen können optional automatisch committet werden.
- Der Git-Tag wird aus `Directory.Build.props` erzeugt.
- Nach dem Tag-Push erstellt GitHub Actions automatisch den Release und lädt das Portable-ZIP hoch.

Alle bedeutenden Änderungen dieses Projekts werden hier dokumentiert.

## [Unreleased]

- vollständige lokale und GitHub-Actions-Validierung der Build- und Release-Pipeline
- portable Testinstallation auf einem zweiten Windows-System

## [0.9.0-preview] - 2026-08-02

- bestehender Funktionsstand von HADash 2.5.3 übernommen
- Repository professionell strukturiert
- zentrale Versionsverwaltung über `Directory.Build.props`
- ReleaseBuilder hinzugefügt
- Build-, Package- und Publish-Skripte ergänzt
- `.cmd`-Starter zur Umgehung lokaler PowerShell-Signaturrestriktionen ergänzt
- GitHub Actions für CI und tagbasierte Releases eingerichtet
- `.gitignore`, `.gitattributes` und `.editorconfig` vervollständigt
- persönliche portable Konfiguration aus dem Repository entfernt
- Dokumentation und GitHub-Vorlagen erweitert
