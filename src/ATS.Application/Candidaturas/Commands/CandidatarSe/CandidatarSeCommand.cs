namespace ATS.Application.Candidaturas.Commands.CandidatarSe;

public record CandidatarSeCommand(
    Guid CandidatoId,
    Guid VagaId
);
