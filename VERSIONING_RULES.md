# Regole versioni e branch

## Struttura cartelle

Cartella di lavoro ufficiale:

```text
C:\CONFIGURATORE\CLONCPRJ
```

Il root del progetto deve contenere solo:

```text
OLD/
V3/
V4/
V5/
...
```

- Ogni versione attiva vive in una cartella dedicata `Vx`.
- Le versioni non piu attive o i file legacy vanno in `OLD/`.
- Non modificare direttamente una versione vecchia per creare una nuova versione: copia la versione di partenza in una nuova cartella `Vx`.
- Non usare piu il percorso OneDrive come cartella di lavoro.

## Struttura interna versione

Ogni cartella versione attiva, per esempio `V4`, deve contenere solo:

```text
portable/
source/
CreaCprj_v4.zip
```

- `source/`: contiene solo sorgenti e file necessari alla compilazione. Non deve contenere `bin/`, `obj/` o file `- Copy`.
- `portable/`: contiene l'eseguibile pronto all'uso e la documentazione della versione. Non deve contenere `.pdb`, vecchie release notes o file `- Copy`.
- `CreaCprj_vx.zip`: contiene solo il contenuto portable da distribuire, cioe eseguibile, `README.txt` e release notes della versione.
- Non lasciare nella root della versione file esterni come `.sln`, `.gitignore`, esempi CSV, README duplicati o `.exe` sciolti.

## Regola branch

Ogni versione deve avere un branch Git dedicato:

```text
V3 -> _versione_v3
V4 -> _versione_v4
V5 -> _versione_v5
```

Il branch `main` contiene l'archivio ordinato e la documentazione comune.

Per pubblicare tutti i branch versione dopo aver configurato `origin`:

```powershell
.\Push-VersionBranches.ps1
```

## Workflow nuova versione

Esempio per creare `V5` partendo da `V4`:

```powershell
cd "C:\CONFIGURATORE\CLONCPRJ"

git checkout main
Copy-Item -LiteralPath ".\V4" -Destination ".\V5" -Recurse

git add V5 VERSIONING_RULES.md README.md
git commit -m "Add V5"

git checkout -b _versione_v5
git push -u origin _versione_v5
git checkout main
git push origin main
```

## Regola modifiche

- Modifiche a `V3`: lavorare su `_versione_v3`.
- Modifiche a `V4`: lavorare su `_versione_v4`.
- Modifiche comuni o riordino cartelle: lavorare su `main`.
- Prima di pubblicare, verificare sempre almeno:

```powershell
git status --short --branch
```
