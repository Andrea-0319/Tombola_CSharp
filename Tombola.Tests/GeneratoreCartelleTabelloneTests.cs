using Tombola.Models;
using Tombola.Services;

namespace Tombola.Tests;

public class GeneratoreCartelleTabelloneTests
{
    [Fact]
    public void CreaCartelleTabelloneCompleto_GeneraSeiCartelleCanonicheConCoperturaCompleta()
    {
        var generatore = new GeneratoreCartelle();

        var cartelle = generatore.CreaCartelleTabelloneCompleto();

        Assert.Equal(6, cartelle.Count);

        var copertura = new HashSet<int>();

        for (var indiceCartella = 0; indiceCartella < cartelle.Count; indiceCartella++)
        {
            var cartella = cartelle[indiceCartella];
            Assert.Equal(15, cartella.Numeri.Count);

            for (var riga = 0; riga < MappaCartelleTabellone.RighePerCartella; riga++)
            {
                for (var colonna = 0; colonna < MappaCartelleTabellone.ColonnePienePerCartella; colonna++)
                {
                    var atteso = MappaCartelleTabellone.NumeroDaPosizione(indiceCartella, riga, colonna);
                    Assert.Equal(atteso, cartella.GetCella(riga, colonna));
                }

                for (var colonna = MappaCartelleTabellone.ColonnePienePerCartella; colonna < 9; colonna++)
                {
                    Assert.Null(cartella.GetCella(riga, colonna));
                }
            }

            foreach (var numero in cartella.Numeri)
            {
                Assert.True(copertura.Add(numero));
            }
        }

        Assert.Equal(90, copertura.Count);
        Assert.Contains(1, copertura);
        Assert.Contains(90, copertura);
    }

    [Fact]
    public void CreaCartelleTabelloneCompleto_E_Deterministico()
    {
        var generatore = new GeneratoreCartelle();

        var prima = generatore.CreaCartelleTabelloneCompleto();
        var seconda = generatore.CreaCartelleTabelloneCompleto();

        Assert.Equal(prima.Count, seconda.Count);

        for (var indiceCartella = 0; indiceCartella < prima.Count; indiceCartella++)
        {
            for (var riga = 0; riga < 3; riga++)
            {
                for (var colonna = 0; colonna < 9; colonna++)
                {
                    Assert.Equal(
                        prima[indiceCartella].GetCella(riga, colonna),
                        seconda[indiceCartella].GetCella(riga, colonna));
                }
            }
        }
    }
}
