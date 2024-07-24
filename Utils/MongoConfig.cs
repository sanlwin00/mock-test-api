using MongoDB.Bson.Serialization.Conventions;

namespace MockTestApi.Utils
{
    public static class MongoConfig
    {
        public static void RegisterConventions()
        {
            var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };
            ConventionRegistry.Register("CamelCaseConvention", conventionPack, type => true);
        }
    }
}

