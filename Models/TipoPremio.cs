namespace Tombola.Models;

public enum TipoPremio
{
    Ambo = 2,
    Terna = 3,
    Quaterna = 4,
    Cinquina = 5
}

public static class TipoPremioExtensions
{
    public static string ToEtichetta(this TipoPremio tipoPremio)
    {
        return tipoPremio switch
        {
            TipoPremio.Ambo => "AMBO",
            TipoPremio.Terna => "TERNA",
            TipoPremio.Quaterna => "QUATERNA",
            TipoPremio.Cinquina => "CINQUINA",
            _ => tipoPremio.ToString().ToUpperInvariant()
        };
    }
}