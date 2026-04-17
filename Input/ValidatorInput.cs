namespace Tombola.Input;

public enum SceltaPostPartita
{
    Termina = 1,
    RicominciaConStesseImpostazioni = 2,
    RiconfiguraGiocatori = 3
}

public static class ValidatorInput
{
    public static int LeggiNumeroGiocatori(int minimo, int massimo = 10)
    {
        if (minimo <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimo), "Il minimo deve essere positivo.");
        }

        if (massimo < minimo)
        {
            throw new ArgumentOutOfRangeException(nameof(massimo), "Il massimo non puo essere minore del minimo.");
        }

        while (true)
        {
            Console.Write($"Quanti giocatori umani? ({minimo}-{massimo}): ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var numero))
            {
                Console.WriteLine("Input non valido: inserisci un numero intero.");
                continue;
            }

            if (numero < minimo || numero > massimo)
            {
                Console.WriteLine($"Il numero di giocatori deve essere compreso tra {minimo} e {massimo}.");
                continue;
            }

            return numero;
        }
    }

    public static bool LeggiIncludiGiocatoreTabellone()
    {
        while (true)
        {
            Console.Write("Vuoi includere il giocatore speciale 'Tabellone' con 6 cartelle? (s/n): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

            switch (input)
            {
                case "s":
                case "si":
                case "y":
                case "yes":
                    return true;

                case "n":
                case "no":
                    return false;

                default:
                    Console.WriteLine("Risposta non valida. Inserisci 's' oppure 'n'.");
                    break;
            }
        }
    }

    public static string LeggiNomeGiocatore(int indice, ISet<string> nomiGiaUsati)
    {
        while (true)
        {
            Console.Write($"Nome giocatore {indice}: ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Il nome non puo essere vuoto.");
                continue;
            }

            if (nomiGiaUsati.Contains(input))
            {
                Console.WriteLine("Nome gia in uso, scegline un altro.");
                continue;
            }

            return input;
        }
    }

    public static int LeggiNumeroCartelle(string nomeGiocatore)
    {
        while (true)
        {
            Console.Write($"Numero cartelle per {nomeGiocatore} (1-6): ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out var numero))
            {
                Console.WriteLine("Input non valido: inserisci un numero intero.");
                continue;
            }

            if (numero is < 1 or > 6)
            {
                Console.WriteLine("Ogni giocatore deve avere da 1 a 6 cartelle.");
                continue;
            }

            return numero;
        }
    }

    public static SceltaPostPartita LeggiSceltaPostPartita()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("PARTITA CONCLUSA");
            Console.WriteLine("1) Termina il gioco");
            Console.WriteLine("2) Ricomincia con le stesse impostazioni");
            Console.WriteLine("3) Riconfigura giocatori e cartelle");
            Console.Write("Scelta (1-3): ");

            var input = Console.ReadLine();
            if (!int.TryParse(input, out var scelta) || scelta is < 1 or > 3)
            {
                Console.WriteLine("Scelta non valida. Inserisci 1, 2 oppure 3.");
                continue;
            }

            return (SceltaPostPartita)scelta;
        }
    }
}
