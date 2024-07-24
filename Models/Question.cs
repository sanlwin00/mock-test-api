namespace MockTestApi.Models
{
    public class Question : IEntity
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<Option> Options { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; }
        public string Explanation { get; set; }
        public Reference Reference { get; set; }
        public string QuestionImage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class Option
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public string Image { get; set; }
    }

    public class Reference
    {
        public string Text { get; set; }
        public string Url { get; set; }
    }
}
