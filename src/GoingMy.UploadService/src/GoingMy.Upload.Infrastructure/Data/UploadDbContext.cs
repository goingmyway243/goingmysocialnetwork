using GoingMy.Upload.Domain.Entities;
using MongoDB.Driver;

namespace GoingMy.Upload.Infrastructure.Data;

public class UploadDbContext
{
    private readonly IMongoDatabase _database;

    public IMongoCollection<MediaFile> MediaFiles => _database.GetCollection<MediaFile>("media_files");

    public UploadDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    public async Task InitializeAsync()
    {
        var indexKeys = Builders<MediaFile>.IndexKeys;

        var byUserIndex = new CreateIndexModel<MediaFile>(
            indexKeys.Combine(
                indexKeys.Ascending(f => f.UploadedByUserId),
                indexKeys.Descending(f => f.CreatedAt)));

        var byStatusIndex = new CreateIndexModel<MediaFile>(
            indexKeys.Combine(
                indexKeys.Ascending(f => f.Status),
                indexKeys.Ascending(f => f.CreatedAt)));

        await MediaFiles.Indexes.CreateManyAsync([byUserIndex, byStatusIndex]);
    }
}
