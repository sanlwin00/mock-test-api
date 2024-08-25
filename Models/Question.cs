namespace MockTestApi.Models
{
    public class Question : IEntity
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public List<Option> Options { get; set; } // for multiple choice questions
        public string CorrectAnswer { get; set; } // for free-text answers
        public List<string> Tags { get; set; }
        public string Explanation { get; set; }
        public Reference Reference { get; set; }
        public string Image { get; set; }
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
        public string MaterialId { get; set; }
        public int Paragraph { get; set; }
    }
}
