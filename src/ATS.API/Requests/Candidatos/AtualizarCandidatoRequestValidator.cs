namespace ATS.API.Requests.Candidatos;

using FluentValidation;

public sealed class AtualizarCandidatoRequestValidator : AbstractValidator<AtualizarCandidatoRequest>
{
    public AtualizarCandidatoRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode exceder 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(254).WithMessage("E-mail não pode exceder 254 caracteres.");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .Matches(@"^\+?[\d\s\-\(\)]{8,20}$").WithMessage("Telefone inválido.")
            .MaximumLength(20).WithMessage("Telefone não pode exceder 20 caracteres.");
    }
}
