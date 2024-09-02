using MockTestApi.Models;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MockTestApi.Utils
{
    public static class MongoUtility
    {
        public static void RegisterConventions()
        {
            var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };
            ConventionRegistry.Register("CamelCaseConvention", conventionPack, type => true);
        }

        public static void SeedCollection<T>(IMongoDatabase database, string collectionName, List<T> items) where T : IEntity
        {
            var collection = database.GetCollection<T>(collectionName);
            var existingItems = collection.Find(_ => true).ToList();
            if (!existingItems.Any())
            {
                collection.InsertMany(items);
            }
        }
    }
}

