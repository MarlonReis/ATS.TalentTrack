namespace ATS.Application.Candidaturas.Commands.AprovarCandidatura;

public record AprovarCandidaturaCommand(
    Guid CandidaturaId,
    string? Observacoes = null
);
