using System.Text.Json.Serialization;

namespace MockTestApi.Models
{
    public class User : IEntity
    {
        public string Id { get; set; } // _id in MongoDB, renamed to Id for C#
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string DisplayName { get; set; }
        public string PhotoURL { get; set; }
        public string PhoneNumber { get; set; }
        public List<ProviderData> ProviderData { get; set; }
        public CustomClaims CustomClaims { get; set; }
        public Metadata Metadata { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public Subscription Subscription { get; set; }
    }

    public class ProviderData
    {
        public string ProviderId { get; set; }
        public string Uid { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhotoURL { get; set; }
    }

    public class CustomClaims
    {
        public bool Admin { get; set; }
    }

    public class Metadata
    {
        public DateTime CreationTime { get; set; }
        public DateTime LastSignInTime { get; set; }
    }

    public class Subscription
    {
        public string Plan { get; set; }
        public string AccessCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SubscriptionDto
    {
        public string Plan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsExpired
        {
            get
            {
                // Check if the plan is not free and if the end date is in the past
                return Plan != "free" && EndDate < DateTime.UtcNow;
            }
        }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string DisplayName { get; set; }
        public string PhotoURL { get; set; }
        public string PhoneNumber { get; set; }
        public List<ProviderData> ProviderData { get; set; }
        public CustomClaims CustomClaims { get; set; }
        public Metadata Metadata { get; set; }
        public SubscriptionDto Subscription { get; set; }
    }

    

}
