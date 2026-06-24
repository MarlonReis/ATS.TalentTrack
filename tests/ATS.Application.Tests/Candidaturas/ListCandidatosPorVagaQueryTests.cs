using ATS.Application.Candidaturas.Queries.ListCandidatosPorVaga;

public class ListCandidatosPorVagaQueryTests
{
    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    public void DeveCriarQueryComVagaIdDefinido(string vagaIdStr)
    {
        var vagaId = Guid.Parse(vagaIdStr);

        var query = new ListCandidatosPorVagaQuery(vagaId);

        Assert.Equal(vagaId, query.VagaId);
    }

    [Theory]
    [InlineData("cccccccc-cccc-cccc-cccc-cccccccccccc")]
    public void DeveSerIgualAOutraQueryComMesmoVagaId(string vagaIdStr)
    {
        var a = new ListCandidatosPorVagaQuery(Guid.Parse(vagaIdStr));
        var b = new ListCandidatosPorVagaQuery(Guid.Parse(vagaIdStr));

        Assert.Equal(a, b);
    }
}
