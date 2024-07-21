namespace MockTestApi.Models
{
    public class MockTest : IEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string QuestionSetId { get; set; }
        public List<MockTestQuestion> Questions { get; set; }
        public MockTestResults Results { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class MockTestQuestion
    {
        public string QuestionId { get; set; }
        public string SelectedOption { get; set; }
    }

    public class MockTestResults
    {
        public int Score { get; set; }
        public bool Passed { get; set; }
        public List<ResultDetail> Details { get; set; }
    }

    public class ResultDetail
    {
        public string QuestionId { get; set; }
        public bool Correct { get; set; }
    }
}
