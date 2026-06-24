using ATS.Application.Candidatos.Commands.UpdateCandidato;

public class UpdateCandidatoCommandTests
{
    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "João Silva", "joao@email.com", "11912345678")]
    [InlineData("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "Maria Santos", "maria@corp.com", "21987654321")]
    public void DeveCriarCommandComPropriedadesDefinidas(
        string idStr, string nome, string email, string telefone)
    {
        var command = new UpdateCandidatoCommand(Guid.Parse(idStr), nome, email, telefone);

        Assert.Equal(Guid.Parse(idStr), command.Id);
        Assert.Equal(nome, command.Nome);
        Assert.Equal(email, command.Email);
        Assert.Equal(telefone, command.Telefone);
    }

    [Theory]
    [InlineData("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "João", "joao@email.com", "11912345678")]
    public void DeveSerIgualAOutroCommandComMesmosValores(
        string idStr, string nome, string email, string tel)
    {
        var id = Guid.Parse(idStr);
        var a = new UpdateCandidatoCommand(id, nome, email, tel);
        var b = new UpdateCandidatoCommand(id, nome, email, tel);

        Assert.Equal(a, b);
    }
}
