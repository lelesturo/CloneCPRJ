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

## V6 — UI comune THYTOOLS

Header parametrico e logo ad alta qualità; tema WinForms condiviso con Backup IBU V3.
Sorgenti in `V6/source`, portable in `V6/portable`, distribuzione `V6/CreaCprj_v6.zip`.
Compilare nella suite THYTOOLS con `tools/build_winforms_ui.ps1 -Tool CLONCPRJ`;
header e logo sono collegati dal repository della suite. Motore/CLI ereditati
dal checkout locale V5.3, senza cambi funzionali nel ciclo UI.
Branch dedicato: `_versione_v6`.
# Crea CPRJ V7 — UI comune

La release V7 conserva motore e CLI della V6 e allinea log grafite/giallo,
scrollbar, pannello log ridimensionabile e menu di selezione delle griglie.
La selezione delle celle non cambia l'inclusione dei dati nel motore.
Portable e archivio sono in `V7`; branch dedicato `_versione_v7`.
Il tema/logo restano collegati alla suite THYTOOLS.
