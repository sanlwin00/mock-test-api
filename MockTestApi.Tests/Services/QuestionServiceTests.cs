using FluentAssertions;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services;
using Moq;

namespace MockTestApi.Tests.Services
{
    public class QuestionServiceTests
    {
        private readonly Mock<IQuestionRepository> _questionRepoMock;
        private readonly Mock<ITestRepository> _testRepoMock;
        private readonly QuestionService _service;

        public QuestionServiceTests()
        {
            _questionRepoMock = new Mock<IQuestionRepository>();
            _testRepoMock = new Mock<ITestRepository>();
            _service = new QuestionService(_questionRepoMock.Object, _testRepoMock.Object);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static Question MakeQuestion(string id, string enText, string frText = "")
        {
            return new Question
            {
                Id = id,
                Text = frText.Length > 0
                    ? new Dictionary<string, string> { ["en"] = enText, ["fr"] = frText }
                    : new Dictionary<string, string> { ["en"] = enText },
                Type = "MultipleChoice",
                Options = new List<Option>
                {
                    new Option
                    {
                        Text = new Dictionary<string, string> { ["en"] = "Option A", ["fr"] = "Option A (fr)" },
                        IsCorrect = true,
                        Image = null
                    }
                },
                CorrectAnswer = null,
                Tags = new List<string> { "history" },
                Explanation = frText.Length > 0
                    ? new Dictionary<string, string> { ["en"] = "Explanation", ["fr"] = "Explication" }
                    : new Dictionary<string, string> { ["en"] = "Explanation" },
                Reference = null,
                Image = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static Test MakeTest(string testId, params (string questionId, int sequence)[] questions)
        {
            return new Test
            {
                Id = testId,
                Title = "Test",
                Questions = questions
                    .Select(q => new QuestionAccess { QuestionId = q.questionId, Access = "free", Sequence = q.sequence })
                    .ToList()
            };
        }

        // ── GetAllQuestionsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetAllQuestionsAsync_ReturnsMappedDtos()
        {
            var questions = new List<Question> { MakeQuestion("q1", "What is Canada?") };
            _questionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(questions);

            var result = (await _service.GetAllQuestionsAsync()).ToList();

            result.Should().HaveCount(1);
            result[0].Id.Should().Be("q1");
            result[0].Text.Should().Be("What is Canada?");
        }

        [Fact]
        public async Task GetAllQuestionsAsync_WithNullLocale_TextLocalIsEmpty()
        {
            _questionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Question> { MakeQuestion("q1", "en text", "fr text") });

            var result = (await _service.GetAllQuestionsAsync(null)).ToList();

            result[0].TextLocal.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllQuestionsAsync_WithEnLocale_TextLocalIsEmpty()
        {
            _questionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Question> { MakeQuestion("q1", "en text", "fr text") });

            var result = (await _service.GetAllQuestionsAsync("en")).ToList();

            result[0].TextLocal.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllQuestionsAsync_WithFrLocale_TextLocalIsPopulated()
        {
            _questionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Question> { MakeQuestion("q1", "en text", "fr text") });

            var result = (await _service.GetAllQuestionsAsync("fr")).ToList();

            result[0].TextLocal.Should().Be("fr text");
            result[0].Options[0].TextLocal.Should().Be("Option A (fr)");
            result[0].ExplanationLocal.Should().Be("Explication");
        }

        [Fact]
        public async Task GetAllQuestionsAsync_WithMissingLocale_FallsBackToEnglish()
        {
            _questionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Question> { MakeQuestion("q1", "en text") }); // no fr key

            var result = (await _service.GetAllQuestionsAsync("fr")).ToList();

            result[0].TextLocal.Should().Be("en text");
        }

        // ── GetQuestionsByTestIdAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetQuestionsByTestIdAsync_TestNotFound_ReturnsEmpty()
        {
            _testRepoMock.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Test)null);

            var result = await _service.GetQuestionsByTestIdAsync("missing");

            result.Should().BeEmpty();
            _questionRepoMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task GetQuestionsByTestIdAsync_ValidTestId_ReturnsMappedDtos()
        {
            var test = MakeTest("t1", ("q1", 1), ("q2", 2));
            var questions = new List<Question>
            {
                MakeQuestion("q1", "Question 1"),
                MakeQuestion("q2", "Question 2"),
            };

            _testRepoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(test);
            _questionRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(questions);

            var result = (await _service.GetQuestionsByTestIdAsync("t1")).ToList();

            result.Should().HaveCount(2);
            result.Select(q => q.Id).Should().Contain(new[] { "q1", "q2" });
        }

        [Fact]
        public async Task GetQuestionsByTestIdAsync_ReturnsQuestionsInSequenceOrder()
        {
            // Test defines q2 first (sequence 1), q1 second (sequence 2)
            var test = MakeTest("t1", ("q2", 1), ("q1", 2));
            var questions = new List<Question>
            {
                MakeQuestion("q1", "Question 1"),
                MakeQuestion("q2", "Question 2"),
            };

            _testRepoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(test);
            _questionRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(questions);

            var result = (await _service.GetQuestionsByTestIdAsync("t1")).ToList();

            result[0].Id.Should().Be("q2");
            result[1].Id.Should().Be("q1");
        }

        [Fact]
        public async Task GetQuestionsByTestIdAsync_OnlyFetchesIdsFromTest()
        {
            var test = MakeTest("t1", ("q1", 1));
            _testRepoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(test);
            _questionRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Question> { MakeQuestion("q1", "Q1") });

            await _service.GetQuestionsByTestIdAsync("t1");

            _questionRepoMock.Verify(
                r => r.GetByIdsAsync(It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "q1" }))),
                Times.Once
            );
        }

        [Fact]
        public async Task GetQuestionsByTestIdAsync_WithFrLocale_TextLocalIsPopulated()
        {
            var test = MakeTest("t1", ("q1", 1));
            _testRepoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(test);
            _questionRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Question> { MakeQuestion("q1", "en text", "fr text") });

            var result = (await _service.GetQuestionsByTestIdAsync("t1", "fr")).ToList();

            result[0].TextLocal.Should().Be("fr text");
        }

        [Fact]
        public async Task GetQuestionsByTestIdAsync_WithEnLocale_TextLocalIsEmpty()
        {
            var test = MakeTest("t1", ("q1", 1));
            _testRepoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(test);
            _questionRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Question> { MakeQuestion("q1", "en text", "fr text") });

            var result = (await _service.GetQuestionsByTestIdAsync("t1", "en")).ToList();

            result[0].TextLocal.Should().BeEmpty();
        }
    }
}
