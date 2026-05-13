# CLONCPRJ

Archivio versionato del tool `CreaCprjMontante`.

## Versioni principali

- `V3`: versione CSV generico.
- `V4`: versione con supporto CSV, XLSX, XLSM e XLS.
- `OLD`: versioni precedenti, duplicati e materiale legacy.

## Branch

- `main`: archivio ordinato.
- `_versione_v3`: branch dedicato alla V3.
- `_versione_v4`: branch dedicato alla V4.

Le regole complete sono in `VERSIONING_RULES.md`.

## Script utili

- `New-VersionBranch.ps1`: crea una nuova cartella versione e il branch dedicato.
- `Push-VersionBranches.ps1`: pubblica `main` e tutti i branch `_versione_*` su `origin`.
