using MongoDB.Bson;
using MongoDB.Driver;
using MockTestApi.Models;
using System;
using System.Collections.Generic;

namespace MockTestApi.Data
{
    public static class DummyData
    {
        public static List<User> GetUsers()
        {
            return new List<User>
            {
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Username = "johndoe",
                    Email = "johndoe@example.com",
                    PasswordHash = "hashedpassword",
                    Role = "user",
                    Profile = new Profile
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Dob = DateTime.Parse("1990-01-01"),
                        Preferences = new Preferences
                        {
                            Language = "English",
                            Timezone = "UTC"
                        }
                    },
                    Subscription = new Subscription
                    {
                        Plan = "free",
                        StartDate = DateTime.Parse("2024-06-01"),
                        EndDate = DateTime.Parse("2025-06-01")
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<Question> GetQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text = "What is the capital of France?",
                    Options = new List<Option>
                    {
                        new Option { OptionText = "Paris", IsCorrect = true, Image = "https://example.com/images/paris.jpg" },
                        new Option { OptionText = "London", IsCorrect = false, Image = "https://example.com/images/london.jpg" },
                        new Option { OptionText = "Berlin", IsCorrect = false, Image = "https://example.com/images/berlin.jpg" },
                        new Option { OptionText = "Madrid", IsCorrect = false, Image = "https://example.com/images/madrid.jpg" }
                    },
                    Category = "Geography",
                    Tags = new List<string> { "capital", "Europe" },
                    Explanation = "Paris is the capital city of France.",
                    Reference = new Reference { Section = "World Capitals", PageNumber = 45 },
                    QuestionImage = "https://example.com/images/capital_city.jpg",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<QuestionSet> GetQuestionSets()
        {
            return new List<QuestionSet>
            {
                new QuestionSet
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Basic Geography Set",
                    Description = "A set of basic geography questions.",
                    Questions = new List<QuestionAccess>
                    {
                        new QuestionAccess { QuestionId = ObjectId.GenerateNewId().ToString(), Access = "free" },
                        new QuestionAccess { QuestionId = ObjectId.GenerateNewId().ToString(), Access = "free" },
                        new QuestionAccess { QuestionId = ObjectId.GenerateNewId().ToString(), Access = "free" }
                    },
                    Access = "free",
                    CreatedBy = ObjectId.GenerateNewId().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<Payment> GetPayments()
        {
            return new List<Payment>
            {
                new Payment
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = ObjectId.GenerateNewId().ToString(),
                    PaymentSessionId = "session_12345",
                    Status = "completed",
                    Amount = 9.99m,
                    Currency = "USD",
                    BillingInfo = new BillingInfo
                    {
                        Surname = "Doe",
                        GivenName = "John",
                        Email = "johndoe@example.com",
                        Phone = "123456789",
                        City = "New York",
                        Province = "NY"
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<Notification> GetNotifications()
        {
            return new List<Notification>
            {
                new Notification
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = ObjectId.GenerateNewId().ToString(),
                    Type = "email",
                    Title = "Welcome",
                    Message = "Welcome to the mock test app!",
                    Status = "unread",
                    SentAt = DateTime.UtcNow
                }
            };
        }

        public static List<AuditLog> GetAuditLogs()
        {
            return new List<AuditLog>
            {
                new AuditLog
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = ObjectId.GenerateNewId().ToString(),
                    Action = "Login",
                    Details = "User logged in successfully.",
                    Timestamp = DateTime.UtcNow
                }
            };
        }

        public static List<UserSession> GetUserSessions()
        {
            return new List<UserSession>
            {
                new UserSession
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = ObjectId.GenerateNewId().ToString(),
                    SessionId = "session_12345",
                    IpAddress = "192.168.0.1",
                    UserAgent = "Mozilla/5.0",
                    LoginTime = DateTime.UtcNow,
                    LogoutTime = DateTime.UtcNow.AddHours(1)
                }
            };
        }

        public static List<MockTest> GetMockTests()
        {
            return new List<MockTest>
            {
                new MockTest
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Geography Basics Test",
                    Description = "A test on basic geography questions",
                    QuestionSetId = ObjectId.GenerateNewId().ToString(),
                    Questions = new List<MockTestQuestion>
                    {
                        new MockTestQuestion { QuestionId = ObjectId.GenerateNewId().ToString(), SelectedOption = "Paris" },
                        new MockTestQuestion { QuestionId = ObjectId.GenerateNewId().ToString(), SelectedOption = "Berlin" }
                    },
                    Results = new MockTestResults
                    {
                        Score = 1,
                        Passed = true,
                        Details = new List<ResultDetail>
                        {
                            new ResultDetail { QuestionId = ObjectId.GenerateNewId().ToString(), Correct = true },
                            new ResultDetail { QuestionId = ObjectId.GenerateNewId().ToString(), Correct = false }
                        }
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<MockTestHistory> GetMockTestHistories()
        {
            return new List<MockTestHistory>
            {
                new MockTestHistory
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = ObjectId.GenerateNewId().ToString(),
                    History = new List<TestHistoryItem>
                    {
                        new TestHistoryItem
                        {
                            MockTestId = ObjectId.GenerateNewId().ToString(),
                            Title = "Geography Basics Test",
                            Score = 1,
                            Passed = true,
                            TakenAt = DateTime.UtcNow
                        },
                        new TestHistoryItem
                        {
                            MockTestId = ObjectId.GenerateNewId().ToString(),
                            Title = "History Basics Test",
                            Score = 2,
                            Passed = false,
                            TakenAt = DateTime.UtcNow
                        }
                    }
                }
            };
        }
    }
}
