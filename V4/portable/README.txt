CREA CPRJ MONTANTE

Versione 4.0.0
- Le tab "Progetti DIGIS da file" e "File generico" leggono CSV, XLSX, XLSM e XLS.
- La CLI continua a usare l'opzione --csv, ma ora accetta anche file Excel.
- Il formato CSV precedente resta compatibile.
- Nei file Excel viene letto il primo foglio utile; se ci sono piu' fogli non vuoti, viene usato quello con piu' righe.

Versione 3.0.0
- Nuova tab "CSV generico": legge CSV con intestazione e genera un file per ogni riga.
- Mapping libero: Colonna CSV -> Testo template da sostituire, es. 1->3M11, 2->12345, 3->cane.
- Colonna nome file selezionabile con checkbox, es. 1 genera 3M12.cprj, 3M13.cprj, ecc.
- Se "Nome file" e' spuntato e il testo template e' vuoto, quella colonna viene usata solo per nominare il file.
- La tab "Progetti da CSV" e' stata rinominata "Progetti DIGIS da CSV".

Versione 2.1.1
- Correzione LABEL_BCU: nella tab CSV viene sostituito l'intero contenuto dell'attributo text="", non solo il montante contenuto nella label.
- Esempio: text="BMUAS1 ... 3M11 DCO" puo' diventare text="ETICHETTA COMPLETA NUOVA AS1 3M12 XYZ".

Versione 2.1.0
- La tab "Sostituzioni Generiche" supporta anche un singolo file di testo come sorgente.
- Il rilevamento testo/binario usato per i .cprj viene applicato anche ai file singoli.
- Se il file singolo e' testuale, il tool genera una copia modificata nella cartella output.
- Se il file singolo non sembra testuale, il tool lo blocca e mostra errore.
- Se "Nome file output" e' vuoto e il nome sorgente non contiene il Find principale, il nome output diventa il valore di Replace con la stessa estensione del sorgente.
- La tab CSV supporta una colonna Label opzionale: vale solo per AS/AS1 e aggiorna LABEL_BCU in states.ccx.

Versione 2.0.0
- Il tool apre il .cprj come archivio e processa ricorsivamente tutti i file contenuti.
- Le sostituzioni vengono applicate a tutti i file rilevati come testo, indipendentemente dall'estensione.
- I file binari vengono copiati invariati.
- Le cartelle interne al .cprj vengono preservate e non vengono rinominate.
- Il log indica quali file testuali sono stati modificati e quante sostituzioni sono state applicate.

1) Template e Output
- Seleziona il .cprj template o la cartella che lo contiene.
- Output predefinito: cartella GENERATI accanto all'exe.

2) Tab "Sostituzioni Generiche"
- Montante template (Find): testo base da sostituire, es. 3M11.
- Ogni riga crea 1 progetto.
- Nuovo montante (Replace): es. 3M12.
- Nome file output (opz.): se vuoto usa il nuovo montante.
- Trova extra / Sostituisci con (opz.): es. PTOV -> TNOT.

3) Tab "Progetti DIGIS da file"
- Montante template (Find): es. 3M11.
- CSV: Montante;Dispositivo;IP;Label opzionale
- Il tool rileva il tipo progetto dal template (OP/AS1/EV) e aggiorna:
  - IP server nel rtu.ccx
  - IP client nei file IED_m61850_x.ccx rilevati dal template, per client OP/AS1/EV (se presenti)
  - LABEL_BCU in states.ccx per template AS/AS1, usando la Label della riga AS/AS1
- Quadriletterale base/nuovo (opz.): es. PTOV -> TNOT.

Esempio CSV:
Montante;Dispositivo;IP;Label
3M11;AS1;10.16.26.34;BMUAS1 LINEA 311 LACCHIARELLA 3M11 DCO
3M11;EV;10.16.26.37;
3M11;OP;10.16.26.41;

4) Tab "CSV generico"
- CSV senza intestazioni: tutte le righe sono dati.
- L'analisi mostra le colonne come C1, C2, C3 con anteprima del primo valore, es. C1: L465.
- L'analisi scrive nel log anche tutte le righe: Riga 1, Riga 2, Riga 3...
- Colonna CSV: puoi usare il numero posizione 1, 2, 3... oppure C1, C2, C3...
- Nome file: spunta la riga della colonna da usare per nominare il file generato.
- Se una riga ha Nome file spuntato e testo template vuoto, non cerca nulla nel template.
- Mapping:
  1 -> 3M11
  2 -> 12345
  3 -> cane

Esempio CSV generico:
3M12;67890;gatto
3M13;99999;cane
