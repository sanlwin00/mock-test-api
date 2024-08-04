namespace MockTestApi.Models
{
    public class PasswordResetToken : IEntity
    {
        public string Id { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string UserId { get; set; }
    }

}
