using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Services.Interfaces;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace MockTestApi.Tests.Services
{
    public class AuthenticationServiceEdgeCaseTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly AuthenticationService _authenticationService;
        private readonly JwtSettings _jwtSettings;

        public AuthenticationServiceEdgeCaseTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordResetTokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _mapperMock = new Mock<IMapper>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();

            _jwtSettings = new JwtSettings
            {
                Key = "ThisIsATestKeyThatIs256BitsLong!!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpireDays = 7
            };

            _jwtSettingsMock.Setup(x => x.Value).Returns(_jwtSettings);

            _authenticationService = new AuthenticationService(
                _userRepositoryMock.Object,
                _passwordResetTokenRepositoryMock.Object,
                _notificationServiceMock.Object,
                _mapperMock.Object,
                _jwtSettingsMock.Object
            );
        }

        [Fact]
        public async Task AuthenticateAsync_WithNullLoginRequest_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _authenticationService.AuthenticateAsync(null));
        }

        [Fact]
        public async Task AuthenticateAsync_WithNullUsername_ShouldHandleGracefully()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = null!,
                Password = "password123"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(null))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(null), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithEmptyUsername_ShouldReturnNull()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "",
                Password = "password123"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(""))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WithNullPassword_ShouldReturnNull()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = null!
            };

            var user = new User
            {
                Id = "user123",
                Email = "test@example.com",
                PasswordSalt = "salt",
                PasswordHash = "hash"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WithVeryLongPassword_ShouldHandleCorrectly()
        {
            // Arrange
            var veryLongPassword = new string('a', 10000); // 10,000 character password
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = veryLongPassword
            };

            var user = new User
            {
                Id = "user123",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "incorrecthash"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull(); // Should fail due to incorrect hash
        }

        [Fact]
        public async Task AuthenticateAsync_WithSpecialCharactersInPassword_ShouldHandleCorrectly()
        {
            // Arrange
            var specialPassword = "P@ssw0rd!@#$%^&*(){}[]|\\:;\"'<>,.?/~`±§";
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = specialPassword
            };

            var user = new User
            {
                Id = "user123",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull(); // Should fail due to password mismatch
        }

        [Fact]
        public async Task AuthenticateWithAccessCodeAsync_WithNullAccessCode_ShouldReturnNull()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByAccessCodeAsync(null))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateWithAccessCodeAsync(null);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.GetByAccessCodeAsync(null), Times.Once);
        }

        [Fact]
        public async Task AuthenticateWithAccessCodeAsync_WithEmptyAccessCode_ShouldReturnNull()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByAccessCodeAsync(""))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateWithAccessCodeAsync("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateWithAccessCodeAsync_WithVeryLongAccessCode_ShouldHandleCorrectly()
        {
            // Arrange
            var longAccessCode = new string('A', 1000);
            _userRepositoryMock.Setup(x => x.GetByAccessCodeAsync(longAccessCode))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateWithAccessCodeAsync(longAccessCode);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authenticationService.AuthenticateAsync(loginRequest));

            exception.Message.Should().Be("Database connection failed");
        }

        [Fact]
        public async Task AuthenticateAsync_WhenMapperThrowsException_ShouldPropagateException()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = "user123",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map<UserDto>(user))
                .Throws(new Exception("Mapping failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authenticationService.AuthenticateAsync(loginRequest));

            exception.Message.Should().Be("Mapping failed");
        }

        [Fact]
        public async Task AuthenticateAsync_WithUserHavingNullId_ShouldThrowArgumentException()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = null!, // Null ID
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _authenticationService.AuthenticateAsync(loginRequest));

            exception.Message.Should().Contain("User ID cannot be null or empty");
        }

        [Fact]
        public async Task AuthenticateAsync_WithUserHavingNullEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = "user123",
                Email = null!, // Null email
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _authenticationService.AuthenticateAsync(loginRequest));

            exception.Message.Should().Contain("User email cannot be null or empty");
        }

        [Fact]
        public void AuthenticationService_WithInvalidJwtSettings_ShouldThrowExceptionOnConstruction()
        {
            // Arrange
            var invalidJwtSettings = new JwtSettings
            {
                Key = "short", // Too short key
                Issuer = null,
                Audience = null,
                ExpireDays = -1 // Negative expiry
            };

            _jwtSettingsMock.Setup(x => x.Value).Returns(invalidJwtSettings);

            // Act & Assert
            // This should throw an exception during construction due to invalid JWT settings
            Assert.Throws<ArgumentException>(
                () => new AuthenticationService(
                    _userRepositoryMock.Object,
                    _passwordResetTokenRepositoryMock.Object,
                    _notificationServiceMock.Object,
                    _mapperMock.Object,
                    _jwtSettingsMock.Object
                ));
        }

        [Fact]
        public async Task AuthenticateAsync_WithZeroExpireDays_ShouldCreateExpiredToken()
        {
            // Arrange
            var zeroExpireSettings = new JwtSettings
            {
                Key = "ThisIsATestKeyThatIs256BitsLong!!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpireDays = 0 // Zero expiry days
            };

            _jwtSettingsMock.Setup(x => x.Value).Returns(zeroExpireSettings);

            var authService = new AuthenticationService(
                _userRepositoryMock.Object,
                _passwordResetTokenRepositoryMock.Object,
                _notificationServiceMock.Object,
                _mapperMock.Object,
                _jwtSettingsMock.Object
            );

            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = "user123",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test@example.com"))
                .ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map<UserDto>(user))
                .Returns(new UserDto());

            // Act
            var result = await authService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();

            // Verify token expiry
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(result.Token);
            jsonToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
    }
}