# CLONCPRJ

Archivio versionato del tool `CreaCprjMontante`.

Cartella di lavoro ufficiale:

```text
C:\CONFIGURATORE\CLONCPRJ
```

## Versioni principali

- `V3`: versione CSV generico.
- `V4`: versione con supporto CSV, XLSX, XLSM e XLS.
- `OLD`: versioni precedenti, duplicati e materiale legacy.

## Branch

- `main`: archivio ordinato.
- `_versione_v3`: branch dedicato alla V3.
- `_versione_v4`: branch dedicato alla V4.

Le regole complete sono in `VERSIONING_RULES.md`.

## Struttura versioni

Ogni versione attiva deve restare pulita:

```text
V4/
  portable/
  source/
  CreaCprj_v4.zip
```

Niente file sciolti nella root della versione oltre allo ZIP; niente `bin/`, `obj/`, `.pdb` o file `- Copy` nei pacchetti finali.

## Script utili

- `New-VersionBranch.ps1`: crea una nuova cartella versione e il branch dedicato.
- `Push-VersionBranches.ps1`: pubblica `main` e tutti i branch `_versione_*` su `origin`.
