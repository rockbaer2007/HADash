# Changelog


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
