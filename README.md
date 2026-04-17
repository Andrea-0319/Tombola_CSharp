# Tombola C Sharp

Progetto console in C# (net10.0) che simula una partita di tombola a classi, ispirato alla versione Python ma con cartella tradizionale 3x9.

## Funzionalita principali

- Cartelle tradizionali 3x9 con 15 numeri validi.
- Estrazione manuale (Invio) e rapida (`scorri`).
- Premi intermedi (`ambo`, `terna`, `quaterna`, `cinquina`) calcolati solo sulla stessa riga.
- Ogni premio intermedio viene assegnato una sola volta globalmente, con pari merito sullo stesso numero estratto.
- I premi mostrano il conteggio delle estrazioni necessarie al raggiungimento (non il numero estratto del premio).
- Comando `cartelle` per vedere lo stato di tutti i giocatori.
- Comando `tabellone` per vedere i numeri da 1 a 90 in 6 blocchi separati stile cartelle.
- Comando `premi` per vedere i premi assegnati fino a quel momento.
- Numeri gia usciti colorati in verde su cartelle e tabellone.
- Messaggi di tensione quando mancano 3 o meno numeri.
- Classifica progressiva e statistiche finali.
- Salvataggio report in `risultati_tombola/` con timestamp.

## Avvio

Dalla cartella del progetto:

```powershell
dotnet restore
dotnet run
```

All'inizio puoi scegliere se includere il giocatore speciale `Tabellone`:

- Nome fisso: `Tabellone`
- 6 cartelle speciali
- Copertura completa dei numeri 1-90 senza duplicati tra le sue cartelle
- Se attivo, basta 1 giocatore umano per iniziare la partita

## Comandi durante la partita

- Invio: estrae un numero.
- `scorri`: estrae in automatico finche nessuno e in zona tensione.
- `cartelle`: mostra tutte le cartelle dei giocatori.
- `tabellone`: mostra il tabellone a 6 blocchi con numeri usciti evidenziati.
- `premi`: mostra l'elenco dei premi intermedi gia assegnati.
- `help`: mostra guida rapida ai comandi.
