namespace Tombola.Models;

public sealed record DettaglioPremio(
    string NomeGiocatore,
    int NumeroCartella,
    int NumeroRiga,
    IReadOnlyList<int> NumeriRiga);

public sealed record PremioAssegnato(
    TipoPremio Tipo,
    int NumeroEstrazioni,
    IReadOnlyList<DettaglioPremio> Vincitori);