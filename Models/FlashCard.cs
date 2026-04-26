namespace MockTestApi.Models
{
    public class FlashCardDeck : IEntity
    {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Blurb { get; set; }
        public string Tenant { get; set; }
        public int Order { get; set; }
        public bool IsPublished { get; set; }
        public int CardCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class FlashCardCard : IEntity
    {
        public string Id { get; set; }
        public string DeckSlug { get; set; }
        public string Term { get; set; }
        public string Back { get; set; }
        public int Order { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class FlashCardSession : IEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DeckSlug { get; set; }
        public int Got { get; set; }
        public int Skip { get; set; }
        public int Total { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    public class FlashCardDeckDto
    {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Blurb { get; set; }
        public int Order { get; set; }
        public int CardCount { get; set; }
        public FlashCardLastSessionDto LastSession { get; set; }
    }

    public class FlashCardDeckDetailDto : FlashCardDeckDto
    {
        public List<FlashCardCardDto> Cards { get; set; }
    }

    public class FlashCardCardDto
    {
        public string Id { get; set; }
        public string Term { get; set; }
        public string Back { get; set; }
        public int Order { get; set; }
    }

    public class FlashCardLastSessionDto
    {
        public int Got { get; set; }
        public int Skip { get; set; }
        public int Total { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class CreateFlashCardSessionDto
    {
        public string DeckSlug { get; set; }
        public int Got { get; set; }
        public int Skip { get; set; }
        public int Total { get; set; }
    }
}
