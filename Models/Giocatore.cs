namespace Tombola.Models;

public sealed class Giocatore
{
    private readonly List<Cartella> _cartelle;

    public Giocatore(string nome, IEnumerable<Cartella> cartelle)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Il nome del giocatore non puo essere vuoto.", nameof(nome));
        }

        _cartelle = cartelle.ToList();
        if (_cartelle.Count == 0)
        {
            throw new ArgumentException("Ogni giocatore deve avere almeno una cartella.", nameof(cartelle));
        }

        Nome = nome.Trim();
    }

    public string Nome { get; }

    public IReadOnlyList<Cartella> Cartelle => _cartelle;

    public int CartelleVincentiTotali => _cartelle.Count(c => c.EVincitrice);

    public (Cartella Cartella, int Mancanti) CartellaPiuVicina()
    {
        var migliore = _cartelle[0];

        for (var i = 1; i < _cartelle.Count; i++)
        {
            if (_cartelle[i].NumeriMancanti < migliore.NumeriMancanti)
            {
                migliore = _cartelle[i];
            }
        }

        return (migliore, migliore.NumeriMancanti);
    }
}
