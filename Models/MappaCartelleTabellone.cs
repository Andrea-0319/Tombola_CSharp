namespace Tombola.Models;

public static class MappaCartelleTabellone
{
    public const int TotaleCartelle = 6;
    public const int RighePerCartella = 3;
    public const int ColonnePienePerCartella = 5;

    public static readonly int[] IniziCartelle = [1, 6, 31, 36, 61, 66];

    public static int NumeroDaPosizione(int indiceCartella, int riga, int colonnaInterna)
    {
        if (indiceCartella < 0 || indiceCartella >= TotaleCartelle)
        {
            throw new ArgumentOutOfRangeException(nameof(indiceCartella));
        }

        if (riga < 0 || riga >= RighePerCartella)
        {
            throw new ArgumentOutOfRangeException(nameof(riga));
        }

        if (colonnaInterna < 0 || colonnaInterna >= ColonnePienePerCartella)
        {
            throw new ArgumentOutOfRangeException(nameof(colonnaInterna));
        }

        return IniziCartelle[indiceCartella] + (riga * 10) + colonnaInterna;
    }
}