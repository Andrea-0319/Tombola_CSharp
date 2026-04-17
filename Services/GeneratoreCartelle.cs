using Tombola.Models;

namespace Tombola.Services;

public sealed class GeneratoreCartelle
{
    private const int NumeroCartelleTabellone = MappaCartelleTabellone.TotaleCartelle;

    public List<Cartella> CreaCartelle(int quantita)
    {
        if (quantita <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantita), "La quantita deve essere positiva.");
        }

        var risultato = new List<Cartella>(quantita);
        for (var i = 0; i < quantita; i++)
        {
            risultato.Add(CreaCartellaTradizionale());
        }

        return risultato;
    }

    public List<Cartella> CreaCartelleTabelloneCompleto()
    {
        var cartelle = new List<Cartella>(NumeroCartelleTabellone);

        for (var indiceCartella = 0; indiceCartella < NumeroCartelleTabellone; indiceCartella++)
        {
            var griglia = CreaGrigliaCartellaTabelloneCanonica(indiceCartella);
            cartelle.Add(new Cartella(griglia, ProfiloCartella.TabelloneCanonico));
        }

        ValidaAllineamentoCartelleTabellone(cartelle);
        return cartelle;
    }

    public Cartella CreaCartellaTradizionale()
    {
        var numeriPerColonna = GeneraNumeriPerColonna();
        var occupazioneRighe = GeneraOccupazioneRighe(numeriPerColonna);
        var griglia = new int?[3, 9];

        for (var colonna = 0; colonna < 9; colonna++)
        {
            var pool = CreaPoolColonna(colonna);
            MescolaInPlace(pool);
            var numeriScelti = pool
                .Take(numeriPerColonna[colonna])
                .OrderBy(n => n)
                .ToList();

            var righeDaPopolare = Enumerable.Range(0, 3)
                .Where(riga => occupazioneRighe[riga, colonna])
                .ToList();

            for (var i = 0; i < righeDaPopolare.Count; i++)
            {
                var riga = righeDaPopolare[i];
                griglia[riga, colonna] = numeriScelti[i];
            }
        }

        return new Cartella(griglia);
    }

    private static int[] GeneraNumeriPerColonna()
    {
        var numeriPerColonna = Enumerable.Repeat(1, 9).ToArray();
        var extraDaDistribuire = 6;

        // Partendo da 1 numero per colonna (9), distribuiamo altri 6 numeri fino ad arrivare a 15.
        while (extraDaDistribuire > 0)
        {
            var colonneDisponibili = Enumerable.Range(0, 9)
                .Where(c => numeriPerColonna[c] < 3)
                .ToList();

            var colonnaScelta = colonneDisponibili[Random.Shared.Next(colonneDisponibili.Count)];
            numeriPerColonna[colonnaScelta]++;
            extraDaDistribuire--;
        }

        return numeriPerColonna;
    }

    private static bool[,] GeneraOccupazioneRighe(int[] numeriPerColonna)
    {
        if (!TryGeneraOccupazioneRighe(numeriPerColonna, out var occupazione))
        {
            throw new InvalidOperationException("Impossibile generare una distribuzione valida 3x9.");
        }

        return occupazione;
    }

    private static bool TryGeneraOccupazioneRighe(int[] numeriPerColonna, out bool[,] occupazione)
    {
        var occupazioneLocale = new bool[3, 9];
        var conteggioPerRiga = new int[3];

        // Backtracking: per ogni colonna scegliamo quali righe occupare rispettando
        // sia i vincoli per colonna (1..3 numeri) sia i 5 numeri per riga.
        var successo = AssegnaColonna(0);
        occupazione = occupazioneLocale;
        return successo;

        bool AssegnaColonna(int colonna)
        {
            if (colonna == 9)
            {
                return conteggioPerRiga.All(c => c == 5);
            }

            var combinazioni = OttieniCombinazioniPerConteggio(numeriPerColonna[colonna]).ToList();
            MescolaInPlace(combinazioni);

            foreach (var combinazione in combinazioni)
            {
                if (combinazione.Any(riga => conteggioPerRiga[riga] >= 5))
                {
                    continue;
                }

                foreach (var riga in combinazione)
                {
                    occupazioneLocale[riga, colonna] = true;
                    conteggioPerRiga[riga]++;
                }

                if (RighePossonoAncoraChiudere(colonna + 1) && AssegnaColonna(colonna + 1))
                {
                    return true;
                }

                foreach (var riga in combinazione)
                {
                    occupazioneLocale[riga, colonna] = false;
                    conteggioPerRiga[riga]--;
                }
            }

            return false;
        }

        bool RighePossonoAncoraChiudere(int prossimaColonna)
        {
            var colonneRimanenti = 9 - prossimaColonna;

            for (var riga = 0; riga < 3; riga++)
            {
                if (conteggioPerRiga[riga] > 5)
                {
                    return false;
                }

                if (conteggioPerRiga[riga] + colonneRimanenti < 5)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private static int?[,] CreaGrigliaCartellaTabelloneCanonica(int indiceCartella)
    {
        var griglia = new int?[3, 9];

        for (var riga = 0; riga < MappaCartelleTabellone.RighePerCartella; riga++)
        {
            for (var colonna = 0; colonna < MappaCartelleTabellone.ColonnePienePerCartella; colonna++)
            {
                griglia[riga, colonna] = MappaCartelleTabellone.NumeroDaPosizione(indiceCartella, riga, colonna);
            }
        }

        return griglia;
    }

    private static void ValidaAllineamentoCartelleTabellone(IReadOnlyList<Cartella> cartelle)
    {
        if (cartelle.Count != NumeroCartelleTabellone)
        {
            throw new InvalidOperationException("Il Tabellone deve avere esattamente 6 cartelle canoniche.");
        }

        for (var indiceCartella = 0; indiceCartella < NumeroCartelleTabellone; indiceCartella++)
        {
            var cartella = cartelle[indiceCartella];

            for (var riga = 0; riga < MappaCartelleTabellone.RighePerCartella; riga++)
            {
                for (var colonna = 0; colonna < MappaCartelleTabellone.ColonnePienePerCartella; colonna++)
                {
                    var atteso = MappaCartelleTabellone.NumeroDaPosizione(indiceCartella, riga, colonna);
                    var numero = cartella.GetCella(riga, colonna);

                    if (numero != atteso)
                    {
                        throw new InvalidOperationException(
                            $"Cartella Tabellone {indiceCartella + 1} non allineata alla mappa canonica.");
                    }
                }

                for (var colonna = MappaCartelleTabellone.ColonnePienePerCartella; colonna < 9; colonna++)
                {
                    if (cartella.GetCella(riga, colonna).HasValue)
                    {
                        throw new InvalidOperationException(
                            $"Cartella Tabellone {indiceCartella + 1} contiene valori fuori dal layout canonico.");
                    }
                }
            }
        }

        if (!HaCoperturaCompletaSenzaDuplicati(cartelle))
        {
            throw new InvalidOperationException(
                "La copertura numerica delle cartelle Tabellone non e completa e univoca su 1..90.");
        }
    }

    private static bool HaCoperturaCompletaSenzaDuplicati(IReadOnlyList<Cartella> cartelle)
    {
        var numeriGlobali = new HashSet<int>();

        foreach (var cartella in cartelle)
        {
            foreach (var numero in cartella.Numeri)
            {
                if (!numeriGlobali.Add(numero))
                {
                    return false;
                }
            }
        }

        return numeriGlobali.Count == 90;
    }

    private static IEnumerable<int[]> OttieniCombinazioniPerConteggio(int conteggio)
    {
        return conteggio switch
        {
            1 =>
            [
                [0],
                [1],
                [2]
            ],
            2 =>
            [
                [0, 1],
                [0, 2],
                [1, 2]
            ],
            3 =>
            [
                [0, 1, 2]
            ],
            _ => throw new InvalidOperationException("Il conteggio per colonna deve essere compreso tra 1 e 3.")
        };
    }

    private static List<int> CreaPoolColonna(int colonna)
    {
        return colonna switch
        {
            0 => Enumerable.Range(1, 9).ToList(),
            8 => Enumerable.Range(80, 11).ToList(),
            _ => Enumerable.Range(colonna * 10, 10).ToList()
        };
    }

    private static void MescolaInPlace<T>(IList<T> valori)
    {
        for (var i = valori.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (valori[i], valori[j]) = (valori[j], valori[i]);
        }
    }
}
