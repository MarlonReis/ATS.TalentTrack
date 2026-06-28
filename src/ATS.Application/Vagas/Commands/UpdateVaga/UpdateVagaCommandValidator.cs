namespace ATS.Application.Vagas.Commands.UpdateVaga;

using FluentValidation;

public sealed class UpdateVagaCommandValidator : AbstractValidator<UpdateVagaCommand>
{
    public UpdateVagaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id da vaga é obrigatório.");

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
    }
}
