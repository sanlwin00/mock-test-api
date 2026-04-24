using FluentAssertions;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services;
using Moq;

namespace MockTestApi.Tests.Services
{
    public class MockTestServiceTests
    {
        private readonly Mock<IMockTestRepository> _mockTestRepositoryMock;
        private readonly MockTestService _mockTestService;

        public MockTestServiceTests()
        {
            _mockTestRepositoryMock = new Mock<IMockTestRepository>();
            _mockTestService = new MockTestService(_mockTestRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithValidMockTest_ShouldCreateAndReturnMockTest()
        {
            // Arrange
            var mockTest = new MockTest
            {
                Id = "64f1234567890abcdef12345",
                UserId = "user123",
                TestId = "test123",
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = "q1", UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.CreateAsync(mockTest))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _mockTestService.CreateAsync(mockTest);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(mockTest);
            _mockTestRepositoryMock.Verify(x => x.CreateAsync(mockTest), Times.Once);
        }

        [Fact]
        public async Task UpdateProgressAsync_WithValidData_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var questionId = "q1";
            var updateDto = new UpdateMockTestDto
            {
                QuestionId = questionId,
                UserAnswer = "Test Answer",
                SelectedOption = 2,
                ReviewLater = true
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                UserId = "user123",
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion
                    {
                        QuestionId = questionId,
                        UserAnswer = "",
                        SelectedOption = 0,
                        ReviewLater = false
                    }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.UpdateProgressAsync(
                mockTestId, questionId, updateDto.UserAnswer, updateDto.SelectedOption, updateDto.ReviewLater))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeTrue();
            _mockTestRepositoryMock.Verify(x => x.GetByIdAsync(mockTestId), Times.Once);
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                mockTestId, questionId, updateDto.UserAnswer, updateDto.SelectedOption, updateDto.ReviewLater), Times.Once);
        }

        [Fact]
        public async Task UpdateProgressAsync_WithNonExistentMockTest_ShouldReturnFalse()
        {
            // Arrange
            var mockTestId = "non-existent-id";
            var updateDto = new UpdateMockTestDto
            {
                QuestionId = "q1",
                UserAnswer = "Test Answer",
                SelectedOption = 1
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync((MockTest)null);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockTestRepositoryMock.Verify(x => x.GetByIdAsync(mockTestId), Times.Once);
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProgressAsync_WithNonExistentQuestion_ShouldReturnFalse()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var nonExistentQuestionId = "non-existent-question";
            var updateDto = new UpdateMockTestDto
            {
                QuestionId = nonExistentQuestionId,
                UserAnswer = "Test Answer",
                SelectedOption = 1
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                UserId = "user123",
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = "q1", UserAnswer = "", SelectedOption = 0 },
                    new MockTestQuestion { QuestionId = "q2", UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockTestRepositoryMock.Verify(x => x.GetByIdAsync(mockTestId), Times.Once);
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CompleteTestAsync_WithValidData_ShouldCompleteAndReturnTrue()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var completeDto = new CompleteMockTestDto
            {
                Results = new MockTestResults
                {
                    Score = 85,
                    TotalQuestions = 10,
                    CorrectAnswers = 8,
                    Passed = true
                }
            };

            var existingMockTest = new MockTest
            {
                Id = mockTestId,
                UserId = "user123"
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.CompleteTestAsync(mockTestId, completeDto.Results))
                .ReturnsAsync(true);

            // Act
            var result = await _mockTestService.CompleteTestAsync(mockTestId, completeDto);

            // Assert
            result.Should().BeTrue();
            _mockTestRepositoryMock.Verify(x => x.GetByIdAsync(mockTestId), Times.Once);
            _mockTestRepositoryMock.Verify(x => x.CompleteTestAsync(mockTestId, completeDto.Results), Times.Once);
        }

        [Fact]
        public async Task CompleteTestAsync_WithNonExistentMockTest_ShouldReturnFalse()
        {
            // Arrange
            var mockTestId = "non-existent-id";
            var completeDto = new CompleteMockTestDto
            {
                Results = new MockTestResults
                {
                    Score = 85,
                    TotalQuestions = 10,
                    CorrectAnswers = 8,
                    Passed = true
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync((MockTest)null);

            // Act
            var result = await _mockTestService.CompleteTestAsync(mockTestId, completeDto);

            // Assert
            result.Should().BeFalse();
            _mockTestRepositoryMock.Verify(x => x.GetByIdAsync(mockTestId), Times.Once);
            _mockTestRepositoryMock.Verify(x => x.CompleteTestAsync(
                It.IsAny<string>(), It.IsAny<MockTestResults>()), Times.Never);
        }

        [Fact]
        public async Task GetAllMockTestsAsync_ShouldReturnAllMockTests()
        {
            // Arrange
            var expectedMockTests = new List<MockTest>
            {
                new MockTest { Id = "1", UserId = "user1" },
                new MockTest { Id = "2", UserId = "user2" }
            };

            _mockTestRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(expectedMockTests);

            // Act
            var result = await _mockTestService.GetAllMockTestsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedMockTests);
            _mockTestRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllMockTestsAsync_WhenNoMockTests_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyMockTests = new List<MockTest>();

            _mockTestRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(emptyMockTests);

            // Act
            var result = await _mockTestService.GetAllMockTestsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockTestRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMockTestByIdAsync_WithValidId_ShouldReturnMockTest()
        {
            // Arrange
            var mockTestId = "64f1234567890abcdef12345";
            var expectedMockTest = new MockTest
            {
                Id = mockTestId,
                UserId = "user123",
                TestId = "test123"
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(expectedMockTest);

            // Act
            var result = await _mockTestService.GetMockTestByIdAsync(mockTestId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedMockTest);
            _mockTestRepositoryMock.Verify(x => x.GetByIdAsync(mockTestId), Times.Once);
        }

        [Fact]
        public async Task GetMockTestByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var mockTestId = "invalid-id";

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync((MockTest)null);

            // Act
            var result = await _mockTestService.GetMockTestByIdAsync(mockTestId);

            // Assert
            result.Should().BeNull();
            _mockTestRepositoryMock.Verify(x => x.GetByIdAsync(mockTestId), Times.Once);
        }

        [Fact]
        public async Task UpdateProgressAsync_WithRepositoryFailure_ShouldReturnFalse()
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
                UserId = "user123",
                Questions = new List<MockTestQuestion>
                {
                    new MockTestQuestion { QuestionId = questionId, UserAnswer = "", SelectedOption = 0 }
                }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByIdAsync(mockTestId))
                .ReturnsAsync(existingMockTest);

            _mockTestRepositoryMock.Setup(x => x.UpdateProgressAsync(
                mockTestId, questionId, updateDto.UserAnswer, updateDto.SelectedOption, updateDto.ReviewLater))
                .ReturnsAsync(false);

            // Act
            var result = await _mockTestService.UpdateProgressAsync(mockTestId, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockTestRepositoryMock.Verify(x => x.UpdateProgressAsync(
                mockTestId, questionId, updateDto.UserAnswer, updateDto.SelectedOption, updateDto.ReviewLater), Times.Once);
        }

        [Fact]
        public async Task GetMockTestsByUserIdAsync_WithValidUserId_ShouldReturnOnlyThatUsersTests()
        {
            // Arrange
            var userId = "user123";
            var expectedTests = new List<MockTest>
            {
                new MockTest { Id = "mt1", UserId = userId, TestId = "test1" },
                new MockTest { Id = "mt2", UserId = userId, TestId = "test2" }
            };

            _mockTestRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedTests);

            // Act
            var result = await _mockTestService.GetMockTestsByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedTests);
            result.Should().OnlyContain(mt => mt.UserId == userId);
            _mockTestRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetMockTestsByUserIdAsync_WhenUserHasNoTests_ShouldReturnEmptyCollection()
        {
            // Arrange
            var userId = "user-with-no-tests";

            _mockTestRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(new List<MockTest>());

            // Act
            var result = await _mockTestService.GetMockTestsByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockTestRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetMockTestsByUserIdAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var userId = "user123";

            _mockTestRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _mockTestService.GetMockTestsByUserIdAsync(userId));

            exception.Message.Should().Be("Database connection failed");
        }

        [Fact]
        public async Task GetMockTestsByUserIdAsync_ShouldNotReturnOtherUsersTests()
        {
            // Arrange
            var userId = "user123";
            var otherUsersTests = new List<MockTest>
            {
                new MockTest { Id = "mt1", UserId = "other-user", TestId = "test1" }
            };

            // Repository filters by userId — returns empty for this user
            _mockTestRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(new List<MockTest>());

            // Act
            var result = await _mockTestService.GetMockTestsByUserIdAsync(userId);

            // Assert
            result.Should().BeEmpty();
            _mockTestRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
            _mockTestRepositoryMock.Verify(x => x.GetByUserIdAsync("other-user"), Times.Never);
        }
    }
}