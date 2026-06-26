namespace ATS.Infrastructure.Persistence.Mappings;

using ATS.Domain.Candidaturas.Entities;
using ATS.Domain.Candidaturas.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public static class CandidaturaMap
{
    public static void Register()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Candidatura)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Candidatura>(map =>
        {
            map.SetIgnoreExtraElements(true);
            map.MapMember(candidatura => candidatura.CandidatoId).SetElementName("candidatoId");
            map.MapMember(candidatura => candidatura.VagaId).SetElementName("vagaId");
            map.MapMember(candidatura => candidatura.Status)
                .SetElementName("status")
                .SetSerializer(new EnumSerializer<StatusCandidatura>(BsonType.String));
            map.MapMember(candidatura => candidatura.DataCandidatura).SetElementName("dataCandidatura");
            map.MapMember(candidatura => candidatura.Observacoes)
                .SetElementName("observacoes")
                .SetIgnoreIfNull(true);
        });
    }
}
