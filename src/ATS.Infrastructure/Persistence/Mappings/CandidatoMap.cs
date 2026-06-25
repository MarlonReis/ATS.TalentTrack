namespace ATS.Infrastructure.Persistence.Mappings;

using System.Reflection;
using ATS.Domain.Candidatos.Entities;
using ATS.Domain.Candidatos.ValueObjects;
using ATS.Domain.Shared;
using MongoDB.Bson.Serialization;

public static class CandidatoMap
{
    public static void Register()
    {
        RegisterEntity();
        RegisterEmail();
        RegisterTelefone();
        RegisterCurriculo();

        if (BsonClassMap.IsClassMapRegistered(typeof(Candidato)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Candidato>(map =>
        {
            map.SetIgnoreExtraElements(true);
            map.MapMember(candidato => candidato.Nome).SetElementName("nome");
            map.MapMember(candidato => candidato.Email).SetElementName("email");
            map.MapMember(candidato => candidato.Telefone).SetElementName("telefone");
            map.MapMember(candidato => candidato.Curriculo)
                .SetElementName("curriculo")
                .SetIgnoreIfNull(true);
            map.MapMember(candidato => candidato.DataCadastro).SetElementName("dataCadastro");
        });
    }

    private static void RegisterEntity()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Entity)))
        {
            BsonClassMap.RegisterClassMap<Entity>(map =>
            {
                map.SetIsRootClass(true);
                map.SetIgnoreExtraElements(true);
                map.MapIdMember(entity => entity.Id);
                map.UnmapMember(entity => entity.DomainEvents);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(AggregateRoot)))
        {
            BsonClassMap.RegisterClassMap<AggregateRoot>(map =>
            {
                map.SetIgnoreExtraElements(true);
            });
        }
    }

    private static void RegisterEmail()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Email)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Email>(map =>
        {
            map.SetIgnoreExtraElements(true);
            map.MapCreator(email => Email.Create(email.Value));
            map.MapMember(email => email.Value).SetElementName("value");
        });
    }

    private static void RegisterTelefone()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Telefone)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Telefone>(map =>
        {
            map.SetIgnoreExtraElements(true);
            map.MapCreator(telefone => Telefone.Create(telefone.Value));
            map.MapMember(telefone => telefone.Value).SetElementName("value");
        });
    }

    private static void RegisterCurriculo()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Curriculo)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Curriculo>(map =>
        {
            map.SetIgnoreExtraElements(true);
            map.MapCreator(curriculo => CreateCurriculo(
                curriculo.NomeArquivo,
                curriculo.ContentType,
                curriculo.UrlOuBase64,
                curriculo.DataUpload));
            map.MapMember(curriculo => curriculo.NomeArquivo).SetElementName("nomeArquivo");
            map.MapMember(curriculo => curriculo.ContentType).SetElementName("contentType");
            map.MapMember(curriculo => curriculo.UrlOuBase64).SetElementName("urlOuBase64");
            map.MapMember(curriculo => curriculo.DataUpload).SetElementName("dataUpload");
        });
    }

    private static Curriculo CreateCurriculo(
        string nomeArquivo,
        string contentType,
        string urlOuBase64,
        DateTime dataUpload)
    {
        var curriculo = Curriculo.Create(nomeArquivo, contentType, urlOuBase64);
        SetReadonlyAutoProperty(curriculo, nameof(Curriculo.DataUpload), dataUpload);
        return curriculo;
    }

    private static void SetReadonlyAutoProperty<T>(
        T target,
        string propertyName,
        object value)
        where T : notnull
    {
        var field = typeof(T).GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        field?.SetValue(target, value);
    }
}
