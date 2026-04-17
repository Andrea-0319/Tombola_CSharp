using Tombola.Models;

namespace Tombola.Infrastructure;

public sealed class GestoreReport
{
    private readonly string _cartellaOutput;
    private const int LarghezzaSeparatore = 70;

    private static readonly string SeparatorePrincipale = new('=', LarghezzaSeparatore);
    private static readonly string SeparatoreSezione = new('-', LarghezzaSeparatore);

    public GestoreReport(string? cartellaOutput = null)
    {
        _cartellaOutput = cartellaOutput ?? Path.Combine(Environment.CurrentDirectory, "risultati_tombola");
    }

    public string SalvaReport(
        Tabellone tabellone,
        Dictionary<Giocatore, List<Cartella>> vincitori,
        IReadOnlyList<Giocatore> giocatori,
        IReadOnlyList<PremioAssegnato> premiAssegnati)
    {
        Directory.CreateDirectory(_cartellaOutput);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var percorso = Path.Combine(_cartellaOutput, $"risultati_{timestamp}.txt");

        using var writer = new StreamWriter(percorso);

        writer.WriteLine("RISULTATO PARTITA TOMBOLA");
        writer.WriteLine(SeparatorePrincipale);
        writer.WriteLine($"Data: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine($"Totale estrazioni: {tabellone.TotaleEstratti}");

        if (tabellone.UltimoEstratto is int ultimo)
        {
            writer.WriteLine($"Numero vincente: {FormattaNumero(ultimo)}");
        }

        writer.WriteLine();
        writer.WriteLine(vincitori.Count == 1 ? "VINCITORE" : "VINCITORI");

        foreach (var entry in vincitori)
        {
            writer.WriteLine($"- {entry.Key.Nome}");
            foreach (var cartella in entry.Value)
            {
                writer.WriteLine($"  Cartella vincente: {cartella.ToListaCompatta()}");
            }
        }

        writer.WriteLine();
        writer.WriteLine("PREMI INTERMEDI ASSEGNATI");
        writer.WriteLine(SeparatoreSezione);

        if (premiAssegnati.Count == 0)
        {
            writer.WriteLine("Nessun premio intermedio assegnato.");
        }
        else
        {
            for (var indice = 0; indice < premiAssegnati.Count; indice++)
            {
                var premio = premiAssegnati[indice];
                writer.WriteLine(
                    $"{indice + 1}. {premio.Tipo.ToEtichetta()} (dopo {premio.NumeroEstrazioni} estrazioni)");

                foreach (var dettaglio in premio.Vincitori)
                {
                    var numeriRiga = string.Join(", ", dettaglio.NumeriRiga.Select(FormattaNumero));
                    writer.WriteLine(
                        $"   - {dettaglio.NomeGiocatore} | Cartella {dettaglio.NumeroCartella}, Riga {dettaglio.NumeroRiga} [{numeriRiga}]");
                }
            }
        }

        writer.WriteLine();
        writer.WriteLine("NUMERI ESTRATTI (ordine cronologico)");
        writer.WriteLine(FormattaNumeri(tabellone.OrdineEstrazioni));

        writer.WriteLine();
        writer.WriteLine("NUMERI ESTRATTI (ordinati)");
        writer.WriteLine(FormattaNumeri(tabellone.EstrattiOrdinati));

        var statistiche = giocatori
            .Select(g =>
            {
                var migliore = g.CartellaPiuVicina();
                return (Nome: g.Nome, Mancanti: migliore.Mancanti, CartelleVincenti: g.CartelleVincentiTotali);
            })
            .OrderBy(x => x.Mancanti)
            .ThenByDescending(x => x.CartelleVincenti)
            .ThenBy(x => x.Nome, StringComparer.OrdinalIgnoreCase)
            .ToList();

        writer.WriteLine();
        writer.WriteLine("STATISTICHE FINALI");
        writer.WriteLine(SeparatoreSezione);

        for (var i = 0; i < statistiche.Count; i++)
        {
            var voce = statistiche[i];
            writer.WriteLine(
                $"{i + 1}. {voce.Nome} | Mancanti migliori: {voce.Mancanti} | Cartelle vinte: {voce.CartelleVincenti}");
        }

        return percorso;
    }

    private static string FormattaNumeri(IEnumerable<int> numeri)
    {
        return string.Join(", ", numeri.Select(FormattaNumero));
    }

    private static string FormattaNumero(int numero)
    {
        return numero.ToString("00");
    }
}
