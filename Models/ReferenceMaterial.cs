namespace MockTestApi.Models
{
    public class ReferenceMaterial: IEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public List<string> Content { get; set; }
        public List<string> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ReferenceMaterial()
        {
            Content = new List<string>();
            Tags = new List<string>();
        }
    }
}
