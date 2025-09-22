namespace MockTestApi.Models
{
    public class MockTest : IEntity
    {
        public string Id { get; set; }        
        public string Title { get; set; }
        public string Description { get; set; }
        public string TestId { get; set; }
        public List<MockTestQuestion> Questions { get; set; }
        public MockTestResults Results { get; set; }
        public string UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class MockTestQuestion
    {
        public string QuestionId { get; set; }
        public int? SelectedOption { get; set; }
        public string UserAnswer { get; set; }
        public bool ReviewLater { get; set; }
        public int Number { get; set; }
    }

    public class MockTestResults
    {
        public double Score { get; set; }
        public bool Passed { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public List<ResultDetail> Details { get; set; }
    }

    public class ResultDetail
    {
        public string QuestionId { get; set; }
        public bool Correct { get; set; }
    }    

    public class UpdateMockTestDto
    {
        public string QuestionId { get; set; }
        public int? SelectedOption { get; set; }
        public string UserAnswer { get; set; }
        public bool ReviewLater { get; set; }
    }

    public class CompleteMockTestDto
    {
        public List<MockTestQuestion> Questions { get; set; }
        public MockTestResults Results { get; set; }
    }

}
