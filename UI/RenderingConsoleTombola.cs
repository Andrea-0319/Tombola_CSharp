using Tombola.Models;

namespace Tombola.UI;

public sealed class RenderingConsoleTombola
{
    private const ConsoleColor ColoreNumeroEstratto = ConsoleColor.Green;
    private const int CellePerBloccoTabellone = MappaCartelleTabellone.ColonnePienePerCartella;
    private const int RighePerBloccoTabellone = MappaCartelleTabellone.RighePerCartella;
    private const int LarghezzaBloccoTabellone = 26;
    private const int SpaziaturaBlocchiTabellone = 4;
    private const int LarghezzaSeparatorePrincipale = 70;
    private const int LarghezzaSeparatoreClassifica = 40;

    private static readonly string SeparatorePrincipale = new('=', LarghezzaSeparatorePrincipale);
    private static readonly string SeparatorePremio = new('*', LarghezzaSeparatorePrincipale);
    private static readonly string SeparatoreTensione = new('!', LarghezzaSeparatorePrincipale);
    private static readonly string SeparatoreClassifica = new('-', LarghezzaSeparatoreClassifica);
    private static readonly string SeparatoreBloccoTabelloneSingolo =
        "+" + string.Concat(Enumerable.Repeat("----+", CellePerBloccoTabellone));

    private static readonly int[][] LayoutBlocchiTabellone =
    [
        [0, 2, 4],
        [1, 3, 5]
    ];

    public void PulisciSchermo()
    {
        Console.Clear();
    }

    public void StampaBenvenuto(IReadOnlyList<Giocatore> giocatori)
    {
        Console.WriteLine(SeparatorePrincipale);
        Console.WriteLine("BENVENUTI ALLA TOMBOLA IN C#");
        Console.WriteLine(SeparatorePrincipale);
        Console.WriteLine($"Giocatori: {string.Join(", ", giocatori.Select(g => g.Nome))}");
        Console.WriteLine("Comandi disponibili: Invio, scorri, cartelle, tabellone, premi, help");
        Console.WriteLine(SeparatorePrincipale);
    }

    public void StampaStatoEstrazioni(Tabellone tabellone)
    {
        if (tabellone.TotaleEstratti == 0)
        {
            Console.WriteLine("Nessun numero estratto.");
            return;
        }

        Console.WriteLine($"Ultimo numero estratto: {tabellone.UltimoEstratto:00}");
        Console.WriteLine($"Totale estrazioni: {tabellone.TotaleEstratti}");
        Console.WriteLine($"Numeri usciti: {string.Join(", ", tabellone.EstrattiOrdinati.Select(FormattaNumero))}");
    }

    public string ChiediComando()
    {
        Console.WriteLine();
        Console.Write("Premi Invio per estrarre, oppure digita un comando (help per lista): ");
        return Console.ReadLine() ?? string.Empty;
    }

    public void StampaNumeroEstratto(int numero)
    {
        Console.WriteLine();
        Console.Write(">>> Numero estratto: ");
        ScriviNumeroColorato(numero, evidenzia: true);
        Console.WriteLine(" <<<");
    }

    public void StampaMessaggio(string messaggio)
    {
        Console.WriteLine(messaggio);
    }

    public void StampaHelp()
    {
        Console.WriteLine();
        Console.WriteLine("HELP COMANDI");
        Console.WriteLine("- Invio: estrae un numero");
        Console.WriteLine("- scorri: estrazione rapida finche qualcuno e' vicino alla tombola");
        Console.WriteLine("- cartelle: mostra tutte le cartelle dei giocatori");
        Console.WriteLine("- tabellone / mostra tabellone: mostra il tabellone a 6 blocchi (numeri usciti in verde)");
        Console.WriteLine("- premi: mostra i premi assegnati fino a questo momento");
        Console.WriteLine("- help: mostra questo aiuto");
    }

    public void StampaTabellone(Tabellone tabellone)
    {
        Console.WriteLine();
        Console.WriteLine(SeparatorePrincipale);
        Console.WriteLine("TABELLONE");
        Console.WriteLine("(Numeri usciti in verde)");
        Console.WriteLine(SeparatorePrincipale);

        foreach (var rigaBlocchi in LayoutBlocchiTabellone)
        {
            StampaIntestazioniBlocchiTabellone(rigaBlocchi);
            StampaSeparatoreBlocchiTabellone(rigaBlocchi.Length);

            for (var rigaInterna = 0; rigaInterna < RighePerBloccoTabellone; rigaInterna++)
            {
                StampaRigaBlocchiTabellone(rigaBlocchi, rigaInterna, tabellone);
                StampaSeparatoreBlocchiTabellone(rigaBlocchi.Length);
            }

            Console.WriteLine();
        }

        Console.WriteLine(SeparatorePrincipale);
    }

    public void StampaNuoviPremi(IReadOnlyList<PremioAssegnato> nuoviPremi)
    {
        if (nuoviPremi.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine(SeparatorePremio);
        Console.WriteLine("PREMI ASSEGNATI IN QUESTO TURNO");
        Console.WriteLine(SeparatorePremio);

        foreach (var premio in nuoviPremi)
        {
            Console.WriteLine($"{premio.Tipo.ToEtichetta()} assegnato dopo {premio.NumeroEstrazioni} estrazioni:");
            foreach (var dettaglio in premio.Vincitori)
            {
                Console.WriteLine($"- {FormattaDettaglioPremio(dettaglio)}");
            }

            Console.WriteLine();
        }
    }

    public void StampaPremiAssegnati(IReadOnlyList<PremioAssegnato> premiAssegnati)
    {
        Console.WriteLine();
        Console.WriteLine(SeparatorePrincipale);
        Console.WriteLine("PREMI ASSEGNATI FINORA");
        Console.WriteLine(SeparatorePrincipale);

        if (premiAssegnati.Count == 0)
        {
            Console.WriteLine("Nessun premio assegnato fino a questo momento.");
            return;
        }

        for (var i = 0; i < premiAssegnati.Count; i++)
        {
            var premio = premiAssegnati[i];
            Console.WriteLine($"{i + 1}. {premio.Tipo.ToEtichetta()} (dopo {premio.NumeroEstrazioni} estrazioni)");
            foreach (var dettaglio in premio.Vincitori)
            {
                Console.WriteLine($"   - {FormattaDettaglioPremio(dettaglio)}");
            }
        }
    }

    public void StampaStatoGiocatori(IReadOnlyList<Giocatore> giocatori, Tabellone tabellone)
    {
        Console.WriteLine();
        Console.WriteLine(SeparatorePrincipale);
        Console.WriteLine("STATO CARTELLE");
        Console.WriteLine("(Numeri usciti in verde)");
        Console.WriteLine(SeparatorePrincipale);

        foreach (var giocatore in giocatori)
        {
            Console.WriteLine();
            Console.WriteLine($"Giocatore: {giocatore.Nome}");
            Console.WriteLine(new string('-', LarghezzaSeparatorePrincipale));

            for (var indice = 0; indice < giocatore.Cartelle.Count; indice++)
            {
                var cartella = giocatore.Cartelle[indice];
                Console.WriteLine($"Cartella {indice + 1} - Mancano {cartella.NumeriMancanti} numeri");
                StampaSingolaCartella(cartella, tabellone);
            }
        }
    }

    public void StampaTensione(IReadOnlyList<(string Nome, int Mancanti)> inTensione)
    {
        if (inTensione.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine(SeparatoreTensione);
        Console.WriteLine("TENSIONE!");

        var gruppi = inTensione
            .GroupBy(x => x.Mancanti)
            .OrderBy(g => g.Key);

        foreach (var gruppo in gruppi)
        {
            var nomi = string.Join(", ", gruppo.Select(x => x.Nome));
            if (gruppo.Key == 1)
            {
                Console.WriteLine($"A {nomi} manca SOLO 1 numero!");
            }
            else
            {
                Console.WriteLine($"A {nomi} mancano {gruppo.Key} numeri.");
            }
        }

        Console.WriteLine(SeparatoreTensione);
    }

    public void StampaClassifica(IReadOnlyList<(string Nome, int Mancanti)> classifica)
    {
        Console.WriteLine();
        Console.WriteLine("CLASSIFICA (cartella piu vicina)");
        Console.WriteLine(SeparatoreClassifica);

        for (var i = 0; i < classifica.Count; i++)
        {
            var voce = classifica[i];
            if (voce.Mancanti == 0)
            {
                Console.WriteLine($"{i + 1}. {voce.Nome} - HA VINTO");
            }
            else if (voce.Mancanti == 1)
            {
                Console.WriteLine($"{i + 1}. {voce.Nome} - manca 1 numero");
            }
            else
            {
                Console.WriteLine($"{i + 1}. {voce.Nome} - mancano {voce.Mancanti} numeri");
            }
        }

        Console.WriteLine(SeparatoreClassifica);
    }

    public void StampaVincitori(Dictionary<Giocatore, List<Cartella>> vincitori, Tabellone tabellone)
    {
        Console.WriteLine(SeparatorePrincipale);
        Console.WriteLine("VITTORIA!");
        Console.WriteLine(SeparatorePrincipale);

        if (tabellone.UltimoEstratto is int ultimo)
        {
            Console.Write("Numero vincente: ");
            ScriviNumeroColorato(ultimo, evidenzia: true);
            Console.WriteLine();
        }

        if (vincitori.Count == 1)
        {
            Console.WriteLine($"Vincitore: {vincitori.Keys.Single().Nome}");
        }
        else
        {
            Console.WriteLine($"Vincitori: {string.Join(", ", vincitori.Keys.Select(v => v.Nome))}");
        }

        foreach (var entry in vincitori)
        {
            Console.WriteLine();
            Console.WriteLine($"{entry.Key.Nome} ha vinto con {entry.Value.Count} cartella/e:");

            foreach (var cartella in entry.Value)
            {
                Console.WriteLine($"- {cartella.ToListaCompatta()}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Totale estrazioni: {tabellone.TotaleEstratti}");
    }

    public void StampaStatisticheFinali(IReadOnlyList<(string Nome, int Mancanti, int CartelleVincenti)> statistiche)
    {
        Console.WriteLine();
        Console.WriteLine("STATISTICHE FINALI");
        Console.WriteLine(new string('-', LarghezzaSeparatorePrincipale));

        for (var i = 0; i < statistiche.Count; i++)
        {
            var voce = statistiche[i];
            Console.WriteLine(
                $"{i + 1}. {voce.Nome} | Numeri mancanti : {voce.Mancanti} | Cartelle vincenti: {voce.CartelleVincenti}");
        }

        Console.WriteLine(new string('-', LarghezzaSeparatorePrincipale));
    }

    public void AttendiInvio(string messaggio = "Premi Invio per continuare...")
    {
        Console.WriteLine();
        Console.Write(messaggio);
        Console.ReadLine();
    }

    private static void StampaIntestazioniBlocchiTabellone(IReadOnlyList<int> indiciBlocco)
    {
        for (var indice = 0; indice < indiciBlocco.Count; indice++)
        {
            var indiceBlocco = indiciBlocco[indice];
            var inizio = MappaCartelleTabellone.IniziCartelle[indiceBlocco];
            var fine = inizio + 24;
            var etichetta = $"Cartella {indiceBlocco + 1} ({inizio:00}-{fine:00})";

            Console.Write(etichetta.PadRight(LarghezzaBloccoTabellone));
            if (indice < indiciBlocco.Count - 1)
            {
                Console.Write(new string(' ', SpaziaturaBlocchiTabellone));
            }
        }

        Console.WriteLine();
    }

    private static void StampaSeparatoreBlocchiTabellone(int numeroBlocchi)
    {
        for (var indice = 0; indice < numeroBlocchi; indice++)
        {
            Console.Write(SeparatoreBloccoTabelloneSingolo);
            if (indice < numeroBlocchi - 1)
            {
                Console.Write(new string(' ', SpaziaturaBlocchiTabellone));
            }
        }

        Console.WriteLine();
    }

    private static void StampaRigaBlocchiTabellone(IReadOnlyList<int> indiciBlocco, int rigaInterna, Tabellone tabellone)
    {
        for (var indice = 0; indice < indiciBlocco.Count; indice++)
        {
            var indiceBlocco = indiciBlocco[indice];
            Console.Write("|");

            for (var colonnaInterna = 0; colonnaInterna < CellePerBloccoTabellone; colonnaInterna++)
            {
                var numero = NumeroDaBloccoTabellone(indiceBlocco, rigaInterna, colonnaInterna);
                Console.Write(" ");
                ScriviNumeroColorato(numero, tabellone.Contiene(numero));
                Console.Write(" |");
            }

            if (indice < indiciBlocco.Count - 1)
            {
                Console.Write(new string(' ', SpaziaturaBlocchiTabellone));
            }
        }

        Console.WriteLine();
    }

    private static int NumeroDaBloccoTabellone(int indiceBlocco, int rigaInterna, int colonnaInterna)
    {
        return MappaCartelleTabellone.NumeroDaPosizione(indiceBlocco, rigaInterna, colonnaInterna);
    }

    private static string FormattaDettaglioPremio(DettaglioPremio dettaglio)
    {
        var numeriRiga = string.Join(", ", dettaglio.NumeriRiga.Select(FormattaNumero));
        return
            $"{dettaglio.NomeGiocatore} | Cartella {dettaglio.NumeroCartella}, Riga {dettaglio.NumeroRiga} [{numeriRiga}]";
    }

    private static void StampaSingolaCartella(Cartella cartella, Tabellone tabellone)
    {
        var separatore = "+" + string.Concat(Enumerable.Repeat("----+", 9));
        Console.WriteLine(separatore);

        for (var riga = 0; riga < 3; riga++)
        {
            Console.Write("|");
            for (var colonna = 0; colonna < 9; colonna++)
            {
                var numero = cartella.GetCella(riga, colonna);
                ScriviCella(numero, tabellone);
                Console.Write("|");
            }

            Console.WriteLine();
            Console.WriteLine(separatore);
        }
    }

    private static void ScriviCella(int? numero, Tabellone tabellone)
    {
        Console.Write(" ");
        if (!numero.HasValue)
        {
            Console.Write("..");
        }
        else
        {
            ScriviNumeroColorato(numero.Value, tabellone.Contiene(numero.Value));
        }

        Console.Write(" ");
    }

    private static void ScriviNumeroColorato(int numero, bool evidenzia)
    {
        if (!evidenzia)
        {
            Console.Write(FormattaNumero(numero));
            return;
        }

        var colorePrecedente = Console.ForegroundColor;
        Console.ForegroundColor = ColoreNumeroEstratto;
        Console.Write(FormattaNumero(numero));
        Console.ForegroundColor = colorePrecedente;
    }

    private static string FormattaNumero(int numero)
    {
        return numero.ToString("00");
    }
}
