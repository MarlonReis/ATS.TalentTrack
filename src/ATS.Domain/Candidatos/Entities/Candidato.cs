namespace ATS.Domain.Candidatos.Entities;

using ATS.Domain.Candidatos.Events;
using ATS.Domain.Candidatos.ValueObjects;
using ATS.Domain.Shared;

public sealed class Candidato : AggregateRoot
{
    public string Nome { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public Telefone Telefone { get; private set; } = default!;
    public Curriculo? Curriculo { get; private set; }
    public DateTime DataCadastro { get; private set; }


    private Candidato() { }

    public static Candidato Criar(string nome, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Nome do candidato é obrigatório.");
        }

        if (nome.Length > 150)
        {
            throw new DomainException("Nome não pode exceder 150 caracteres.");
        }

        var candidato = new Candidato
        {
            Nome = nome.Trim(),
            Email = Email.Create(email),
            Telefone = Telefone.Create(telefone),
            DataCadastro = DateTime.UtcNow
        };

        candidato.AddDomainEvent(new CandidatoCriadoEvent(candidato.Id, candidato.Nome));

        return candidato;
    }

    public void AtualizarContato(string nome, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Nome é obrigatório.");
        }

        Nome = nome.Trim();
        Email = Email.Create(email);
        Telefone = Telefone.Create(telefone);
    }

    public void AdicionarCurriculo(string nomeArquivo, string contentType, string urlOuBase64)
    {
        Curriculo = Curriculo.Create(nomeArquivo, contentType, urlOuBase64);
        AddDomainEvent(new CurriculoAdicionadoEvent(Id, nomeArquivo));
    }
}
