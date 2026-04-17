using Tombola.Models;
using Tombola.Services;

namespace Tombola.Tests;

public class ModelliInvariantiTests
{
    [Fact]
    public void Tabellone_AggiuntaDuplicataNonIncrementaEstratti()
    {
        var tabellone = new Tabellone();

        Assert.True(tabellone.AggiungiNumeroEstratto(10));
        Assert.False(tabellone.AggiungiNumeroEstratto(10));
        Assert.Equal(1, tabellone.TotaleEstratti);
        Assert.Equal(10, tabellone.UltimoEstratto);
        Assert.True(tabellone.Contiene(10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(91)]
    public void Tabellone_NumeroFuoriRangeLanciaEccezione(int numero)
    {
        var tabellone = new Tabellone();

        Assert.Throws<ArgumentOutOfRangeException>(() => tabellone.AggiungiNumeroEstratto(numero));
    }

    [Fact]
    public void Cartella_CostruttoreConDimensioniErrateLanciaEccezione()
    {
        var grigliaErrata = new int?[2, 9];

        Assert.Throws<ArgumentException>(() => new Cartella(grigliaErrata));
    }

    [Fact]
    public void Cartella_SegnandoTuttiINumeriDiventaVincitrice()
    {
        var cartella = new GeneratoreCartelle().CreaCartellaTradizionale();

        foreach (var numero in cartella.Numeri)
        {
            Assert.True(cartella.SegnaNumero(numero));
        }

        Assert.True(cartella.EVincitrice);
        Assert.Equal(15, cartella.NumeriSegnati);
        Assert.Equal(0, cartella.NumeriMancanti);
        Assert.Empty(cartella.GetNumeriRimanenti());
    }
}
