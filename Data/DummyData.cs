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
                    Id = "g001",
                    Text = "What is the largest planet in our solar system?",
                    Type = "MultipleChoice",
                    Options = new List<Option>
                    {
                        new Option { Text = "Earth", IsCorrect = false, Image = null },
                        new Option { Text = "Jupiter", IsCorrect = true, Image = null },
                        new Option { Text = "Saturn", IsCorrect = false, Image = null },
                        new Option { Text = "Mars", IsCorrect = false, Image = null }
                    },
                    Tags = new List<string> { "planet", "solar system", "astronomy" },
                    Explanation = "Jupiter is the largest planet in our solar system.",
                    Reference = new Reference { Text = "NASA - Jupiter", Url = "https://example.com/jupiter" },
                    Image = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Multiple Choice Question 2
                new Question
                {
                    Id = "c001",
                    Text = "Which element has the chemical symbol 'O'?",
                    Type = "MultipleChoice",
                    Options = new List<Option>
                    {
                        new Option { Text = "Oxygen", IsCorrect = true, Image = null },
                        new Option { Text = "Gold", IsCorrect = false, Image = null },
                        new Option { Text = "Osmium", IsCorrect = false, Image = null },
                        new Option { Text = "Silver", IsCorrect = false, Image = null }
                    },
                    Tags = new List<string> { "chemistry", "elements", "periodic table" },
                    Explanation = "The chemical symbol 'O' stands for Oxygen.",
                    Reference = new Reference { Text = "Periodic Table", Url = "https://example.com/periodic_table" },
                    Image = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Multiple Choice Question 3
                new Question
                {
                    Id = "h001",
                    Text = "Who wrote 'To Kill a Mockingbird'?",
                    Type = "MultipleChoice",
                    Options = new List<Option>
                    {
                        new Option { Text = "Harper Lee", IsCorrect = true, Image = null },
                        new Option { Text = "Mark Twain", IsCorrect = false, Image = null },
                        new Option { Text = "J.K. Rowling", IsCorrect = false, Image = null },
                        new Option { Text = "Ernest Hemingway", IsCorrect = false, Image = null }
                    },
                    Tags = new List<string> { "literature", "books", "authors" },
                    Explanation = "'To Kill a Mockingbird' was written by Harper Lee.",
                    Reference = new Reference { Text = "Harper Lee Biography", Url = "https://example.com/harper_lee" },
                    Image = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Free Text Question 1
                new Question
                {
                    Id = "g002",
                    Text = "What is the capital of France?",
                    Type = "FreeText",
                    Options = new List<Option>(), // No options needed for free-text questions
                    CorrectAnswer = "Paris",
                    Tags = new List<string> { "geography", "capital cities", "France" },
                    Explanation = "The capital city of France is Paris.",
                    Reference = new Reference { Text = "Capital Cities of Europe", Url = "https://example.com/capital_cities_europe" },
                    Image = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Free Text Question 2
                new Question
                {
                    Id = "g003",
                    Text = "Which country is known as the Land of the Rising Sun?",
                    Type = "FreeText",
                    Options = new List<Option>(), // No options needed for free-text questions
                    CorrectAnswer = "Japan",
                    Tags = new List<string> { "geography", "country names", "Japan" },
                    Explanation = "Japan is often referred to as the Land of the Rising Sun.",
                    Reference = new Reference { Text = "Japan Overview", Url = "https://example.com/japan_overview" },
                    Image = null,
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
                    Title = "Sample Question Set",
                    Sequence = 1,
                    Description = "A set of basic trivia questions.",
                    Category = "mock",
                    Questions = new List<QuestionAccess>
                    {
                        new QuestionAccess { QuestionId = "g001", Access = "free" },
                        new QuestionAccess { QuestionId = "c001", Access = "free" },
                        new QuestionAccess { QuestionId = "g002", Access = "free" },
                        new QuestionAccess { QuestionId = "h001", Access = "free" },
                        new QuestionAccess { QuestionId = "g003", Access = "free" }
                    },
                    Access = "free",
                    CreatedBy = ObjectId.GenerateNewId().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                },
                new Test
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Geography",
                    Sequence = 1,
                    Description = "A set of basic geography questions.",
                    Category = "topic",
                    Questions = new List<QuestionAccess>
                    {
                        new QuestionAccess { QuestionId = "g001", Access = "free" },
                        new QuestionAccess { QuestionId = "g002", Access = "free" },
                        new QuestionAccess { QuestionId = "g003", Access = "free" }
                    },
                    Access = "free",
                    CreatedBy = ObjectId.GenerateNewId().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                }
            };
        }

        public static List<ReferenceMaterial> GetReferenceMaterials()
        {
            return new List<ReferenceMaterial>
            {
                new ReferenceMaterial
                {
                    Id = "toefl-article-001",
                    Title = "Examining the Problem of Bycatch",
                    Category = "toefl",
                    Content = new List<string>
                    {
                        "1. A topic of increasing relevance to the conservation of marine life is bycatch—fish and other animals that are unintentionally caught in the process of fishing for a targeted population of fish. Bycatch is a common occurrence in longline fishing, which utilizes a long heavy fishing line with baited hooks placed at intervals, and in trawling, which utilizes a fishing net (trawl) that is dragged along the ocean floor or through the mid-ocean waters. Few fisheries employ gear that can catch one species to the exclusion of all others. Dolphins, whales, and turtles are frequently captured in nets set for tunas and billfishes, and seabirds and turtles are caught in longline sets. Because bycatch often goes unreported, it is difficult to accurately estimate its extent. Available data indicate that discarded biomass (organic matter from living things) amounts to 25–30 percent of official catch, or about 30 million metric tons.",
                        "2. The bycatch problem is particularly acute when trawl nets with small mesh sizes (smaller-than-average holes in the net material) are dragged along the bottom of the ocean in pursuit of groundfish or shrimp. Because of the small mesh size of the shrimp trawl nets, most of the fishes captured are either juveniles (young), smaller than legal size limits, or undesirable small species. Even larger mesh sizes do not prevent bycatch because once the net begins to fill with fish or shrimp, small individuals caught subsequently are trapped without ever encountering the mesh. In any case, these incidental captures are unmarketable and are usually shoveled back over the side of the vessel dead or dying."
                    },
                    Tags = new List<string> { "reading", "marine-life", "fishing", "environment" },
                    CreatedAt = DateTime.Parse("2024-08-24T10:00:00Z"),
                    UpdatedAt = DateTime.Parse("2024-08-24T10:00:00Z")
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
                    CreatedAt = DateTime.UtcNow,
                    Message = new EmailMessage { Body = "Welcome to the mock test app!", Subject="Welcome", To= "email@address.com"},
                    Status = "Sent",
                    SentVia = "Brevo",
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
