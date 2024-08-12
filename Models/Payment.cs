namespace MockTestApi.Models
{
    public class Payment : IEntity
    {
        public string Id { get; set; } 
        public string PaymentRef { get; set; }
        public double Amount { get; set; } 
        public string Currency { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string Customer { get; set; }
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

        public string PostalCode { get; set; }
    }

    public class PaymentDto
    {
        public string Id { get; set; }
        public string PaymentRef { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string Customer { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
    }
}