# Release-Checkliste

HADash veröffentlicht Releases automatisch über einen Git-Tag.

## Voraussetzungen

- Git für Windows
- .NET 8 SDK
- ein eingerichtetes Remote `origin`
- GitHub Actions im Repository aktiviert

## Automatischer Ablauf

Im Repository-Stamm ausführen:

```cmd
build\publish.cmd
```

Das Skript:

1. liest die Version aus `Directory.Build.props`,
2. prüft Git, .NET SDK, Branch und Remote,
3. prüft auf ungespeicherte Git-Änderungen,
4. erstellt lokal das Portable-Paket,
5. prüft, ob der Release-Tag bereits auf GitHub existiert,
6. erstellt den Tag `v<Version>`,
7. pusht den aktuellen Branch,
8. pusht den Tag,
9. löst dadurch den GitHub-Actions-Workflow aus,
10. GitHub erstellt den Release und hängt das Portable-ZIP an.

## Änderungen automatisch committen

```cmd
build\publish.cmd -CommitMessage "Release v0.9.0-preview"
```

## Ohne Rückfrage veröffentlichen

```cmd
build\publish.cmd -Yes
```

## Nur Tag und GitHub-Automatik, ohne lokales Paket

```cmd
build\publish.cmd -SkipLocalPackage
```

## Neue Version veröffentlichen

Vor dem nächsten Release die Version in `Directory.Build.props` erhöhen. Beispiel:

```xml
<Version>0.9.1-preview</Version>
<FileVersion>0.9.1.0</FileVersion>
<AssemblyVersion>0.9.1.0</AssemblyVersion>
```

Danach erneut `build\publish.cmd` ausführen.
