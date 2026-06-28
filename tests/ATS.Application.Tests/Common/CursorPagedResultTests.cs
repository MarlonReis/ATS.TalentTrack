namespace ATS.Application.Tests.Common;

using ATS.Application.Common.Pagination;

public class CursorPagedResultTests
{
    [Fact]
    public void DeveArmazenarItensECursor()
    {
        var items = new[] { "a", "b", "c" };
        var result = new CursorPagedResult<string>(items, "cursor123", true);

        Assert.Equal(items, result.Items);
        Assert.Equal("cursor123", result.ProximoCursor);
        Assert.True(result.TemMais);
    }

    [Fact]
    public void DevePermitirCursorNuloETemMaisFalso()
    {
        var result = new CursorPagedResult<int>([], null, false);

        Assert.Empty(result.Items);
        Assert.Null(result.ProximoCursor);
        Assert.False(result.TemMais);
    }

    [Fact]
    public void DoisRegistrosComMesmosValoresDevemSerIguais()
    {
        var items = new[] { 1, 2 };
        var a = new CursorPagedResult<int>(items, "cur", true);
        var b = new CursorPagedResult<int>(items, "cur", true);

        Assert.Equal(a, b);
    }
}
