using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;

namespace ATS.Infrastructure.Tests.Persistence.Repositories;

internal static class MongoRepositoryTestHelpers
{
    public static IAsyncCursor<T> CriarCursor<T>(IEnumerable<T> documentos)
    {
        var cursorMock = new Mock<IAsyncCursor<T>>();
        var lote = documentos.ToList();

        cursorMock
            .SetupSequence(cursor => cursor.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        cursorMock
            .SetupSequence(cursor => cursor.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorMock
            .SetupGet(cursor => cursor.Current)
            .Returns(lote);

        return cursorMock.Object;
    }

    public static BsonDocument Render<T>(FilterDefinition<T> filter) =>
        filter.Render(CriarRenderArgs<T>());

    public static BsonDocument Render<T>(IndexKeysDefinition<T> keys) =>
        keys.Render(CriarRenderArgs<T>());

    private static RenderArgs<T> CriarRenderArgs<T>() =>
        new(
            BsonSerializer.SerializerRegistry.GetSerializer<T>(),
            BsonSerializer.SerializerRegistry);
}
