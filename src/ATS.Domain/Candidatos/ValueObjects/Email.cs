namespace ATS.Domain.Candidatos.ValueObjects;

using System.Text.RegularExpressions;
using ATS.Domain.Shared;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("E-mail não pode ser vazio.");
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new DomainException($"E-mail '{email}' possui formato inválido.");
        }

        return new Email(email.Trim().ToLower());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
