namespace ATS.Application.Candidaturas.Commands.CandidatarSe;

using FluentValidation;

public sealed class CandidatarSeCommandValidator : AbstractValidator<CandidatarSeCommand>
{
    public CandidatarSeCommandValidator()
    {
        RuleFor(x => x.CandidatoId)
            .NotEmpty().WithMessage("CandidatoId é obrigatório.");

        RuleFor(x => x.VagaId)
            .NotEmpty().WithMessage("VagaId é obrigatório.");
    }
}
