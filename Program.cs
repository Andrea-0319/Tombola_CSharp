using Tombola.Infrastructure;
using Tombola.Input;
using Tombola.Models;
using Tombola.Services;
using Tombola.UI;

namespace Tombola;

internal static class Program
{
    private const string NomeGiocatoreTabellone = "Tabellone";

    private static void Main()
    {
        Console.Title = "Tombola C#";
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("CONFIGURAZIONE PARTITA");
        Console.WriteLine(new string('-', 40));

        var configurazioni = RaccogliConfigurazioniGiocatori();

        var generatoreCartelle = new GeneratoreCartelle();
        var gestoreReport = new GestoreReport();
        var renderer = new RenderingConsoleTombola();

        var gioco = new TombolaGame(configurazioni, generatoreCartelle, gestoreReport, renderer);
        gioco.Avvia();
    }

    private static List<ConfigurazioneGiocatore> RaccogliConfigurazioniGiocatori()
    {
        var includeTabellone = ValidatorInput.LeggiIncludiGiocatoreTabellone();
        var minimoGiocatoriUmani = includeTabellone ? 1 : 2;
        var numeroGiocatori = ValidatorInput.LeggiNumeroGiocatori(minimoGiocatoriUmani, 10);

        var nomiGiaUsati = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (includeTabellone)
        {
            nomiGiaUsati.Add(NomeGiocatoreTabellone);
        }

        var configurazioni = new List<ConfigurazioneGiocatore>(numeroGiocatori);

        for (var i = 1; i <= numeroGiocatori; i++)
        {
            var nome = ValidatorInput.LeggiNomeGiocatore(i, nomiGiaUsati);
            nomiGiaUsati.Add(nome);

            var numeroCartelle = ValidatorInput.LeggiNumeroCartelle(nome);
            configurazioni.Add(new ConfigurazioneGiocatore(nome, numeroCartelle, ETabellone: false));
        }

        if (includeTabellone)
        {
            configurazioni.Add(new ConfigurazioneGiocatore(NomeGiocatoreTabellone, 6, ETabellone: true));
        }

        return configurazioni;
    }
}
