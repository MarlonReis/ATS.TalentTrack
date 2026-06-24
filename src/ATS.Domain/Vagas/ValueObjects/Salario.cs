namespace ATS.Domain.Vagas.ValueObjects;

using ATS.Domain.Shared;

public sealed class Salario : ValueObject
{
    public decimal Valor { get; }
    public string Moeda { get; }

    private Salario(decimal valor, string moeda)
    {
        Valor = valor;
        Moeda = moeda;
    }

    public static Salario Create(decimal valor, string moeda = "BRL")
    {
        if (valor < 0)
        {
            throw new DomainException("Salário não pode ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(moeda))
        {
            throw new DomainException("Moeda é obrigatória.");
        }

        return new Salario(valor, moeda.ToUpper());
    }

    public static Salario Indefinido() => new(0, "BRL");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
        yield return Moeda;
    }

    public override string ToString() => $"{Moeda} {Valor:N2}";
}
