using MongoDB.Driver;
using Mongo2Go;
using MockTestApi.Utils;

namespace MockTestApi.Tests.Helpers
{
    public class MongoTestHelper : IDisposable
    {
        private MongoDbRunner _runner;
        private IMongoDatabase _database;
        private readonly bool _useInMemory;

        public MongoTestHelper(bool useInMemory = true)
        {
            _useInMemory = useInMemory;
            SetupDatabase();
        }

        public IMongoDatabase Database => _database;

        private void SetupDatabase()
        {
            if (_useInMemory)
            {
                _runner = MongoDbRunner.Start();
                var client = new MongoClient(_runner.ConnectionString);
                _database = client.GetDatabase("MockTestDb_Test");
            }
            else
            {
                // For integration tests with real MongoDB
                var connectionString = Environment.GetEnvironmentVariable("MONGODB_TEST_CONNECTION_STRING")
                    ?? "mongodb://localhost:27017";
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase($"MockTestDb_Test_{Guid.NewGuid():N}");
            }

            // Register MongoDB conventions
            MongoUtility.RegisterConventions();
        }

        public async Task ClearCollectionAsync<T>(string collectionName)
        {
            var collection = _database.GetCollection<T>(collectionName);
            await collection.DeleteManyAsync(FilterDefinition<T>.Empty);
        }

        public async Task SeedCollectionAsync<T>(string collectionName, IEnumerable<T> documents)
        {
            var collection = _database.GetCollection<T>(collectionName);
            await collection.InsertManyAsync(documents);
        }

        public void Dispose()
        {
            if (_useInMemory)
            {
                _runner?.Dispose();
            }
            else
            {
                // Clean up test database
                try
                {
                    var client = _database.Client;
                    client.DropDatabase(_database.DatabaseNamespace.DatabaseName);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}