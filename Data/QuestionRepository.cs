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

    public async Task<Question> GetByIdAsync(string id)
    {
        return await _collection.Find(q => q.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Question question)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        if (string.IsNullOrEmpty(question.Id))
        {
            question.Id = ObjectId.GenerateNewId().ToString();
        }

        await _collection.InsertOneAsync(question);
    }

    public async Task<bool> UpdateAsync(Question question)
    {
        var result = await _collection.ReplaceOneAsync(q => q.Id == question.Id, question);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(q => q.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}