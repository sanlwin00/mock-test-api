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
                    Id = ObjectId.GenerateNewId().ToString(), // Assuming ObjectId is a method to generate new IDs
                    Email = "johndoe@example.com",
                    EmailVerified = true,
                    DisplayName = "John Doe",
                    PhoneNumber = "123456789",
                    PasswordHash = "hashedpassword",
                    PasswordSalt = "saltsalt",
                    Metadata = new Metadata
                    {
                        CreationTime = DateTime.UtcNow,
                        LastSignInTime = DateTime.UtcNow
                    },
                    CustomClaims = new CustomClaims
                    {
                        Admin = false
                    },
                    ProviderData = new List<ProviderData>
                    {
                        new ProviderData
                        {
                            ProviderId = "password",
                            Uid = "johndoe@example.com",
                            DisplayName = "John Doe",
                            Email = "johndoe@example.com",
                            PhoneNumber = "123456789",
                            PhotoURL = ""
                        }
                    },
                    Subscription = new Subscription
                    {
                        Plan = "free",
                        AccessCode = "FREE123",
                        StartDate = DateTime.Parse("2024-06-01"),
                        EndDate = DateTime.Parse("2025-06-01")
                    }
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
                        new Option { Text = "Paris", IsCorrect = true, Image = "https://example.com/images/paris.jpg" },
                        new Option { Text = "London", IsCorrect = false, Image = "https://example.com/images/london.jpg" },
                        new Option { Text = "Berlin", IsCorrect = false, Image = "https://example.com/images/berlin.jpg" },
                        new Option { Text = "Madrid", IsCorrect = false, Image = "https://example.com/images/madrid.jpg" }
                    },
                    Category = "Geography",
                    Tags = new List<string> { "capital", "Europe" },
                    Explanation = "Paris is the capital city of France.",
                    Reference = new Reference { Text = "World Capitals", Url = "45" },
                    QuestionImage = "https://example.com/images/capital_city.jpg",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public static List<Test> GetTests()
        {
            return new List<Test>
            {
                new Test
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
                    ModifiedAt = DateTime.UtcNow
                }
            };
        }

        public static List<Payment> GetPayments()
        {
            return new List<Payment>
            {
                new Payment
                {
                    Id = ObjectId.GenerateNewId().ToString(), // Assuming ObjectId is a method to generate new IDs
                    PaymentRef = "session_12345",
                    Amount = 9.99m,
                    Currency = "USD",
                    Status = "completed",
                    PaymentMethod = "paypal", // Example method, adjust as needed
                    Customer = ObjectId.GenerateNewId().ToString(), // Link to user ID
                    Description = "Payment for John Doe",
                    Created = DateTime.UtcNow,
                    BillingDetails = new BillingDetails
                    {
                        Name = "John Doe",
                        Email = "johndoe@example.com",
                        Phone = "123456789",
                        Address = new Address
                        {
                            City = "New York",
                            State = "NY",
                            Country = "US"
                        }
                    }
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

        public static List<MockTest> GetMockTests()
        {
            return new List<MockTest>
            {
                new MockTest
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Geography Basics Test",
                    Description = "A test on basic geography questions",
                    TestId = ObjectId.GenerateNewId().ToString(),
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
