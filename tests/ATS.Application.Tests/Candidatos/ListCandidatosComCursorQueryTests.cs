namespace ATS.Application.Tests.Candidatos;

using ATS.Application.Candidatos.Queries.ListCandidatos;

public class ListCandidatosComCursorQueryTests
{
    [Fact]
    public void DeveUsarValoresPadrao()
    {
        var query = new ListCandidatosComCursorQuery();

        Assert.Null(query.Cursor);
        Assert.Equal(20, query.Limite);
    }

    [Fact]
    public void DeveAtribuirValoresInformados()
    {
        var query = new ListCandidatosComCursorQuery("cursor-xyz", 50);

        Assert.Equal("cursor-xyz", query.Cursor);
        Assert.Equal(50, query.Limite);
    }
}
