using Tombola.Infrastructure;
using Tombola.Models;
using Tombola.UI;

namespace Tombola.Services;

public sealed class TombolaGame
{
    private static readonly TipoPremio[] OrdinePremi =
    [
        TipoPremio.Ambo,
        TipoPremio.Terna,
        TipoPremio.Quaterna,
        TipoPremio.Cinquina
    ];

    private const int SogliaTensione = 3;
    private const string NomeGiocatoreTabellone = "Tabellone";

    private readonly List<Giocatore> _giocatori;
    private readonly GestoreEstrazione _gestoreEstrazione = new();
    private readonly Tabellone _tabellone = new();
    private readonly RenderingConsoleTombola _renderer;
    private readonly GestoreReport _gestoreReport;
    private readonly HashSet<string> _chiaviTensioneGiaMostrate = [];
    private readonly HashSet<TipoPremio> _premiAssegnati = [];
    private readonly List<PremioAssegnato> _storicoPremi = [];

    public TombolaGame(
        IEnumerable<ConfigurazioneGiocatore> configurazioni,
        GeneratoreCartelle generatoreCartelle,
        GestoreReport gestoreReport,
        RenderingConsoleTombola renderer)
    {
        _renderer = renderer;
        _gestoreReport = gestoreReport;

        var configurazioniPartita = configurazioni.ToList();
        var giocatoriUmani = configurazioniPartita.Count(config => !config.ETabellone);
        var tabelloni = configurazioniPartita.Count(config => config.ETabellone);

        if (tabelloni > 1)
        {
            throw new InvalidOperationException("Puoi includere al massimo un giocatore Tabellone.");
        }

        if (giocatoriUmani < 1)
        {
            throw new InvalidOperationException("Serve almeno un giocatore umano per avviare la partita.");
        }

        if (giocatoriUmani + tabelloni < 2)
        {
            throw new InvalidOperationException("Servono almeno due partecipanti totali (umani e/o Tabellone).");
        }

        _giocatori = configurazioniPartita
            .Select(config =>
            {
                var cartelle = config.ETabellone
                    ? generatoreCartelle.CreaCartelleTabelloneCompleto()
                    : generatoreCartelle.CreaCartelle(config.NumeroCartelle);

                var nomeGiocatore = config.ETabellone ? NomeGiocatoreTabellone : config.Nome;
                return new Giocatore(nomeGiocatore, cartelle);
            })
            .ToList();

        var nomiDuplicati = _giocatori
            .GroupBy(giocatore => giocatore.Nome, StringComparer.OrdinalIgnoreCase)
            .Where(gruppo => gruppo.Count() > 1)
            .Select(gruppo => gruppo.Key)
            .ToList();

        if (nomiDuplicati.Count > 0)
        {
            throw new InvalidOperationException(
                $"Nomi giocatori duplicati non ammessi: {string.Join(", ", nomiDuplicati)}.");
        }
    }

    public void Avvia()
    {
        _renderer.PulisciSchermo();
        _renderer.StampaBenvenuto(_giocatori);

        while (_gestoreEstrazione.HaNumeriDisponibili)
        {
            _renderer.StampaStatoEstrazioni(_tabellone);
            var comando = _renderer.ChiediComando();

            if (string.IsNullOrWhiteSpace(comando))
            {
                if (EseguiEstrazioneSingola())
                {
                    return;
                }

                _renderer.AttendiInvio();
                _renderer.PulisciSchermo();
                continue;
            }

            switch (comando.Trim().ToLowerInvariant())
            {
                case "scorri":
                    if (EseguiScorrimentoRapido())
                    {
                        return;
                    }

                    _renderer.AttendiInvio();
                    _renderer.PulisciSchermo();
                    break;

                case "cartelle":
                    MostraCartellePulite();
                    break;

                case "tabellone":
                case "mostra tabellone":
                    MostraTabellonePulito();
                    break;

                case "premi":
                    _renderer.PulisciSchermo();
                    _renderer.StampaPremiAssegnati(_storicoPremi);
                    _renderer.AttendiInvio();
                    _renderer.PulisciSchermo();
                    break;

                case "stato":
                    MostraStatoSintetico();
                    break;

                case "restart":
                case "esci":
                    _renderer.PulisciSchermo();
                    _renderer.StampaMessaggio("Partita interrotta: apro il menu di fine partita.");
                    return;

                case "help":
                    _renderer.StampaHelp();
                    _renderer.AttendiInvio();
                    _renderer.PulisciSchermo();
                    break;

                default:
                    _renderer.StampaMessaggio("Comando non riconosciuto. Digita 'help' per la lista comandi.");
                    _renderer.AttendiInvio();
                    _renderer.PulisciSchermo();
                    break;
            }
        }

        _renderer.StampaMessaggio("Partita terminata: non ci sono piu numeri disponibili.");
    }

    private bool EseguiEstrazioneSingola()
    {
        var numero = _gestoreEstrazione.EstraiProssimoNumero();
        _tabellone.AggiungiNumeroEstratto(numero);
        _renderer.StampaNumeroEstratto(numero);

        var esitoTurno = ProcessaEstrazione(numero);
        if (esitoTurno.NuoviPremi.Count > 0)
        {
            _renderer.StampaNuoviPremi(esitoTurno.NuoviPremi);
        }

        if (esitoTurno.Vincitori.Count > 0)
        {
            ConcludiPartita(esitoTurno.Vincitori);
            return true;
        }

        ControllaTensioneEClassifica();
        return false;
    }

    private bool EseguiScorrimentoRapido()
    {
        if (QualcunoInTensione())
        {
            _renderer.StampaMessaggio("Scorrimento fermo: c'e gia almeno un giocatore in tensione (meno di 3 numeri mancanti).");
            return false;
        }

        _renderer.StampaMessaggio("Scorrimento avviato...");
        var estrattiInRapida = 0;

        while (!QualcunoInTensione() && _gestoreEstrazione.HaNumeriDisponibili)
        {
            var numero = _gestoreEstrazione.EstraiProssimoNumero();
            _tabellone.AggiungiNumeroEstratto(numero);
            estrattiInRapida++;

            var esitoTurno = ProcessaEstrazione(numero);
            if (esitoTurno.NuoviPremi.Count > 0)
            {
                _renderer.StampaNuoviPremi(esitoTurno.NuoviPremi);
            }

            if (esitoTurno.Vincitori.Count > 0)
            {
                _renderer.StampaMessaggio($"Scorrimento interrotto dopo {estrattiInRapida} estrazioni per vittoria.");
                ConcludiPartita(esitoTurno.Vincitori);
                return true;
            }
        }

        if (estrattiInRapida == 0)
        {
            _renderer.StampaMessaggio("Nessun numero estratto in modalita rapida.");
        }
        else
        {
            _renderer.StampaMessaggio($"Modalita rapida: estratti {estrattiInRapida} numeri.");
            if (_tabellone.UltimoEstratto is int ultimo)
            {
                _renderer.StampaMessaggio($"Ultimo numero estratto: {ultimo:00}");
            }
        }

        ControllaTensioneEClassifica();
        return false;
    }

    private (Dictionary<Giocatore, List<Cartella>> Vincitori, List<PremioAssegnato> NuoviPremi) ProcessaEstrazione(
        int numero)
    {
        var vincitori = new Dictionary<Giocatore, List<Cartella>>();
        var candidatiPremio = new Dictionary<TipoPremio, List<DettaglioPremio>>();

        foreach (var giocatore in _giocatori)
        {
            for (var indiceCartella = 0; indiceCartella < giocatore.Cartelle.Count; indiceCartella++)
            {
                var cartella = giocatore.Cartelle[indiceCartella];
                if (!cartella.SegnaNumero(numero))
                {
                    continue;
                }

                if (cartella.EVincitrice)
                {
                    if (!vincitori.TryGetValue(giocatore, out var cartelleVincenti))
                    {
                        cartelleVincenti = [];
                        vincitori[giocatore] = cartelleVincenti;
                    }

                    cartelleVincenti.Add(cartella);
                }

                if (!cartella.TryTrovaRigaNumero(numero, out var rigaNumeroEstratto))
                {
                    continue;
                }

                var tipoPremio = TipoPremioDaConteggio(cartella.ConteggioSegnatiInRiga(rigaNumeroEstratto));
                if (tipoPremio is null || _premiAssegnati.Contains(tipoPremio.Value))
                {
                    continue;
                }

                var dettagli = candidatiPremio.GetValueOrDefault(tipoPremio.Value);
                if (dettagli is null)
                {
                    dettagli = [];
                    candidatiPremio[tipoPremio.Value] = dettagli;
                }

                dettagli.Add(new DettaglioPremio(
                    giocatore.Nome,
                    indiceCartella + 1,
                    rigaNumeroEstratto + 1,
                    cartella.GetNumeriRiga(rigaNumeroEstratto).ToList()));
            }
        }

        var nuoviPremi = AssegnaPremiDaCandidati(_tabellone.TotaleEstratti, candidatiPremio);
        return (vincitori, nuoviPremi);
    }

    private List<PremioAssegnato> AssegnaPremiDaCandidati(
        int numeroEstrazioni,
        Dictionary<TipoPremio, List<DettaglioPremio>> candidatiPremio)
    {
        var nuoviPremi = new List<PremioAssegnato>();

        foreach (var tipoPremio in OrdinePremi)
        {
            if (_premiAssegnati.Contains(tipoPremio))
            {
                continue;
            }

            if (!candidatiPremio.TryGetValue(tipoPremio, out var candidati) || candidati.Count == 0)
            {
                continue;
            }

            var vincitori = candidati
                .OrderBy(candidato => candidato.NomeGiocatore, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidato => candidato.NumeroCartella)
                .ThenBy(candidato => candidato.NumeroRiga)
                .ToList();

            var premioAssegnato = new PremioAssegnato(tipoPremio, numeroEstrazioni, vincitori);
            _premiAssegnati.Add(tipoPremio);
            _storicoPremi.Add(premioAssegnato);
            nuoviPremi.Add(premioAssegnato);
        }

        return nuoviPremi;
    }

    private static TipoPremio? TipoPremioDaConteggio(int conteggioSegnatiInRiga)
    {
        return conteggioSegnatiInRiga switch
        {
            2 => TipoPremio.Ambo,
            3 => TipoPremio.Terna,
            4 => TipoPremio.Quaterna,
            5 => TipoPremio.Cinquina,
            _ => null
        };
    }

    private bool QualcunoInTensione()
    {
        foreach (var giocatore in _giocatori)
        {
            var mancanti = giocatore.CartellaPiuVicina().Mancanti;
            if (mancanti is > 0 and <= SogliaTensione)
            {
                return true;
            }
        }

        return false;
    }

    private void ControllaTensioneEClassifica()
    {
        var classifica = CreaClassificaCorrente();
        var inTensione = classifica
            .Where(voce => voce.Mancanti is > 0 and <= SogliaTensione)
            .ToList();

        if (inTensione.Count == 0)
        {
            return;
        }

        var chiave = string.Join(
            "|",
            inTensione.Select(voce => $"{voce.Nome.ToLowerInvariant()}:{voce.Mancanti}"));

        if (!_chiaviTensioneGiaMostrate.Add(chiave))
        {
            return;
        }

        _renderer.StampaTensione(inTensione);
        _renderer.StampaClassifica(classifica);
    }

    private List<(string Nome, int Mancanti)> CreaClassificaCorrente()
    {
        return _giocatori
            .Select(giocatore =>
            {
                var migliore = giocatore.CartellaPiuVicina();
                return (Nome: giocatore.Nome, Mancanti: migliore.Mancanti);
            })
            .OrderBy(voce => voce.Mancanti)
            .ThenBy(voce => voce.Nome, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void ConcludiPartita(Dictionary<Giocatore, List<Cartella>> vincitori)
    {
        _renderer.PulisciSchermo();
        _renderer.StampaVincitori(vincitori, _tabellone);

        var statistiche = _giocatori
            .Select(giocatore =>
            {
                var migliore = giocatore.CartellaPiuVicina();
                return (
                    Nome: giocatore.Nome,
                    Mancanti: migliore.Mancanti,
                    CartelleVincenti: giocatore.CartelleVincentiTotali);
            })
            .OrderBy(voce => voce.Mancanti)
            .ThenByDescending(voce => voce.CartelleVincenti)
            .ThenBy(voce => voce.Nome, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _renderer.StampaStatisticheFinali(statistiche);

        var percorsoReport = _gestoreReport.SalvaReport(_tabellone, vincitori, _giocatori, _storicoPremi);
        _renderer.StampaMessaggio($"Report salvato in: {percorsoReport}");

        _renderer.AttendiInvio("Premi Invio per terminare la partita...");
    }

    private void MostraCartellePulite()
    {
        _renderer.PulisciSchermo();
        _renderer.StampaStatoGiocatori(_giocatori, _tabellone);
        _renderer.AttendiInvio();
        _renderer.PulisciSchermo();
    }

    private void MostraTabellonePulito()
    {
        _renderer.PulisciSchermo();
        _renderer.StampaTabellone(_tabellone);
        _renderer.AttendiInvio();
        _renderer.PulisciSchermo();
    }

    private void MostraStatoSintetico()
    {
        _renderer.PulisciSchermo();

        var classifica = CreaClassificaCorrente();
        var inTensione = classifica
            .Where(voce => voce.Mancanti is > 0 and <= SogliaTensione)
            .ToList();

        if (inTensione.Count > 0)
        {
            _renderer.StampaTensione(inTensione);
        }

        _renderer.StampaClassifica(classifica.Take(3).ToList());
        _renderer.AttendiInvio();
        _renderer.PulisciSchermo();
    }
}
