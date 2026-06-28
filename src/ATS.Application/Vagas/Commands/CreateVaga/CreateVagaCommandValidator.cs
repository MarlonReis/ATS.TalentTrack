namespace ATS.Application.Vagas.Commands.CreateVaga;

using FluentValidation;

public sealed class CreateVagaCommandValidator : AbstractValidator<CreateVagaCommand>
{
    private static readonly string[] _moedasPermitidas = ["BRL", "USD", "EUR"];

    public CreateVagaCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título não pode exceder 200 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(5000).WithMessage("Descrição não pode exceder 5000 caracteres.");

        RuleFor(x => x.Requisitos)
            .MaximumLength(5000).WithMessage("Requisitos não podem exceder 5000 caracteres.")
            .When(x => x.Requisitos is not null);

        RuleFor(x => x.Salario)
            .GreaterThanOrEqualTo(0).WithMessage("Salário não pode ser negativo.");

        RuleFor(x => x.Moeda)
            .NotEmpty().WithMessage("Moeda é obrigatória.")
            .Must(m => _moedasPermitidas.Contains(m.ToUpperInvariant()))
            .WithMessage($"Moeda deve ser uma de: {string.Join(", ", _moedasPermitidas)}.");
    }
}
