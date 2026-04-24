using MockTestApi.Models;
using MongoDB.Bson;

namespace MockTestApi.Tests.Helpers
{
    public static class TestDataGenerator
    {
        public static MockTest CreateValidMockTest(string id = null, string userId = "test-user")
        {
            return new MockTest
            {
                Id = id ?? ObjectId.GenerateNewId().ToString(),
                Title = "Test Mock Test",
                Description = "A test mock test for unit testing",
                TestId = ObjectId.GenerateNewId().ToString(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Questions = CreateValidQuestions(),
                Results = null
            };
        }

        public static List<MockTestQuestion> CreateValidQuestions(int count = 3)
        {
            var questions = new List<MockTestQuestion>();
            for (int i = 0; i < count; i++)
            {
                questions.Add(new MockTestQuestion
                {
                    QuestionId = ObjectId.GenerateNewId().ToString(),
                    SelectedOption = null,
                    UserAnswer = "",
                    ReviewLater = false,
                    Number = i + 1
                });
            }
            return questions;
        }

        public static MockTestResults CreateValidResults(int totalQuestions = 3, int correctAnswers = 2)
        {
            var score = (double)correctAnswers / totalQuestions * 100;
            return new MockTestResults
            {
                Score = score,
                Passed = score >= 75,
                CorrectAnswers = correctAnswers,
                TotalQuestions = totalQuestions,
                Details = CreateResultDetails(totalQuestions, correctAnswers)
            };
        }

        public static List<ResultDetail> CreateResultDetails(int totalQuestions, int correctAnswers)
        {
            var details = new List<ResultDetail>();
            for (int i = 0; i < totalQuestions; i++)
            {
                details.Add(new ResultDetail
                {
                    QuestionId = ObjectId.GenerateNewId().ToString(),
                    Correct = i < correctAnswers
                });
            }
            return details;
        }

        public static UpdateMockTestDto CreateValidUpdateDto(string questionId = null)
        {
            return new UpdateMockTestDto
            {
                QuestionId = questionId ?? ObjectId.GenerateNewId().ToString(),
                SelectedOption = 1,
                UserAnswer = "Test Answer",
                ReviewLater = false
            };
        }

        public static CompleteMockTestDto CreateValidCompleteDto(int questionCount = 3)
        {
            return new CompleteMockTestDto
            {
                Questions = CreateValidQuestions(questionCount),
                Results = CreateValidResults(questionCount, 2)
            };
        }
    }
}