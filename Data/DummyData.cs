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
                // Multiple Choice Question 1
                new Question
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text = "What is the largest planet in our solar system?",
                    Type = "MultipleChoice",
                    Options = new List<Option>
                    {
                        new Option { Text = "Earth", IsCorrect = false, Image = "https://example.com/images/earth.jpg" },
                        new Option { Text = "Jupiter", IsCorrect = true, Image = "https://example.com/images/jupiter.jpg" },
                        new Option { Text = "Saturn", IsCorrect = false, Image = "https://example.com/images/saturn.jpg" },
                        new Option { Text = "Mars", IsCorrect = false, Image = "https://example.com/images/mars.jpg" }
                    },
                    Tags = new List<string> { "planet", "solar system", "astronomy" },
                    Explanation = "Jupiter is the largest planet in our solar system.",
                    Reference = new Reference { Text = "NASA - Jupiter", Url = "https://example.com/jupiter" },
                    Image = "https://example.com/images/jupiter_large.jpg",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Multiple Choice Question 2
                new Question
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text = "Which element has the chemical symbol 'O'?",
                    Type = "MultipleChoice",
                    Options = new List<Option>
                    {
                        new Option { Text = "Oxygen", IsCorrect = true, Image = "https://example.com/images/oxygen.jpg" },
                        new Option { Text = "Gold", IsCorrect = false, Image = "https://example.com/images/gold.jpg" },
                        new Option { Text = "Osmium", IsCorrect = false, Image = "https://example.com/images/osmium.jpg" },
                        new Option { Text = "Silver", IsCorrect = false, Image = "https://example.com/images/silver.jpg" }
                    },
                    Tags = new List<string> { "chemistry", "elements", "periodic table" },
                    Explanation = "The chemical symbol 'O' stands for Oxygen.",
                    Reference = new Reference { Text = "Periodic Table", Url = "https://example.com/periodic_table" },
                    Image = "https://example.com/images/oxygen_element.jpg",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Multiple Choice Question 3
                new Question
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text = "Who wrote 'To Kill a Mockingbird'?",
                    Type = "MultipleChoice",
                    Options = new List<Option>
                    {
                        new Option { Text = "Harper Lee", IsCorrect = true, Image = "https://example.com/images/harper_lee.jpg" },
                        new Option { Text = "Mark Twain", IsCorrect = false, Image = "https://example.com/images/mark_twain.jpg" },
                        new Option { Text = "J.K. Rowling", IsCorrect = false, Image = "https://example.com/images/jk_rowling.jpg" },
                        new Option { Text = "Ernest Hemingway", IsCorrect = false, Image = "https://example.com/images/ernest_hemingway.jpg" }
                    },
                    Tags = new List<string> { "literature", "books", "authors" },
                    Explanation = "'To Kill a Mockingbird' was written by Harper Lee.",
                    Reference = new Reference { Text = "Harper Lee Biography", Url = "https://example.com/harper_lee" },
                    Image = "https://example.com/images/to_kill_a_mockingbird.jpg",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Free Text Question 1
                new Question
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text = "What is the capital of France?",
                    Type = "FreeText",
                    Options = new List<Option>(), // No options needed for free-text questions
                    CorrectAnswer = "Paris",
                    Tags = new List<string> { "geography", "capital cities", "France" },
                    Explanation = "The capital city of France is Paris.",
                    Reference = new Reference { Text = "Capital Cities of Europe", Url = "https://example.com/capital_cities_europe" },
                    Image = "https://example.com/images/paris.jpg",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Free Text Question 2
                new Question
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text = "Which country is known as the Land of the Rising Sun?",
                    Type = "FreeText",
                    Options = new List<Option>(), // No options needed for free-text questions
                    CorrectAnswer = "Japan",
                    Tags = new List<string> { "geography", "country names", "Japan" },
                    Explanation = "Japan is often referred to as the Land of the Rising Sun.",
                    Reference = new Reference { Text = "Japan Overview", Url = "https://example.com/japan_overview" },
                    Image = "https://example.com/images/japan.jpg",
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
                    Sequence = 1,
                    Description = "A set of basic geography questions.",
                    Category = "Geography",
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
                    Amount = 9.99,
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
