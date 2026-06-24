using ATS.Domain.Shared;
using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Events;
using Xunit;

namespace ATS.Domain.Tests.Vagas;

public class VagaTests
{
    private const string _tituloValido = "Desenvolvedor Back-end Sênior";
    private const string _descricaoValida = "Vaga para desenvolvedor .NET com experiência em DDD.";
    private const string _requisitosValidos = "5+ anos de experiência, .NET 8+, MongoDB";
    private const decimal _salarioValido = 12000m;

    [Theory]
    [InlineData("Dev Back-end", "Descrição da vaga", "Requisitos da vaga", 8000)]
    [InlineData("Analista QA", "Vaga para analista", "Selenium, Cypress", 6000)]
    [InlineData("Tech Lead", "Liderar equipe técnica", "10+ anos exp", 18000)]
    public void DeveCriarVagaComTodasAsPropriedadesCorretamenteDefinidas(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var antes = DateTime.UtcNow;


        var vaga = Vaga.Criar(titulo, descricao, requisitos, salario);


        Assert.NotEqual(Guid.Empty, vaga.Id);
        Assert.Equal(titulo.Trim(), vaga.Titulo);
        Assert.Equal(descricao.Trim(), vaga.Descricao);
        Assert.Equal(requisitos.Trim(), vaga.Requisitos);
        Assert.Equal(salario, vaga.Salario.Valor);
        Assert.Equal(StatusVaga.Aberta, vaga.Status);
        Assert.Null(vaga.DataEncerramento);
        Assert.True(vaga.DataAbertura >= antes);
        Assert.True(vaga.DataAbertura <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("  Dev Back-end  ", "Dev Back-end")]
    [InlineData("   Analista QA", "Analista QA")]
    [InlineData("Tech Lead  ", "Tech Lead")]
    public void DeveTrimmarTituloAoCriarVaga(string tituloComEspacos, string tituloEsperado)
    {

        var vaga = Vaga.Criar(tituloComEspacos, _descricaoValida, _requisitosValidos, _salarioValido);

        Assert.Equal(tituloEsperado, vaga.Titulo);
    }

    [Theory]
    [InlineData("  Descrição com espaços  ", "Descrição com espaços")]
    [InlineData("   Outra descrição", "Outra descrição")]
    public void DeveTrimmarDescricaoAoCriarVaga(string descricaoComEspacos, string descricaoEsperada)
    {

        var vaga = Vaga.Criar(_tituloValido, descricaoComEspacos, _requisitosValidos, _salarioValido);


        Assert.Equal(descricaoEsperada, vaga.Descricao);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DeveDefinirRequisitosComoStringVaziaQuandoNuloOuVazio(string? requisitos)
    {

        var vaga = Vaga.Criar(_tituloValido, _descricaoValida, requisitos!, _salarioValido);


        Assert.Equal(string.Empty, vaga.Requisitos);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    [InlineData("Tech Lead")]
    public void DeveGerarIdUnicoParaCadaVagaCriada(string titulo)
    {

        var vagaA = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        var vagaB = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);


        Assert.NotEqual(Guid.Empty, vagaA.Id);
        Assert.NotEqual(Guid.Empty, vagaB.Id);
        Assert.NotEqual(vagaA.Id, vagaB.Id);
    }





    [Theory]
    [InlineData("Dev Back-end Sênior", "Descrição", "Requisitos", 10000)]
    [InlineData("  Tech Lead  ", "Descrição", "Requisitos", 8000)]
    public void DeveDispararExatamenteUmEventoVagaPublicadaAoCriar(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var vaga = Vaga.Criar(titulo, descricao, requisitos, salario);


        var evento = Assert.Single(vaga.DomainEvents);
        Assert.IsType<VagaPublicadaEvent>(evento);
    }

    [Theory]
    [InlineData("Dev Back-end Sênior", "Descrição", "Requisitos", 10000)]
    [InlineData("  Tech Lead  ", "Descrição", "Requisitos", 8000)]
    public void DeveEventoVagaPublicadaConterIdETituloTrimadoDaVaga(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var vaga = Vaga.Criar(titulo, descricao, requisitos, salario);
        var evento = vaga.DomainEvents.OfType<VagaPublicadaEvent>().Single();


        Assert.Equal(vaga.Id, evento.VagaId);
        Assert.Equal(vaga.Titulo, evento.Titulo);
    }





    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecaoQuandoTituloForNuloOuVazio(string? titulo)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Vaga.Criar(titulo!, _descricaoValida, _requisitosValidos, _salarioValido));


        Assert.Equal("Título da vaga é obrigatório.", excecao.Message);
    }

    [Theory]
    [InlineData(199)]
    [InlineData(200)]
    public void DeveAceitarTituloComAte200Caracteres(int tamanho)
    {

        var titulo = new string('A', tamanho);


        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);


        Assert.Equal(tamanho, vaga.Titulo.Length);
    }

    [Theory]
    [InlineData(201)]
    [InlineData(300)]
    public void DeveLancarExcecaoQuandoTituloExceder200Caracteres(int tamanho)
    {

        var titulo = new string('A', tamanho);


        var excecao = Assert.Throws<DomainException>(
            () => Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido));


        Assert.Equal("Título não pode exceder 200 caracteres.", excecao.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecaoQuandoDescricaoForNulaOuVazia(string? descricao)
    {

        var excecao = Assert.Throws<DomainException>(
            () => Vaga.Criar(_tituloValido, descricao!, _requisitosValidos, _salarioValido));


        Assert.Equal("Descrição da vaga é obrigatória.", excecao.Message);
    }





    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Novos requisitos", 15000)]
    [InlineData("Outro Título", "Outra descrição", "Outros requisitos", 9000)]
    public void DeveAtualizarTodasAsPropriedadesCorretamente(
        string novoTitulo, string novaDescricao, string novosRequisitos, decimal novoSalario)
    {

        var vaga = Vaga.Criar(_tituloValido, _descricaoValida, _requisitosValidos, _salarioValido);


        vaga.Atualizar(novoTitulo, novaDescricao, novosRequisitos, novoSalario);


        Assert.Equal(novoTitulo.Trim(), vaga.Titulo);
        Assert.Equal(novaDescricao.Trim(), vaga.Descricao);
        Assert.Equal(novosRequisitos.Trim(), vaga.Requisitos);
        Assert.Equal(novoSalario, vaga.Salario.Valor);
    }

    [Theory]
    [InlineData("  Título atualizado  ", "Título atualizado")]
    [InlineData("   Analista Pleno", "Analista Pleno")]
    public void DeveTrimmarTituloAoAtualizar(string tituloComEspacos, string tituloEsperado)
    {

        var vaga = Vaga.Criar(_tituloValido, _descricaoValida, _requisitosValidos, _salarioValido);


        vaga.Atualizar(tituloComEspacos, _descricaoValida, _requisitosValidos, _salarioValido);


        Assert.Equal(tituloEsperado, vaga.Titulo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DeveDefinirRequisitosComoStringVaziaAoAtualizarComNuloOuVazio(
        string? requisitos)
    {

        var vaga = Vaga.Criar(_tituloValido, _descricaoValida, _requisitosValidos, _salarioValido);


        vaga.Atualizar(_tituloValido, _descricaoValida, requisitos!, _salarioValido);


        Assert.Equal(string.Empty, vaga.Requisitos);
    }

    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Novos requisitos", 15000)]
    public void DeveNaoAdicionarNovosEventosAoAtualizar(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var vaga = Vaga.Criar(_tituloValido, _descricaoValida, _requisitosValidos, _salarioValido);
        var quantidadeAntes = vaga.DomainEvents.Count;


        vaga.Atualizar(titulo, descricao, requisitos, salario);


        Assert.Equal(quantidadeAntes, vaga.DomainEvents.Count);
    }


    [Theory]
    [InlineData("Novo Título", "Nova descrição", "Requisitos", 10000)]
    public void DeveLancarExcecaoAoAtualizarVagaFechada(
        string titulo, string descricao, string requisitos, decimal salario)
    {

        var vaga = Vaga.Criar(_tituloValido, _descricaoValida, _requisitosValidos, _salarioValido);
        vaga.Fechar();

        var excecao = Assert.Throws<DomainException>(
            () => vaga.Atualizar(titulo, descricao, requisitos, salario));


        Assert.Equal("Não é possível editar uma vaga fechada.", excecao.Message);
    }


    [Theory]
    [InlineData("Dev Back-end")]
    [InlineData("Tech Lead")]
    public void DeveFecharVagaDefinindoStatusEDataEncerramento(string titulo)
    {

        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        var antes = DateTime.UtcNow;


        vaga.Fechar();


        Assert.Equal(StatusVaga.Fechada, vaga.Status);
        Assert.NotNull(vaga.DataEncerramento);
        Assert.Equal(DateTimeKind.Utc, vaga.DataEncerramento!.Value.Kind);
        Assert.True(vaga.DataEncerramento >= antes);
        Assert.True(vaga.DataEncerramento <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    public void DeveLancarExcecaoAoFecharVagaJaFechada(string titulo)
    {

        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        vaga.Fechar();


        var excecao = Assert.Throws<DomainException>(() => vaga.Fechar());


        Assert.Equal("Vaga já está fechada.", excecao.Message);
    }





    [Theory]
    [InlineData("Dev Back-end")]
    [InlineData("Tech Lead")]
    public void DeveReabrirVagaDefinindoStatusAbertaELimpandoDataEncerramento(string titulo)
    {

        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        vaga.Fechar();
        Assert.NotNull(vaga.DataEncerramento);


        vaga.Reabrir();


        Assert.Equal(StatusVaga.Aberta, vaga.Status);
        Assert.Null(vaga.DataEncerramento);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    public void DeveLancarExcecaoAoReabrirVagaJaAberta(string titulo)
    {

        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        Assert.Equal(StatusVaga.Aberta, vaga.Status);


        var excecao = Assert.Throws<DomainException>(() => vaga.Reabrir());


        Assert.Equal("Vaga já está aberta.", excecao.Message);
    }





    [Theory]
    [InlineData("Dev Back-end", "Título atualizado", "Nova descrição", "Novos req", 14000)]
    public void DevePermitirAtualizacaoAposReabrirVaga(
        string titulo, string novoTitulo, string novaDescricao,
        string novosRequisitos, decimal novoSalario)
    {

        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);
        vaga.Fechar();
        vaga.Reabrir();


        vaga.Atualizar(novoTitulo, novaDescricao, novosRequisitos, novoSalario);


        Assert.Equal(novoTitulo.Trim(), vaga.Titulo);
        Assert.Equal(StatusVaga.Aberta, vaga.Status);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    public void DeveResetarDataEncerramentoAoCadaNovoFechamento(string titulo)
    {

        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);


        vaga.Fechar();
        var primeiraDataEncerramento = vaga.DataEncerramento;


        vaga.Reabrir();
        vaga.Fechar();


        Assert.NotNull(vaga.DataEncerramento);
        Assert.True(vaga.DataEncerramento >= primeiraDataEncerramento);
    }

    [Theory]
    [InlineData("Dev Back-end")]
    public void DeveManterStatusAbertaEDataEncerramentoNulaAposMultiplasReaberturas(
        string titulo)
    {

        var vaga = Vaga.Criar(titulo, _descricaoValida, _requisitosValidos, _salarioValido);

        vaga.Fechar();
        vaga.Reabrir();
        vaga.Fechar();
        vaga.Reabrir();

        Assert.Equal(StatusVaga.Aberta, vaga.Status);
        Assert.Null(vaga.DataEncerramento);
    }
}
