namespace MockTestApi.Models
{
    public class Test : IEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Sequence { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<QuestionAccess> Questions { get; set; }
        public string Access { get; set; }
        public int? DurationMinutes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class QuestionAccess
    {
        public string QuestionId { get; set; }
        public string Access { get; set; }
        public int Sequence { get; set; }

    }
}
