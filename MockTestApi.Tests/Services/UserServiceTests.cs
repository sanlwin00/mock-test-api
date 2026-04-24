using AutoMapper;
using FluentAssertions;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Services.Interfaces;
using Moq;

namespace MockTestApi.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _notificationServiceMock = new Mock<INotificationService>();
            _authenticationServiceMock = new Mock<IAuthenticationService>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _notificationServiceMock.Object,
                _mapperMock.Object,
                _authenticationServiceMock.Object
            );
        }

        [Fact]
        public async Task RegisterUserAsync_WithValidData_ShouldReturnLoginResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                DisplayName = "Test User",
                TimeZone = "UTC"
            };

            var expectedLoginResponse = new LoginResponse
            {
                Token = "test-token",
                User = new UserDto
                {
                    Email = "test@example.com",
                    DisplayName = "Test User"
                }
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(expectedLoginResponse);

            // Act
            var result = await _userService.RegisterUserAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("test-token");
            result.User.Email.Should().Be("test@example.com");
            result.User.DisplayName.Should().Be("Test User");

            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(registerRequest.Email), Times.Once);
            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _notificationServiceMock.Verify(x => x.SendWelcomeEmailAsync(registerRequest.Email, registerRequest.DisplayName), Times.Once);
            _authenticationServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "password123",
                DisplayName = "Test User",
                TimeZone = "UTC"
            };

            var existingUser = new User
            {
                Email = "existing@example.com"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.RegisterUserAsync(registerRequest));

            exception.Message.Should().Be("Email already exists. Please sign in.");
            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(registerRequest.Email), Times.Once);
            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = "64f1234567890abcdef12345";
            var expectedUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                DisplayName = "Test User"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.Email.Should().Be("test@example.com");
            result.DisplayName.Should().Be("Test User");

            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var userId = "invalid-id";

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithUser_ShouldReturnRepositoryResult()
        {
            // Arrange
            var user = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "test@example.com",
                DisplayName = "Updated User"
            };

            _userRepositoryMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithDto_WhenUserExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var userId = "64f1234567890abcdef12345";
            var updateDto = new UpdateUserDto
            {
                DisplayName = "Updated Name",
                PhoneNumber = "+1234567890",
                TimeZone = "EST"
            };

            var existingUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                DisplayName = "Original Name",
                PhoneNumber = "",
                TimeZone = "UTC"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().BeTrue();
            existingUser.DisplayName.Should().Be("Updated Name");
            existingUser.PhoneNumber.Should().Be("+1234567890");
            existingUser.TimeZone.Should().Be("EST");

            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithDto_WhenUserNotExists_ShouldReturnFalse()
        {
            // Arrange
            var userId = "non-existent-id";
            var updateDto = new UpdateUserDto
            {
                DisplayName = "Updated Name"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_WithPartialDto_ShouldOnlyUpdateProvidedFields()
        {
            // Arrange
            var userId = "64f1234567890abcdef12345";
            var updateDto = new UpdateUserDto
            {
                DisplayName = "Updated Name",
                PhoneNumber = null, // Should not update
                TimeZone = "" // Should not update
            };

            var existingUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                DisplayName = "Original Name",
                PhoneNumber = "Original Phone",
                TimeZone = "Original TimeZone"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().BeTrue();
            existingUser.DisplayName.Should().Be("Updated Name");
            existingUser.PhoneNumber.Should().Be("Original Phone"); // Should remain unchanged
            existingUser.TimeZone.Should().Be("Original TimeZone"); // Should remain unchanged

            _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_WhenEmailServiceFails_ShouldStillCompleteRegistration()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                DisplayName = "Test User",
                TimeZone = "UTC"
            };

            var expectedLoginResponse = new LoginResponse
            {
                Token = "test-token",
                User = new UserDto { Email = "test@example.com" }
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service failed"));

            _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(expectedLoginResponse);

            // Act
            var result = await _userService.RegisterUserAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("test-token");

            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _authenticationServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()), Times.Once);
        }
    }
}