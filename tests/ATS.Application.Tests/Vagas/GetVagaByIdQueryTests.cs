using ATS.Application.Vagas.Queries.GetVagaById;

namespace ATS.Application.Tests.Vagas;

public class GetVagaByIdQueryTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
    public void DeveCriarQueryComIdDefinido(string idStr)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(id, new GetVagaByIdQuery(id).Id);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
    public void DeveSerIgualAOutraQueryComMesmoId(string idStr)
    {
        var id = Guid.Parse(idStr);
        Assert.Equal(new GetVagaByIdQuery(id), new GetVagaByIdQuery(id));
    }
}
