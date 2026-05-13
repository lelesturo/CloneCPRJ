CREA CPRJ MONTANTE

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
