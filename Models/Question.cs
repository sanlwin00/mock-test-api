namespace MockTestApi.Models
{
    public class QuestionDto : IEntity
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string TextLocal { get; set; }
        public string Type { get; set; }
        public List<OptionDto> Options { get; set; } // for multiple choice questions
        public string CorrectAnswer { get; set; } // for free-text answers
        public List<string> Tags { get; set; }
        public string Explanation { get; set; }
        public string ExplanationLocal { get; set; }
        public Reference Reference { get; set; }
        public string Image { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OptionDto
    {
        public string Text { get; set; }
        public string TextLocal { get; set; }
        public bool IsCorrect { get; set; }
        public string Image { get; set; }
    }

    public class Reference
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public int Page { get; set; }
        public string MaterialId { get; set; }
        public int Paragraph { get; set; }
    }

    public class Question : IEntity
    {
        public string Id { get; set; }
        public Dictionary<string, string> Text { get; set; } 
        public string Type { get; set; }
        public List<Option> Options { get; set; } 
        public string CorrectAnswer { get; set; } 
        public List<string> Tags { get; set; }
        public Dictionary<string, string> Explanation { get; set; } 
        public Reference Reference { get; set; }
        public string Image { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class Option
    {
        public Dictionary<string, string> Text { get; set; } 
        public bool IsCorrect { get; set; }
        public string Image { get; set; }
    }

}
