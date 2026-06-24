namespace ATS.Application.Candidatos.Commands.UpdateCandidato;

public record UpdateCandidatoCommand(
    Guid Id,
    string Nome,
    string Email,
    string Telefone
);
