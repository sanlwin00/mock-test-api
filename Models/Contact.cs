namespace MockTestApi.Models
{
    public class ContactFormRequest
    {
        public string ToEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
