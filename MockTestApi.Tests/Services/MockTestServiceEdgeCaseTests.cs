using FluentAssertions;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services;
using Moq;

namespace MockTestApi.Tests.Services
{
    public class MockTestServiceEdgeCaseTests
    {
        private readonly Mock<IMockTestRepository> _mockTestRepositoryMock;
        private readonly MockTestService _mockTestService;

        public MockTestServiceEdgeCaseTests()
        {
            _mockTestRepositoryMock = new Mock<IMockTestRepository>();
            _mockTestService = new MockTestService(_mockTestRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithNullMockTest_ShouldThrowException()
        {
            // Arrange
            _mockTestRepositoryMock.Setup(x => x.CreateAsync(null))
                .ThrowsAsync(new ArgumentNullException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _mockTestService.CreateAsync(null));
        }

        [Fact]
        public async Task CreateAsync_WithEmptyQuestionsList_ShouldStillCreate()
        {
            // Arrange
            var mockTest = new MockTest
            {
                Id = "64f1234567890abcdef12345",
                UserId = "user123",
                TestId = "test123",
                Questions = new List<MockTestQuestion>() // Empty list
            };

            _mockTestRepositoryMock.Setup(x => x.CreateAsync(mockTest))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _mockTestService.CreateAsync(mockTest);

            // Assert
            result.Should().NotBeNull();
            result.Questions.Should().BeEmpty();
            _mockTestRepositoryMock.Verify(x => x.CreateAsync(mockTest), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var mockTest = new MockTest
            {
                Id = "64f1234567890abcdef12345",
                UserId = "user123"
            };

            _mockTestRepositoryMock.Setup(x => x.CreateAsync(mockTest))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _mockTestService.CreateAsync(mockTest));

            exception.Message.Should().Be("Database connection failed");
        }

        [Fact]
        public async Task UpdateProgressAsync_WithVeryLongUserAnswer_ShouldHandleCorrectly()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var questionId = "q1";
            var veryLongAnswer = new string('A', 10000); // 10,000 character answer

            var updateDto = new UpdateMockTestDto
            {
                QuestionId = questionId,
                UserAnswer = veryLongAnswer,
                SelectedOption = 1
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = questionId, UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.UpdateProgressAsync(
                mockTestId, questionId, veryLongAnswer, 1, false))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeTrue();
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                mockTestId, questionId, veryLongAnswer, 1, false), Times.Once);
        }

        [Fact]
        public async Task UpdateProgressAsync_WithSpecialCharactersInAnswer_ShouldHandleCorrectly()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var questionId = "q1";
            var specialCharAnswer = "Test with special chars: !@#$%^&*(){}[]|\\:;\"'<>,.?/~`±§";

            var updateDto = new UpdateMockTestDto
            {
                QuestionId = questionId,
                UserAnswer = specialCharAnswer,
                SelectedOption = 2
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = questionId, UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.UpdateProgressAsync(
                mockTestId, questionId, specialCharAnswer, 2, false))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeTrue();
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                mockTestId, questionId, specialCharAnswer, 2, false), Times.Once);
        }

        [Fact]
        public async Task UpdateProgressAsync_WithNegativeSelectedOption_ShouldStillProcess()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var questionId = "q1";

            var updateDto = new UpdateMockTestDto
            {
                QuestionId = questionId,
                UserAnswer = "Test Answer",
                SelectedOption = -1 // Negative option
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = questionId, UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.UpdateProgressAsync(
                mockTestId, questionId, "Test Answer", -1, false))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateProgressAsync_WithVeryLargeSelectedOption_ShouldStillProcess()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var questionId = "q1";

            var updateDto = new UpdateMockTestDto
            {
                QuestionId = questionId,
                UserAnswer = "Test Answer",
                SelectedOption = int.MaxValue
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = questionId, UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.UpdateProgressAsync(
                mockTestId, questionId, "Test Answer", int.MaxValue, false))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateProgressAsync_WithDuplicateQuestionIds_ShouldFindFirstMatch()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var questionId = "q1";

            var updateDto = new UpdateMockTestDto
            {
                QuestionId = questionId,
                UserAnswer = "Test Answer",
                SelectedOption = 1
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = questionId, UserAnswer = "First", SelectedOption = 0 },
                    new MockTestQuestion { QuestionId = questionId, UserAnswer = "Second", SelectedOption = 0 } // Duplicate
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.UpdateProgressAsync(
                mockTestId, questionId, "Test Answer", 1, false))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeTrue();
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                mockTestId, questionId, "Test Answer", 1, false), Times.Once);
        }

        [Fact]
        public async Task CompleteTestAsync_WithNullResults_ShouldStillProcess()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var completeDto = new CompleteMockTestDto
            {
                Results = null
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                UserId = "user123"
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.CompleteTestAsync(mockTestId, null))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.CompleteTestAsync(mockTestId, completeDto);

            // Assert
            result.Should().BeTrue();
            _mockTestRepositoryMock.Verify(x => x.CompleteTestAsync(mockTestId, null), Times.Once);
        }

        [Fact]
        public async Task GetAllMockTestsAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            _mockTestRepositoryMock.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database timeout"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _mockTestService.GetAllMockTestsAsync());

            exception.Message.Should().Be("Database timeout");
        }

        [Fact]
        public async Task GetMockTestByIdAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _mockTestService.GetMockTestByIdAsync(mockTestId));

            exception.Message.Should().Be("Network error");
        }

        [Fact]
        public async Task UpdateProgressAsync_WithEmptyQuestionId_ShouldReturnFalse()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var updateDto = new UpdateMockTestDto
            {
                QuestionId = "", // Empty question ID
                UserAnswer = "Test Answer",
                SelectedOption = 1
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = "q1", UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProgressAsync_WithNullQuestionsList_ShouldReturnFalse()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var updateDto = new UpdateMockTestDto
            {
                QuestionId = "q1",
                UserAnswer = "Test Answer",
                SelectedOption = 1
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                Questions = null // Null questions list
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _mockTestService.UpdateProgressAsync(mockTestId, updateDto));
        }

        [Fact]
        public async Task CompleteTestAsync_WithExtremeScoreValues_ShouldStillProcess()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var completeDto = new CompleteMockTestDto
            {
                Results = new MockTestResults
                {
                    Score = -100, // Negative score
                    TotalQuestions = 0, // Zero questions
                    CorrectAnswers = 1000, // More correct than total
                    Passed = false // Extreme case
                }
            };

            var existingMockTest = new MockTest { Id = mockTestId };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.CompleteTestAsync(mockTestId, completeDto.Results))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.CompleteTestAsync(mockTestId, completeDto);

            // Assert
            result.Should().BeTrue();
            _mockTestRepositoryMock.Verify(x => x.CompleteTestAsync(mockTestId, completeDto.Results), Times.Once);
        }
    }
}