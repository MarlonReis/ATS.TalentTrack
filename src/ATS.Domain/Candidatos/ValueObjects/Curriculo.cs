namespace ATS.Domain.Candidatos.ValueObjects;

using ATS.Domain.Shared;

public sealed class Curriculo : ValueObject
{
    public string NomeArquivo { get; }
    public string ContentType { get; }
    public string UrlOuBase64 { get; }
    public DateTime DataUpload { get; }

    private static readonly string[] _extensoesPermitidas = { ".pdf", ".doc", ".docx" };

    private Curriculo(string nomeArquivo, string contentType, string urlOuBase64)
    {
        NomeArquivo = nomeArquivo;
        ContentType = contentType;
        UrlOuBase64 = urlOuBase64;
        DataUpload = DateTime.UtcNow;
    }

    public static Curriculo Create(string nomeArquivo, string contentType, string urlOuBase64)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo))
        {
            throw new DomainException("Nome do arquivo do currículo é obrigatório.");
        }

        var extensao = Path.GetExtension(nomeArquivo).ToLower();
        if (!_extensoesPermitidas.Contains(extensao))
        {
            throw new DomainException($"Formato '{extensao}' não permitido. Use PDF, DOC ou DOCX.");
        }

        if (string.IsNullOrWhiteSpace(urlOuBase64))
        {
            throw new DomainException("Conteúdo do currículo é obrigatório.");
        }

        return new Curriculo(nomeArquivo, contentType, urlOuBase64);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return NomeArquivo;
        yield return DataUpload;
    }
}
