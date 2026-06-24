using ATS.Application.Candidatos.Queries.GetCandidatoById;

public class GetCandidatoByIdQueryTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public void DeveCriarQueryComIdDefinido(string idStr)
    {
        var id = Guid.Parse(idStr);
        var query = new GetCandidatoByIdQuery(id);

        Assert.Equal(id, query.Id);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveSerIgualAOutraQueryComMesmoId(string idStr)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(new GetCandidatoByIdQuery(id), new GetCandidatoByIdQuery(id));
    }
}
