using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.Events;
using ATS.Domain.Shared;
using Xunit;

namespace ATS.Domain.Tests.Candidatos;

public class CandidatoTests
{
    private const string _nomeValido = "João da Silva";
    private const string _emailValido = "joao@email.com";
    private const string _telefoneValido = "11912345678";

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Maria Santos", "maria@empresa.com.br", "21987654321")]
    [InlineData("Ana Lima", "ana.lima@corp.io", "1134567890")]
    public void DeveCriarCandidatoComTodasAsPropriedadesCorretamenteDefinidas(
        string nome, string email, string telefone)
    {
        var antes = DateTime.UtcNow;

        var candidato = Candidato.Criar(nome, email, telefone);

        Assert.NotEqual(Guid.Empty, candidato.Id);
        Assert.Equal(nome.Trim(), candidato.Nome);
        Assert.Equal(email, candidato.Email.Value);
        Assert.Equal(telefone, candidato.Telefone.Value);
        Assert.Null(candidato.Curriculo);
        Assert.True(candidato.DataCadastro >= antes);
        Assert.True(candidato.DataCadastro <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("  João Silva  ", "João Silva")]
    [InlineData("   Ana Lima", "Ana Lima")]
    [InlineData("Pedro Souza  ", "Pedro Souza")]
    public void DeveTrimmarNomeAoCriarCandidato(string nomeComEspacos, string nomeEsperado)
    {
        var candidato = Candidato.Criar(nomeComEspacos, _emailValido, _telefoneValido);

        Assert.Equal(nomeEsperado, candidato.Nome);
    }

    [Theory]
    [InlineData("João Silva")]
    [InlineData("Maria Santos")]
    public void DeveGerarIdUnicoParaCadaCandidatoCriado(string nome)
    {
        var a = Candidato.Criar(nome, _emailValido, _telefoneValido);
        var b = Candidato.Criar(nome, "outro@email.com", "21911111111");

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(Guid.Empty, b.Id);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Theory]
    [InlineData("João da Silva", "joao@email.com", "11912345678")]
    [InlineData("Maria Santos", "maria@corp.com", "21987654321")]
    public void DeveDispararExatamenteUmEventoCandidatoCriadoAoCriar(
        string nome, string email, string telefone)
    {
        var candidato = Candidato.Criar(nome, email, telefone);

        var evento = Assert.Single(candidato.DomainEvents);
        Assert.IsType<CandidatoCriadoEvent>(evento);
    }

    [Theory]
    [InlineData("  João Silva  ", "João Silva")]
    [InlineData("Maria Santos", "Maria Santos")]
    public void DeveEventoCandidatoCriadoConterNomeTrimadoEIdCorretos(
        string nomeEntrada, string nomeEsperado)
    {
        var candidato = Candidato.Criar(nomeEntrada, _emailValido, _telefoneValido);

        var evento = candidato.DomainEvents.OfType<CandidatoCriadoEvent>().Single();
        Assert.Equal(candidato.Id, evento.CandidatoId);
        Assert.Equal(nomeEsperado, evento.Nome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecaoQuandoNomeForNuloOuVazioAoCriar(string? nome)
    {
        var excecao = Assert.Throws<DomainException>(
            () => Candidato.Criar(nome!, _emailValido, _telefoneValido));

        Assert.Equal("Nome do candidato é obrigatório.", excecao.Message);
    }

    [Theory]
    [InlineData(149)]
    [InlineData(150)]
    public void DeveAceitarNomeComAte150Caracteres(int tamanho)
    {
        var nome = new string('A', tamanho);

        var candidato = Candidato.Criar(nome, _emailValido, _telefoneValido);

        Assert.Equal(tamanho, candidato.Nome.Length);
    }

    [Theory]
    [InlineData(151)]
    [InlineData(200)]
    public void DeveLancarExcecaoQuandoNomeExceder150Caracteres(int tamanho)
    {
        var nome = new string('A', tamanho);

        var excecao = Assert.Throws<DomainException>(
            () => Candidato.Criar(nome, _emailValido, _telefoneValido));

        Assert.Equal("Nome não pode exceder 150 caracteres.", excecao.Message);
    }

    [Theory]
    [InlineData("Novo Nome", "novo@email.com", "21987654321")]
    [InlineData("Carlos Pereira", "carlos@corp.io", "1134567890")]
    public void DeveAtualizarNomeEmailETelefoneCorretamente(
        string novoNome, string novoEmail, string novoTelefone)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);

        candidato.AtualizarContato(novoNome, novoEmail, novoTelefone);

        Assert.Equal(novoNome.Trim(), candidato.Nome);
        Assert.Equal(novoEmail, candidato.Email.Value);
        Assert.Equal(novoTelefone, candidato.Telefone.Value);
    }

    [Theory]
    [InlineData("  Carlos Pereira  ", "Carlos Pereira")]
    [InlineData("Ana  ", "Ana")]
    public void DeveTrimmarNomeAoAtualizarContato(string nomeComEspacos, string nomeEsperado)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);

        candidato.AtualizarContato(nomeComEspacos, _emailValido, _telefoneValido);

        Assert.Equal(nomeEsperado, candidato.Nome);
    }

    [Theory]
    [InlineData("Novo Nome", "novo@email.com", "21987654321")]
    public void DeveNaoAdicionarNovosEventosAoAtualizarContato(
        string novoNome, string novoEmail, string novoTelefone)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        var quantidadeAntes = candidato.DomainEvents.Count;

        candidato.AtualizarContato(novoNome, novoEmail, novoTelefone);

        Assert.Equal(quantidadeAntes, candidato.DomainEvents.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveLancarExcecaoAoAtualizarContatoComNomeInvalido(string? nome)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);

        var excecao = Assert.Throws<DomainException>(
            () => candidato.AtualizarContato(nome!, _emailValido, _telefoneValido));

        Assert.Equal("Nome é obrigatório.", excecao.Message);
    }

    [Theory]
    [InlineData("curriculo.pdf", "application/pdf", "base64==")]
    [InlineData("portfolio.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "https://storage/cv")]
    public void DeveAdicionarCurriculoAoCandidatoCorretamente(
        string nomeArquivo, string contentType, string urlOuBase64)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        Assert.Null(candidato.Curriculo);

        candidato.AdicionarCurriculo(nomeArquivo, contentType, urlOuBase64);

        Assert.NotNull(candidato.Curriculo);
        Assert.Equal(nomeArquivo, candidato.Curriculo.NomeArquivo);
        Assert.Equal(contentType, candidato.Curriculo.ContentType);
        Assert.Equal(urlOuBase64, candidato.Curriculo.UrlOuBase64);
    }

    [Theory]
    [InlineData("curriculo_v1.pdf", "curriculo_v2.pdf")]
    [InlineData("cv_antigo.doc", "cv_novo.docx")]
    public void DeveSubstituirCurriculoExistenteAoAdicionarNovoCurriculo(
        string nomeAntigo, string nomeNovo)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.AdicionarCurriculo(nomeAntigo, "application/pdf", "conteudoAntigo");

        candidato.AdicionarCurriculo(nomeNovo, "application/pdf", "conteudoNovo");

        Assert.Equal(nomeNovo, candidato.Curriculo!.NomeArquivo);
    }

    [Theory]
    [InlineData("curriculo.pdf", "application/pdf", "base64==")]
    [InlineData("cv.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "https://storage/cv")]
    public void DeveDispararEventoCurriculoAdicionadoComDadosCorretos(
        string nomeArquivo, string contentType, string urlOuBase64)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.ClearDomainEvents();

        candidato.AdicionarCurriculo(nomeArquivo, contentType, urlOuBase64);

        var evento = Assert.Single(candidato.DomainEvents);
        var curriculoEvent = Assert.IsType<CurriculoAdicionadoEvent>(evento);
        Assert.Equal(candidato.Id, curriculoEvent.CandidatoId);
        Assert.Equal(nomeArquivo, curriculoEvent.NomeArquivo);
    }

    [Theory]
    [InlineData("curriculo.pdf", "application/pdf", "base64==")]
    public void DeveAcumularEventosNaOrdemCorretaDuranteOFluxoCompleto(
        string nomeArquivo, string contentType, string urlOuBase64)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.AdicionarCurriculo(nomeArquivo, contentType, urlOuBase64);

        Assert.Equal(2, candidato.DomainEvents.Count);
        Assert.IsType<CandidatoCriadoEvent>(candidato.DomainEvents.First());
        Assert.IsType<CurriculoAdicionadoEvent>(candidato.DomainEvents.Last());
    }

    [Theory]
    [InlineData("curriculo.pdf")]
    public void DeveLimparEventosAoChamarClearDomainEvents(string nomeArquivo)
    {
        var candidato = Candidato.Criar(_nomeValido, _emailValido, _telefoneValido);
        candidato.AdicionarCurriculo(nomeArquivo, "application/pdf", "data");
        Assert.Equal(2, candidato.DomainEvents.Count);

        candidato.ClearDomainEvents();

        Assert.Empty(candidato.DomainEvents);
    }
}
