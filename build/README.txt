HADash Release Publisher – korrigierte Dateien

Kopiere beide Dateien in den Ordner:

build\

Vorhandene Dateien überschreiben:
- publish.ps1
- publish.cmd

Danach ausführen:

build\publish.cmd -CommitMessage "Fix release publisher" -Yes

Sind bereits alle Änderungen committet:

build\publish.cmd -Yes
