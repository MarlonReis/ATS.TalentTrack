using ATS.Application.Candidatos.Commands.AddCurriculo;

public class AddCurriculoCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "cv.pdf", "application/pdf", "base64==")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "portfolio.docx", "application/vnd.openxmlformats", "url")]
    public void DeveCriarCommandComPropriedadesDefinidas(
        string candidatoIdStr, string nomeArquivo, string contentType, string url)
    {
        var id = Guid.Parse(candidatoIdStr);
        var command = new AddCurriculoCommand(id, nomeArquivo, contentType, url);

        Assert.Equal(id, command.CandidatoId);
        Assert.Equal(nomeArquivo, command.NomeArquivo);
        Assert.Equal(contentType, command.ContentType);
        Assert.Equal(url, command.UrlOuBase64);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "cv.pdf", "application/pdf", "data")]
    public void DeveSerIgualAOutroCommandComMesmosValores(
        string idStr, string nome, string ct, string url)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(
            new AddCurriculoCommand(id, nome, ct, url),
            new AddCurriculoCommand(id, nome, ct, url));
    }
}
