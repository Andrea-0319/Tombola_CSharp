using Tombola.Infrastructure;
using Tombola.Models;
using Tombola.Services;
using Tombola.UI;

namespace Tombola.Tests;

public class PremiOutputTests
{
    [Fact]
    public void RenderingPremiAssegnati_UsaNumeroEstrazioni()
    {
        var renderer = new RenderingConsoleTombola();
        var premi = new List<PremioAssegnato>
        {
            new(
                TipoPremio.Ambo,
                12,
                new List<DettaglioPremio>
                {
                    new("Mario", 1, 2, new List<int> { 1, 2, 3, 4, 5 })
                })
        };

        var output = CaptureConsoleOutput(() => renderer.StampaPremiAssegnati(premi));

        Assert.Contains("PREMI ASSEGNATI FINORA", output);
        Assert.Contains("(dopo 12 estrazioni)", output);
    }

    [Fact]
    public void RenderingNuoviPremi_UsaNumeroEstrazioni()
    {
        var renderer = new RenderingConsoleTombola();
        var premi = new List<PremioAssegnato>
        {
            new(
                TipoPremio.Terna,
                23,
                new List<DettaglioPremio>
                {
                    new("Luigi", 2, 1, new List<int> { 11, 12, 13, 14, 15 })
                })
        };

        var output = CaptureConsoleOutput(() => renderer.StampaNuoviPremi(premi));

        Assert.Contains("PREMI ASSEGNATI IN QUESTO TURNO", output);
        Assert.Contains("assegnato dopo 23 estrazioni", output);
    }

    [Fact]
    public void ReportPremi_UsaNumeroEstrazioni()
    {
        var directoryTemporanea = Path.Combine(
            Path.GetTempPath(),
            "tombola-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryTemporanea);

        try
        {
            var tabellone = new Tabellone();
            for (var i = 1; i <= 12; i++)
            {
                tabellone.AggiungiNumeroEstratto(i);
            }

            var cartella = new GeneratoreCartelle().CreaCartellaTradizionale();
            var giocatore = new Giocatore("Mario", new List<Cartella> { cartella });
            var vincitori = new Dictionary<Giocatore, List<Cartella>>
            {
                [giocatore] = new List<Cartella> { cartella }
            };

            var premi = new List<PremioAssegnato>
            {
                new(
                    TipoPremio.Ambo,
                    12,
                    new List<DettaglioPremio>
                    {
                        new("Mario", 1, 2, new List<int> { 1, 2, 3, 4, 5 })
                    })
            };

            var gestoreReport = new GestoreReport(directoryTemporanea);
            var percorso = gestoreReport.SalvaReport(tabellone, vincitori, new List<Giocatore> { giocatore }, premi);
            var contenuto = File.ReadAllText(percorso);

            Assert.Contains("PREMI INTERMEDI ASSEGNATI", contenuto);
            Assert.Contains("(dopo 12 estrazioni)", contenuto);
        }
        finally
        {
            if (Directory.Exists(directoryTemporanea))
            {
                Directory.Delete(directoryTemporanea, recursive: true);
            }
        }
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var originale = Console.Out;

        try
        {
            using var writer = new StringWriter();
            Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originale);
        }
    }
}
