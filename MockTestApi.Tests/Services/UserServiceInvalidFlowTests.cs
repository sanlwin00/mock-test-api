using AutoMapper;
using FluentAssertions;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Services.Interfaces;
using Moq;

namespace MockTestApi.Tests.Services
{
    public class UserServiceInvalidFlowTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly UserService _userService;

        public UserServiceInvalidFlowTests()
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
        public async Task RegisterUserAsync_WithNullRequest_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _userService.RegisterUserAsync(null));
        }

        [Fact]
        public async Task RegisterUserAsync_WithInvalidEmail_ShouldStillProcess()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "invalid-email-format",
                Password = "password123",
                DisplayName = "Test User",
                TimeZone = "UTC"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResponse { Token = "test-token" });

            // Act
            var result = await _userService.RegisterUserAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_WithEmptyPassword_ShouldStillCreateUser()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "", // Empty password
                DisplayName = "Test User",
                TimeZone = "UTC"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResponse { Token = "test-token" });

            // Act
            var result = await _userService.RegisterUserAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_WhenRepositoryCreateFails_ShouldThrowException()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                DisplayName = "Test User",
                TimeZone = "UTC"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _userService.RegisterUserAsync(registerRequest));

            exception.Message.Should().Be("Database connection failed");
            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _notificationServiceMock.Verify(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_WhenAuthenticationFails_ShouldThrowException()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                DisplayName = "Test User",
                TimeZone = "UTC"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
                .ThrowsAsync(new Exception("Authentication service failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _userService.RegisterUserAsync(registerRequest));

            exception.Message.Should().Be("Authentication service failed");
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNullId_ShouldReturnNull()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(null))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(null);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.GetByIdAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithEmptyId_ShouldReturnNull()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(""))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync("");

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.GetByIdAsync(""), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithNullUser_ShouldHandleGracefully()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.UpdateAsync(null))
                .ThrowsAsync(new ArgumentNullException());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _userService.UpdateUserAsync(null));
        }

        [Fact]
        public async Task UpdateUserAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var user = new User { Id = "test-id", Email = "test@example.com" };

            _userRepositoryMock.Setup(x => x.UpdateAsync(user))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _userService.UpdateUserAsync(user));

            exception.Message.Should().Be("Database error");
        }

        [Fact]
        public async Task UpdateUserAsync_WithDto_WhenRepositoryFailsAfterRetrieval_ShouldThrowException()
        {
            // Arrange
            var userId = "64f1234567890abcdef12345";
            var updateDto = new UpdateUserDto { DisplayName = "Updated Name" };

            var existingUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                DisplayName = "Original Name"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Update failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _userService.UpdateUserAsync(userId, updateDto));

            exception.Message.Should().Be("Update failed");
            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithDto_WhenRepositoryGetFails_ShouldThrowException()
        {
            // Arrange
            var userId = "64f1234567890abcdef12345";
            var updateDto = new UpdateUserDto { DisplayName = "Updated Name" };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ThrowsAsync(new Exception("Database connection lost"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _userService.UpdateUserAsync(userId, updateDto));

            exception.Message.Should().Be("Database connection lost");
            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_WithNullDisplayName_ShouldStillCreateUser()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                DisplayName = null, // Null display name
                TimeZone = "UTC"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(new LoginResponse { Token = "test-token" });

            // Act
            var result = await _userService.RegisterUserAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.DisplayName == null)), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithAllNullFields_ShouldNotChangeUser()
        {
            // Arrange
            var userId = "64f1234567890abcdef12345";
            var updateDto = new UpdateUserDto
            {
                DisplayName = null,
                PhoneNumber = null,
                TimeZone = null
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
            existingUser.DisplayName.Should().Be("Original Name");
            existingUser.PhoneNumber.Should().Be("Original Phone");
            existingUser.TimeZone.Should().Be("Original TimeZone");
        }
    }
}