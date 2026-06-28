namespace ATS.Application.Common.Validation;

using FluentValidation.Results;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("Um ou mais erros de validação ocorreram.")
    {
        Errors = errors.ToList();
    }
}
