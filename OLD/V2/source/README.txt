CREA CPRJ MONTANTE

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

3) Tab "Progetti da CSV"
- Montante template (Find): es. 3M11.
- CSV: Montante;Dispositivo;IP
- Il tool rileva il tipo progetto dal template (OP/AS1/EV) e aggiorna:
  - IP server nel rtu.ccx
  - IP client nei file IED_m61850_x.ccx rilevati dal template, per client OP/AS1/EV (se presenti)
- Quadriletterale base/nuovo (opz.): es. PTOV -> TNOT.

Esempio CSV:
Montante;Dispositivo;IP
3M11;AS1;10.16.26.34
3M11;EV;10.16.26.37
3M11;OP;10.16.26.41
