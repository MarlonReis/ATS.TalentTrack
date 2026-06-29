namespace ATS.Application.Tests.Vagas;

using ATS.Application.Vagas.Queries.ListVagas;
using ATS.Domain.Vagas.Enums;

public class ListVagasComCursorQueryTests
{
    [Fact]
    public void DeveUsarValoresPadrao()
    {
        var query = new ListVagasComCursorQuery();

        Assert.Null(query.Cursor);
        Assert.Equal(20, query.Limite);
        Assert.Null(query.Status);
    }

    [Fact]
    public void DeveAtribuirValoresInformados()
    {
        var query = new ListVagasComCursorQuery("cursor-abc", 10, StatusVaga.Aberta);

        Assert.Equal("cursor-abc", query.Cursor);
        Assert.Equal(10, query.Limite);
        Assert.Equal(StatusVaga.Aberta, query.Status);
    }
}
