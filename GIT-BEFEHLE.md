# Git-Befehle für HADash v0.9.2-preview

Die Befehle im Stammordner des Repositorys ausführen.

## 1. Repository prüfen

```powershell
git status
git remote -v
```

## 2. Richtige GitHub-Adresse setzen

Nur nötig, wenn `origin` fehlt oder falsch ist:

```powershell
git remote set-url origin https://github.com/rockbaer2007/HADash.git
```

Falls `origin` noch nicht existiert:

```powershell
git remote add origin https://github.com/rockbaer2007/HADash.git
```

## 3. Änderungen übernehmen und automatisch veröffentlichen

```powershell
build\publish.cmd -CommitMessage "Release v0.9.2-preview" -Yes
```

Der Befehl erstellt den Commit, baut das Portable-Paket, pusht `main`, erstellt den Tag `v0.9.2-preview` und pusht den Tag. GitHub Actions erstellt anschließend den Release und lädt das ZIP als Asset hoch.

## 4. Falls der lokale Branch hinter GitHub liegt

Zuerst:

```powershell
git fetch origin
```

Wenn der GitHub-Stand bewusst durch diesen Repository-Stand ersetzt werden soll:

```powershell
git push --force-with-lease -u origin main
```

Danach:

```powershell
build\publish.cmd -CommitMessage "Release v0.9.2-preview" -Yes
```

## 5. Vorhandenen fehlerhaften lokalen Tag entfernen

Nur nötig, wenn `v0.9.2-preview` lokal bereits auf einen falschen Commit zeigt:

```powershell
git tag -d v0.9.2-preview
```

Danach den Publish-Befehl erneut ausführen.
