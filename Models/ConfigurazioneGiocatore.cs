namespace Tombola.Models;

// DTO minimale per trasferire la configurazione iniziale dal Program al motore di gioco.
public sealed record ConfigurazioneGiocatore(string Nome, int NumeroCartelle, bool ETabellone = false);
