namespace MockTestApi.Models
{
    public class MockTestHistory : IEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public List<TestHistoryItem> History { get; set; }
    }

    public class TestHistoryItem
    {
        public string MockTestId { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }
        public DateTime? TakenAt { get; set; }
    }
}
