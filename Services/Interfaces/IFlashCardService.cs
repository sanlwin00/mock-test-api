using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IFlashCardService
    {
        Task<IEnumerable<FlashCardDeckDto>> GetDecksAsync(string tenant, string? userId = null);
        Task<FlashCardDeckDetailDto> GetDeckWithCardsAsync(string slug);
        Task SaveSessionAsync(string userId, CreateFlashCardSessionDto dto);
    }
}
