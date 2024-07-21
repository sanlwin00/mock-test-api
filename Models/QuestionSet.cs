namespace MockTestApi.Models
{
    public class QuestionSet : IEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<QuestionAccess> Questions { get; set; }
        public string Access { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class QuestionAccess
    {
        public string QuestionId { get; set; }
        public string Access { get; set; }
    }
}
