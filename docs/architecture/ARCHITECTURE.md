# Architektur

HADash 3.0 trennt Repository, Anwendung, Launcher und Release-Automatisierung.

- `src/HADash.App`: WinForms-Anwendung mit modularen Ordnern für Core, UI, Parser, Export, Backup, Portable, Helpers und Resources.
- `src/HADash.Launcher`: kleiner .NET-Framework-4.8-Launcher, der die benötigte .NET-8-Desktop-Runtime prüft.
- `tools/ReleaseBuilder`: erzeugt reproduzierbare portable ZIP-Pakete.
- `build`: zentrale PowerShell-Einstiegspunkte.

Die bestehende Anwendung bleibt bewusst zunächst in einem Assembly, damit der Umbau keine Funktionsregression erzeugt. Spätere Sprints können Module kontrolliert in eigene Bibliotheken auslagern.
