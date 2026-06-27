namespace ATS.Domain.Vagas.Entities;

using ATS.Domain.Shared;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.Events;
using ATS.Domain.Vagas.ValueObjects;

public sealed class Vaga : AggregateRoot
{
    public string Titulo { get; private set; } = default!;
    public string Descricao { get; private set; } = default!;
    public string Requisitos { get; private set; } = default!;
    public Salario Salario { get; private set; } = default!;
    public StatusVaga Status { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataEncerramento { get; private set; }

    private Vaga() { }

    public static Vaga Criar(string titulo, string descricao, string requisitos, decimal salario)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new DomainException("Título da vaga é obrigatório.");
        }

        if (titulo.Length > 200)
        {
            throw new DomainException("Título não pode exceder 200 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new DomainException("Descrição da vaga é obrigatória.");
        }

        var vaga = new Vaga
        {
            Titulo = titulo.Trim(),
            Descricao = descricao.Trim(),
            Requisitos = requisitos?.Trim() ?? string.Empty,
            Salario = Salario.Create(salario),
            Status = StatusVaga.Aberta,
            DataAbertura = DateTime.UtcNow
        };

        vaga.AddDomainEvent(new VagaPublicadaEvent(vaga.Id, vaga.Titulo));
        return vaga;
    }

    public void Atualizar(string titulo, string descricao, string requisitos, decimal salario)
    {
        if (Status == StatusVaga.Fechada)
        {
            throw new DomainException("Não é possível editar uma vaga fechada.");
        }

        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        Requisitos = requisitos?.Trim() ?? string.Empty;
        Salario = Salario.Create(salario);
    }

    public void Fechar()
    {
        if (Status == StatusVaga.Fechada)
        {
            throw new DomainException("Vaga já está fechada.");
        }

        Status = StatusVaga.Fechada;
        DataEncerramento = DateTime.UtcNow;
        AddDomainEvent(new VagaFechadaEvent(Id, Titulo));
    }

    public void Reabrir()
    {
        if (Status == StatusVaga.Aberta)
        {
            throw new DomainException("Vaga já está aberta.");
        }

        Status = StatusVaga.Aberta;
        DataEncerramento = null;
    }
}
