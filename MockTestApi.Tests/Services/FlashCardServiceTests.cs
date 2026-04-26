using FluentAssertions;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services;
using Moq;

namespace MockTestApi.Tests.Services
{
    public class FlashCardServiceTests
    {
        private readonly Mock<IFlashCardRepository> _repoMock;
        private readonly FlashCardService _service;
        private const string Tenant = "citizenshiptest";

        public FlashCardServiceTests()
        {
            _repoMock = new Mock<IFlashCardRepository>();
            _service = new FlashCardService(_repoMock.Object);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static FlashCardDeck MakeDeck(string slug, int order = 1, int cardCount = 5) => new()
        {
            Id = slug + "-id",
            Slug = slug,
            Name = $"Deck {slug}",
            Blurb = "Test blurb",
            Tenant = Tenant,
            Order = order,
            IsPublished = true,
            CardCount = cardCount
        };

        private static FlashCardCard MakeCard(string deckSlug, int order) => new()
        {
            Id = $"{deckSlug}-card-{order}",
            DeckSlug = deckSlug,
            Term = $"Term {order}",
            Back = $"Back {order}",
            Order = order
        };

        private static FlashCardSession MakeSession(string userId, string deckSlug, int got, int skip, int total) => new()
        {
            Id = $"session-{deckSlug}",
            UserId = userId,
            DeckSlug = deckSlug,
            Got = got,
            Skip = skip,
            Total = total,
            CompletedAt = DateTime.UtcNow
        };

        // ── GetDecksAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetDecksAsync_NoUserId_ReturnsDeckDtosWithNullLastSession()
        {
            _repoMock.Setup(r => r.GetAllDecksAsync(Tenant))
                     .ReturnsAsync(new List<FlashCardDeck> { MakeDeck("history"), MakeDeck("geography", 2) });

            var result = (await _service.GetDecksAsync(Tenant)).ToList();

            result.Should().HaveCount(2);
            result[0].Slug.Should().Be("history");
            result[0].LastSession.Should().BeNull();
            result[1].Slug.Should().Be("geography");
            _repoMock.Verify(r => r.GetLastSessionsByUserAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetDecksAsync_WithUserId_AttachesLastSessionToDeck()
        {
            var decks = new List<FlashCardDeck> { MakeDeck("history"), MakeDeck("geography", 2) };
            var sessions = new List<FlashCardSession>
            {
                MakeSession("user1", "history", got: 8, skip: 2, total: 10)
            };

            _repoMock.Setup(r => r.GetAllDecksAsync(Tenant)).ReturnsAsync(decks);
            _repoMock.Setup(r => r.GetLastSessionsByUserAsync("user1")).ReturnsAsync(sessions);

            var result = (await _service.GetDecksAsync(Tenant, "user1")).ToList();

            result[0].LastSession.Should().NotBeNull();
            result[0].LastSession.Got.Should().Be(8);
            result[0].LastSession.Skip.Should().Be(2);
            result[0].LastSession.Total.Should().Be(10);
            result[1].LastSession.Should().BeNull();
        }

        [Fact]
        public async Task GetDecksAsync_EmptyDecks_ReturnsEmptyList()
        {
            _repoMock.Setup(r => r.GetAllDecksAsync(Tenant)).ReturnsAsync(new List<FlashCardDeck>());

            var result = await _service.GetDecksAsync(Tenant);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDecksAsync_MapsDeckFieldsCorrectly()
        {
            var deck = MakeDeck("government", order: 3, cardCount: 7);
            _repoMock.Setup(r => r.GetAllDecksAsync(Tenant)).ReturnsAsync(new List<FlashCardDeck> { deck });

            var result = (await _service.GetDecksAsync(Tenant)).First();

            result.Id.Should().Be("government-id");
            result.Slug.Should().Be("government");
            result.Name.Should().Be("Deck government");
            result.Order.Should().Be(3);
            result.CardCount.Should().Be(7);
        }

        // ── GetDeckWithCardsAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetDeckWithCardsAsync_UnknownSlug_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetDeckBySlugAsync("missing")).ReturnsAsync((FlashCardDeck)null);

            var result = await _service.GetDeckWithCardsAsync("missing");

            result.Should().BeNull();
            _repoMock.Verify(r => r.GetCardsByDeckSlugAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetDeckWithCardsAsync_ValidSlug_ReturnsDeckWithCards()
        {
            var deck = MakeDeck("history", cardCount: 2);
            var cards = new List<FlashCardCard> { MakeCard("history", 1), MakeCard("history", 2) };

            _repoMock.Setup(r => r.GetDeckBySlugAsync("history")).ReturnsAsync(deck);
            _repoMock.Setup(r => r.GetCardsByDeckSlugAsync("history")).ReturnsAsync(cards);

            var result = await _service.GetDeckWithCardsAsync("history");

            result.Should().NotBeNull();
            result.Slug.Should().Be("history");
            result.Cards.Should().HaveCount(2);
            result.Cards[0].Term.Should().Be("Term 1");
            result.Cards[1].Term.Should().Be("Term 2");
        }

        [Fact]
        public async Task GetDeckWithCardsAsync_MapsCardFieldsCorrectly()
        {
            var deck = MakeDeck("symbols");
            var card = MakeCard("symbols", 3);

            _repoMock.Setup(r => r.GetDeckBySlugAsync("symbols")).ReturnsAsync(deck);
            _repoMock.Setup(r => r.GetCardsByDeckSlugAsync("symbols")).ReturnsAsync(new List<FlashCardCard> { card });

            var result = await _service.GetDeckWithCardsAsync("symbols");

            result.Cards[0].Id.Should().Be("symbols-card-3");
            result.Cards[0].Back.Should().Be("Back 3");
            result.Cards[0].Order.Should().Be(3);
        }

        // ── SaveSessionAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task SaveSessionAsync_CallsRepositoryWithCorrectData()
        {
            FlashCardSession saved = null;
            _repoMock.Setup(r => r.SaveSessionAsync(It.IsAny<FlashCardSession>()))
                     .Callback<FlashCardSession>(s => saved = s)
                     .Returns(Task.CompletedTask);

            var dto = new CreateFlashCardSessionDto { DeckSlug = "history", Got = 7, Skip = 3, Total = 10 };
            await _service.SaveSessionAsync("user42", dto);

            saved.Should().NotBeNull();
            saved.UserId.Should().Be("user42");
            saved.DeckSlug.Should().Be("history");
            saved.Got.Should().Be(7);
            saved.Skip.Should().Be(3);
            saved.Total.Should().Be(10);
            saved.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
