# Release-Checkliste

## Vor dem Tag

- [ ] `git status` ist sauber
- [ ] `build\build.cmd` läuft fehlerfrei
- [ ] `build\package.cmd` erzeugt das portable ZIP
- [ ] Anwendung startet über `HADash.exe`
- [ ] Runtime-Hinweis wurde auf einem System ohne .NET 8 geprüft
- [ ] `.dash`, `.yaml` und `.json` wurden getestet
- [ ] einzelne Ansicht wurde als TXT und YAML exportiert
- [ ] ausgewählte Ansicht wurde als neues Dashboard erzeugt
- [ ] komplettes Dashboard wurde gespeichert
- [ ] portable `config\user.config` wird neben der EXE erzeugt
- [ ] keine persönlichen Pfade befinden sich im Repository oder Paket
- [ ] Versionsnummer und Changelog stimmen überein

## GitHub

- [ ] CI-Build ist grün
- [ ] Release-Tag folgt dem Format `vX.Y.Z`
- [ ] Portable ZIP ist als Release-Asset vorhanden
- [ ] Release Notes enthalten Änderungen und bekannte Einschränkungen
