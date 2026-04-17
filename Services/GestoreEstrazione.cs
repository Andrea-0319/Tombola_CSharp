namespace Tombola.Services;

public sealed class GestoreEstrazione
{
    private readonly List<int> _sequenza;
    private int _indice;

    public GestoreEstrazione()
    {
        _sequenza = Enumerable.Range(1, 90).ToList();
        MescolaInPlace(_sequenza);
        _indice = 0;
    }

    public bool HaNumeriDisponibili => _indice < _sequenza.Count;

    public int EstraiProssimoNumero()
    {
        if (!HaNumeriDisponibili)
        {
            throw new InvalidOperationException("Non ci sono piu numeri da estrarre.");
        }

        var numero = _sequenza[_indice];
        _indice++;
        return numero;
    }

    private static void MescolaInPlace(List<int> numeri)
    {
        // Fisher-Yates: garantisce uno shuffle uniforme dell'intera sequenza.
        for (var i = numeri.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (numeri[i], numeri[j]) = (numeri[j], numeri[i]);
        }
    }
}
