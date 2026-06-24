namespace ATS.Application.Candidaturas.Commands.ReprovarCandidatura;

public record ReprovarCandidaturaCommand(
    Guid CandidaturaId,
    string? Observacoes = null
);
