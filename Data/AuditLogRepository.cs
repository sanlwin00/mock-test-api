using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IMongoCollection<AuditLog> _collection;

        public AuditLogRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<AuditLog>("audit_logs");
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task CreateAsync(AuditLog auditLog)
        {
            if (auditLog == null)
            {
                throw new ArgumentNullException(nameof(auditLog));
            }

            if (string.IsNullOrEmpty(auditLog.Id))
            {
                auditLog.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(auditLog);
        }

        public async Task<MockTestUserCount> GetMockTestStartedEventCountAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    ["entityType"] = "MockTest",
                    ["action"] = "Started"
                }),
                new BsonDocument("$facet", new BsonDocument
                {
                    ["unknownUsers"] = new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("userName", "Unknown")),
                        new BsonDocument("$group", new BsonDocument
                        {
                            ["_id"] = new BsonDocument
                            {
                                ["userName"] = "$userName",
                                ["ipAddress"] = "$iPAddress"
                            }
                        }),
                        new BsonDocument("$count", "count")
                    },
                    ["knownUsers"] = new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("userName", new BsonDocument("$ne", "Unknown"))),
                        new BsonDocument("$group", new BsonDocument
                        {
                            ["_id"] = "$userName",
                            ["ip"] = new BsonDocument("$last", "$iPAddress")
                        }),
                        new BsonDocument("$count", "count")
                    }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    ["total_count"] = new BsonDocument("$add", new BsonArray
                    {
                        new BsonDocument("$ifNull", new BsonArray
                        {
                            new BsonDocument("$arrayElemAt", new BsonArray { "$unknownUsers.count", 0 }),
                            0
                        }),
                        new BsonDocument("$ifNull", new BsonArray
                        {
                            new BsonDocument("$arrayElemAt", new BsonArray { "$knownUsers.count", 0 }),
                            0
                        })
                    }),
                    ["unknownUser_count"] = new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$unknownUsers.count", 0 }),
                        0
                    }),
                    ["knownUser_count"] = new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$knownUsers.count", 0 }),
                        0
                    })
                })
            };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            if (result.Count == 0)
            {
                return new MockTestUserCount
                {
                    TotalCount = 0,
                    UnknownUserCount = 0,
                    KnownUserCount = 0
                };
            }

            var doc = result[0];
            return new MockTestUserCount
            {
                TotalCount = doc.GetValue("total_count", 0).AsInt32,
                UnknownUserCount = doc.GetValue("unknownUser_count", 0).AsInt32,
                KnownUserCount = doc.GetValue("knownUser_count", 0).AsInt32
            };
        }

        public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(int pageNumber, int pageSize, string? searchKeyword = null)
        {
            var filterBuilder = Builders<AuditLog>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var searchFilters = new List<FilterDefinition<AuditLog>>
                {
                    filterBuilder.Regex(x => x.UserName, new BsonRegularExpression(searchKeyword, "i")),
                    filterBuilder.Regex(x => x.Action, new BsonRegularExpression(searchKeyword, "i")),
                    filterBuilder.Regex(x => x.EntityType, new BsonRegularExpression(searchKeyword, "i")),
                    filterBuilder.Regex(x => x.EntityId, new BsonRegularExpression(searchKeyword, "i")),
                    filterBuilder.Regex(x => x.Details, new BsonRegularExpression(searchKeyword, "i")),
                    filterBuilder.Regex(x => x.IPAddress, new BsonRegularExpression(searchKeyword, "i"))
                };

                filter = filterBuilder.Or(searchFilters);
            }

            var totalCount = await _collection.CountDocumentsAsync(filter);
            var skip = (pageNumber - 1) * pageSize;

            var auditLogs = await _collection
                .Find(filter)
                .Sort(Builders<AuditLog>.Sort.Descending(x => x.Timestamp))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<AuditLog>
            {
                Items = auditLogs,
                TotalCount = (int)totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
