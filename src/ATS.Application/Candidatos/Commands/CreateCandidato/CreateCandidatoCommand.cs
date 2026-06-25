namespace ATS.Application.Candidatos.Commands.CreateCandidato;

public record CreateCandidatoCommand(
    string Nome,
    string Email,
    string Telefone
);
