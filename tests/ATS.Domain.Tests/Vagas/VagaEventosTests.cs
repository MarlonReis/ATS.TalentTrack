namespace ATS.Domain.Tests.Vagas;

using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Events;
using Xunit;

public class VagaEventosTests
{
    [Fact]
    public void FecharDeveAdicionarVagaFechadaEventComDadosCorretos()
    {
        var vaga = Vaga.Criar("Dev Back-end", "Descrição", "Requisitos", 12000);
        vaga.ClearDomainEvents();

        vaga.Fechar();

        var evento = Assert.IsType<VagaFechadaEvent>(vaga.DomainEvents.Single());
        Assert.Equal(vaga.Id, evento.VagaId);
        Assert.Equal("Dev Back-end", evento.Titulo);
        Assert.True(evento.OcorridoEm >= DateTime.UtcNow.AddSeconds(-1));
        Assert.True(evento.OcorridoEm <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void ReabrirEFecharNovamenteDeveAdicionarSegundoVagaFechadaEvent()
    {
        var vaga = Vaga.Criar("Tech Lead", "Descrição", "Requisitos", 18000);

        vaga.Fechar();
        vaga.Reabrir();
        vaga.Fechar();

        var vagaFechadaEvents = vaga.DomainEvents.OfType<VagaFechadaEvent>().ToList();
        Assert.Equal(2, vagaFechadaEvents.Count);
        Assert.Contains(vaga.DomainEvents, e => e is VagaPublicadaEvent);
    }
}
