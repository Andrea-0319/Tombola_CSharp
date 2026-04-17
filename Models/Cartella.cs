namespace Tombola.Models;

public sealed class Cartella
{
    private const int NumeroTotaleCaselleConNumero = 15;
    private readonly int?[,] _griglia;
    private readonly ProfiloCartella _profilo;
    private readonly HashSet<int> _numeri = [];
    private readonly HashSet<int> _numeriSegnati = [];

    public Cartella(int?[,] griglia, ProfiloCartella profilo = ProfiloCartella.Standard)
    {
        if (griglia.GetLength(0) != 3 || griglia.GetLength(1) != 9)
        {
            throw new ArgumentException("La cartella deve avere formato 3x9.", nameof(griglia));
        }

        _profilo = profilo;
        _griglia = (int?[,])griglia.Clone();
        ValidaGrigliaEInizializzaNumeri();
    }

    public int NumeriSegnati => _numeriSegnati.Count;

    public int NumeriMancanti => NumeroTotaleCaselleConNumero - _numeriSegnati.Count;

    public bool EVincitrice => _numeriSegnati.Count == NumeroTotaleCaselleConNumero;

    public IReadOnlyCollection<int> Numeri => _numeri;

    public bool ContieneNumero(int numero)
    {
        return _numeri.Contains(numero);
    }

    public int? GetCella(int riga, int colonna)
    {
        ValidaIndiceRiga(riga);
        ValidaIndiceColonna(colonna);

        return _griglia[riga, colonna];
    }

    public bool TryTrovaRigaNumero(int numero, out int riga)
    {
        for (var indiceRiga = 0; indiceRiga < 3; indiceRiga++)
        {
            for (var colonna = 0; colonna < 9; colonna++)
            {
                if (_griglia[indiceRiga, colonna] != numero)
                {
                    continue;
                }

                riga = indiceRiga;
                return true;
            }
        }

        riga = -1;
        return false;
    }

    public int ConteggioSegnatiInRiga(int riga)
    {
        ValidaIndiceRiga(riga);

        var conteggio = 0;
        for (var colonna = 0; colonna < 9; colonna++)
        {
            var numero = _griglia[riga, colonna];
            if (numero.HasValue && _numeriSegnati.Contains(numero.Value))
            {
                conteggio++;
            }
        }

        return conteggio;
    }

    public IReadOnlyList<int> GetNumeriRiga(int riga)
    {
        ValidaIndiceRiga(riga);

        var numeri = new List<int>(5);
        for (var colonna = 0; colonna < 9; colonna++)
        {
            var numero = _griglia[riga, colonna];
            if (numero.HasValue)
            {
                numeri.Add(numero.Value);
            }
        }

        return numeri;
    }

    public bool SegnaNumero(int numero)
    {
        if (!_numeri.Contains(numero))
        {
            return false;
        }

        _numeriSegnati.Add(numero);
        return true;
    }

    public IReadOnlyList<int> GetNumeriRimanenti()
    {
        return _numeri
            .Where(n => !_numeriSegnati.Contains(n))
            .OrderBy(n => n)
            .ToList();
    }

    public string ToListaCompatta()
    {
        return string.Join(", ", _numeri.OrderBy(n => n).Select(n => n.ToString("00")));
    }

    private void ValidaGrigliaEInizializzaNumeri()
    {
        var totaleNumeri = 0;

        for (var riga = 0; riga < 3; riga++)
        {
            var numeriInRiga = 0;

            for (var colonna = 0; colonna < 9; colonna++)
            {
                var numero = _griglia[riga, colonna];
                if (!numero.HasValue)
                {
                    continue;
                }

                numeriInRiga++;
                totaleNumeri++;

                if (_profilo == ProfiloCartella.Standard)
                {
                    var (min, max) = IntervalloColonna(colonna);
                    if (numero.Value < min || numero.Value > max)
                    {
                        throw new InvalidOperationException(
                            $"Numero {numero.Value} fuori intervallo per colonna {colonna + 1} ({min}-{max}).");
                    }
                }

                if (!_numeri.Add(numero.Value))
                {
                    throw new InvalidOperationException($"Numero duplicato nella cartella: {numero.Value}.");
                }
            }

            if (numeriInRiga != 5)
            {
                throw new InvalidOperationException("Ogni riga della cartella deve contenere esattamente 5 numeri.");
            }
        }

        if (_profilo == ProfiloCartella.Standard)
        {
            // Ogni colonna deve avere da 1 a 3 numeri, ordinati dall'alto verso il basso.
            for (var colonna = 0; colonna < 9; colonna++)
            {
                var numeriInColonna = 0;
                var precedente = int.MinValue;

                for (var riga = 0; riga < 3; riga++)
                {
                    var numero = _griglia[riga, colonna];
                    if (!numero.HasValue)
                    {
                        continue;
                    }

                    numeriInColonna++;
                    if (numero.Value < precedente)
                    {
                        throw new InvalidOperationException("I numeri in colonna devono essere ordinati in modo crescente.");
                    }

                    precedente = numero.Value;
                }

                if (numeriInColonna < 1 || numeriInColonna > 3)
                {
                    throw new InvalidOperationException("Ogni colonna deve avere almeno 1 numero e al massimo 3 numeri.");
                }
            }
        }
        else if (_profilo == ProfiloCartella.TabelloneCanonico)
        {
            ValidaSchemaTabelloneCanonico();
        }

        if (totaleNumeri != NumeroTotaleCaselleConNumero)
        {
            throw new InvalidOperationException("La cartella deve contenere esattamente 15 numeri.");
        }
    }

    private void ValidaSchemaTabelloneCanonico()
    {
        for (var riga = 0; riga < 3; riga++)
        {
            for (var colonna = 0; colonna < 5; colonna++)
            {
                if (!_griglia[riga, colonna].HasValue)
                {
                    throw new InvalidOperationException(
                        "Una cartella Tabellone canonica deve avere le prime 5 colonne sempre valorizzate.");
                }
            }

            for (var colonna = 5; colonna < 9; colonna++)
            {
                if (_griglia[riga, colonna].HasValue)
                {
                    throw new InvalidOperationException(
                        "Una cartella Tabellone canonica deve avere le ultime 4 colonne vuote.");
                }
            }
        }

        for (var colonna = 0; colonna < 5; colonna++)
        {
            var precedente = int.MinValue;

            for (var riga = 0; riga < 3; riga++)
            {
                var numero = _griglia[riga, colonna]!.Value;
                if (numero < precedente)
                {
                    throw new InvalidOperationException(
                        "Le cartelle Tabellone canoniche devono avere colonne ordinate in modo crescente.");
                }

                precedente = numero;
            }
        }
    }

    private static (int Min, int Max) IntervalloColonna(int colonna)
    {
        return colonna switch
        {
            0 => (1, 9),
            8 => (80, 90),
            _ => (colonna * 10, colonna * 10 + 9)
        };
    }

    private static void ValidaIndiceRiga(int riga)
    {
        if (riga < 0 || riga >= 3)
        {
            throw new ArgumentOutOfRangeException(nameof(riga));
        }
    }

    private static void ValidaIndiceColonna(int colonna)
    {
        if (colonna < 0 || colonna >= 9)
        {
            throw new ArgumentOutOfRangeException(nameof(colonna));
        }
    }
}
