using MongoDB.Driver;

namespace GreatVoidBattle.Infrastructure;

public static class MongoDbFactory
{
    public static IMongoDatabase Create(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        return client.GetDatabase(dbName);
    }
}