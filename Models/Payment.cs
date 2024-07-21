namespace MockTestApi.Models
{
    public class Payment : IEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PaymentSessionId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public BillingInfo BillingInfo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BillingInfo
    {
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
    }
}
