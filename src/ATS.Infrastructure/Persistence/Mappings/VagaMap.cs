namespace ATS.Infrastructure.Persistence.Mappings;

using ATS.Domain.Vagas.Entities;
using ATS.Domain.Vagas.Enums;
using ATS.Domain.Vagas.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public static class VagaMap
{
    public static void Register()
    {
        RegisterSalario();

        if (BsonClassMap.IsClassMapRegistered(typeof(Vaga)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Vaga>(map =>
        {
            map.SetIgnoreExtraElements(true);
            map.MapMember(vaga => vaga.Titulo).SetElementName("titulo");
            map.MapMember(vaga => vaga.Descricao).SetElementName("descricao");
            map.MapMember(vaga => vaga.Requisitos).SetElementName("requisitos");
            map.MapMember(vaga => vaga.Salario).SetElementName("salario");
            map.MapMember(vaga => vaga.Status)
                .SetElementName("status")
                .SetSerializer(new EnumSerializer<StatusVaga>(BsonType.String));
            map.MapMember(vaga => vaga.DataAbertura).SetElementName("dataAbertura");
            map.MapMember(vaga => vaga.DataEncerramento)
                .SetElementName("dataEncerramento")
                .SetIgnoreIfNull(true);
        });
    }

    private static void RegisterSalario()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Salario)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Salario>(map =>
        {
            map.SetIgnoreExtraElements(true);
            map.MapCreator(salario => Salario.Create(salario.Valor, salario.Moeda));
            map.MapMember(salario => salario.Valor).SetElementName("valor");
            map.MapMember(salario => salario.Moeda).SetElementName("moeda");
        });
    }
}
