using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IFlashCardRepository
    {
        Task<IEnumerable<FlashCardDeck>> GetAllDecksAsync(string tenant);
        Task<FlashCardDeck> GetDeckBySlugAsync(string slug);
        Task<IEnumerable<FlashCardCard>> GetCardsByDeckSlugAsync(string slug);
        Task<FlashCardSession> GetLastSessionAsync(string userId, string deckSlug);
        Task<IEnumerable<FlashCardSession>> GetLastSessionsByUserAsync(string userId);
        Task SaveSessionAsync(FlashCardSession session);
    }
}
