using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Driver;

public class FlashCardRepository : IFlashCardRepository
{
    private readonly IMongoCollection<FlashCardDeck> _decks;
    private readonly IMongoCollection<FlashCardCard> _cards;
    private readonly IMongoCollection<FlashCardSession> _sessions;

    public FlashCardRepository(IMongoDatabase database)
    {
        _decks    = database.GetCollection<FlashCardDeck>("flashcard_decks");
        _cards    = database.GetCollection<FlashCardCard>("flashcard_cards");
        _sessions = database.GetCollection<FlashCardSession>("flashcard_sessions");
    }

    public async Task<IEnumerable<FlashCardDeck>> GetAllDecksAsync(string tenant)
    {
        return await _decks
            .Find(d => d.Tenant == tenant && d.IsPublished)
            .SortBy(d => d.Order)
            .ToListAsync();
    }

    public async Task<FlashCardDeck> GetDeckBySlugAsync(string slug)
    {
        return await _decks.Find(d => d.Slug == slug).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FlashCardCard>> GetCardsByDeckSlugAsync(string slug)
    {
        return await _cards
            .Find(c => c.DeckSlug == slug)
            .SortBy(c => c.Order)
            .ToListAsync();
    }

    public async Task<FlashCardSession> GetLastSessionAsync(string userId, string deckSlug)
    {
        return await _sessions
            .Find(s => s.UserId == userId && s.DeckSlug == deckSlug)
            .SortByDescending(s => s.CompletedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FlashCardSession>> GetLastSessionsByUserAsync(string userId)
    {
        // One document per deck slug — the most recent session for each
        var pipeline = new[]
        {
            new MongoDB.Bson.BsonDocument("$match",
                new MongoDB.Bson.BsonDocument("userId", userId)),
            new MongoDB.Bson.BsonDocument("$sort",
                new MongoDB.Bson.BsonDocument("completedAt", -1)),
            new MongoDB.Bson.BsonDocument("$group",
                new MongoDB.Bson.BsonDocument
                {
                    { "_id", "$deckSlug" },
                    { "doc", new MongoDB.Bson.BsonDocument("$first", "$$ROOT") }
                }),
            new MongoDB.Bson.BsonDocument("$replaceRoot",
                new MongoDB.Bson.BsonDocument("newRoot", "$doc"))
        };

        return await _sessions.Aggregate<FlashCardSession>(pipeline).ToListAsync();
    }

    public async Task SaveSessionAsync(FlashCardSession session)
    {
        await _sessions.InsertOneAsync(session);
    }
}
