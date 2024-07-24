namespace MockTestApi.Models
{
    public class Payment : IEntity
    {
        public string Id { get; set; } // _id in MongoDB, renamed to Id for C#
        public string PaymentRef { get; set; }
        public decimal Amount { get; set; } // Using decimal for currency
        public string Currency { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string Customer { get; set; } // This might be a user ID or a customer ID
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public BillingDetails BillingDetails { get; set; }
    }

    public class BillingDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}