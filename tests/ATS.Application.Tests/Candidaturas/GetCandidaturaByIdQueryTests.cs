using ATS.Application.Candidaturas.Queries.GetCandidaturaById;

public class GetCandidaturaByIdQueryTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    public void DeveCriarQueryComIdDefinido(string idStr)
    {
        var id = Guid.Parse(idStr);

        var query = new GetCandidaturaByIdQuery(id);

        Assert.Equal(id, query.Id);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveSerIgualAOutraQueryComMesmoId(string idStr)
    {
        var a = new GetCandidaturaByIdQuery(Guid.Parse(idStr));
        var b = new GetCandidaturaByIdQuery(Guid.Parse(idStr));

        Assert.Equal(a, b);
    }
}
