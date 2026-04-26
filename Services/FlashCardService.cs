using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class FlashCardService : IFlashCardService
    {
        private readonly IFlashCardRepository _repository;

        public FlashCardService(IFlashCardRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<FlashCardDeckDto>> GetDecksAsync(string tenant, string? userId = null)
        {
            var decks = await _repository.GetAllDecksAsync(tenant);

            Dictionary<string, FlashCardSession> sessionMap = new();
            if (!string.IsNullOrEmpty(userId))
            {
                var sessions = await _repository.GetLastSessionsByUserAsync(userId);
                sessionMap = sessions.ToDictionary(s => s.DeckSlug);
            }

            return decks.Select(d => new FlashCardDeckDto
            {
                Id        = d.Id,
                Slug      = d.Slug,
                Name      = d.Name,
                Blurb     = d.Blurb,
                Order     = d.Order,
                CardCount = d.CardCount,
                LastSession = sessionMap.TryGetValue(d.Slug, out var session)
                    ? new FlashCardLastSessionDto
                    {
                        Got         = session.Got,
                        Skip        = session.Skip,
                        Total       = session.Total,
                        CompletedAt = session.CompletedAt
                    }
                    : null
            });
        }

        public async Task<FlashCardDeckDetailDto> GetDeckWithCardsAsync(string slug)
        {
            var deck = await _repository.GetDeckBySlugAsync(slug);
            if (deck == null) return null;

            var cards = await _repository.GetCardsByDeckSlugAsync(slug);

            return new FlashCardDeckDetailDto
            {
                Id        = deck.Id,
                Slug      = deck.Slug,
                Name      = deck.Name,
                Blurb     = deck.Blurb,
                Order     = deck.Order,
                CardCount = deck.CardCount,
                Cards     = cards.Select(c => new FlashCardCardDto
                {
                    Id    = c.Id,
                    Term  = c.Term,
                    Back  = c.Back,
                    Order = c.Order
                }).ToList()
            };
        }

        public async Task SaveSessionAsync(string userId, CreateFlashCardSessionDto dto)
        {
            var session = new FlashCardSession
            {
                UserId      = userId,
                DeckSlug    = dto.DeckSlug,
                Got         = dto.Got,
                Skip        = dto.Skip,
                Total       = dto.Total,
                CompletedAt = DateTime.UtcNow
            };
            await _repository.SaveSessionAsync(session);
        }
    }
}
