using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

public class QuestionRepository : IQuestionRepository
{
    private readonly IMongoCollection<Question> _collection;

    public QuestionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Question>("questions");
    }

    public async Task<IEnumerable<Question>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
    
}