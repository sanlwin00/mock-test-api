namespace MockTestApi.Models
{
    public class MyDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class User : IEntity
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public Profile Profile { get; set; }
        public Subscription Subscription { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class Profile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Dob { get; set; }
        public Preferences Preferences { get; set; }
    }

    public class Preferences
    {
        public string Language { get; set; }
        public string Timezone { get; set; }
    }

    public class Subscription
    {
        public string Plan { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
