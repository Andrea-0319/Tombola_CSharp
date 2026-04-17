namespace Tombola.Models;

public sealed class Tabellone
{
    private readonly HashSet<int> _estratti = [];
    private readonly List<int> _ordineEstrazioni = [];

    public IReadOnlyList<int> OrdineEstrazioni => _ordineEstrazioni.AsReadOnly();

    public IReadOnlyList<int> EstrattiOrdinati => _estratti.OrderBy(n => n).ToList();

    public int TotaleEstratti => _ordineEstrazioni.Count;

    public int? UltimoEstratto => _ordineEstrazioni.Count == 0 ? null : _ordineEstrazioni[^1];

    public bool AggiungiNumeroEstratto(int numero)
    {
        if (numero < 1 || numero > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(numero), "Il numero deve essere compreso tra 1 e 90.");
        }

        if (!_estratti.Add(numero))
        {
            return false;
        }

        _ordineEstrazioni.Add(numero);
        return true;
    }

    public bool Contiene(int numero)
    {
        return _estratti.Contains(numero);
    }
}
