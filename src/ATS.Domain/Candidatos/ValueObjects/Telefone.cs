namespace ATS.Domain.Candidatos.ValueObjects;

using ATS.Domain.Shared;
using System.Text.RegularExpressions;

public sealed class Telefone : ValueObject
{
    public string Value { get; }

    private Telefone(string numero) => Value = numero;

    public static Telefone Create(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("Telefone não pode ser vazio.");

        var apenasDigitos = Regex.Replace(numero, @"\D", "");

        if (apenasDigitos.Length is < 10 or > 11)
            throw new DomainException("Telefone deve ter 10 ou 11 dígitos.");

        return new Telefone(apenasDigitos);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}