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
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly AuthenticationService _authenticationService;
        private readonly JwtSettings _jwtSettings;

        public AuthenticationServiceTests()
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
        public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnLoginResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7" // hex hash of "password123testsalt"
            };

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map<UserDto>(user))
                .Returns(userDto);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
            result.User.Email.Should().Be(user.Email);

            // Verify JWT token structure
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(result.Token);
            jsonToken.Subject.Should().Be(user.Id);
            jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value.Should().Be(user.Email);

            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(loginRequest.Username), Times.Once);
            _mapperMock.Verify(x => x.Map<UserDto>(user), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidUsername_ShouldReturnNull()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistent@example.com",
                Password = "password123"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(loginRequest.Username), Times.Once);
            _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7" // hex hash of "password123testsalt"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(loginRequest.Username), Times.Once);
            _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_WithAdminUser_ShouldIncludeAdminRole()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "admin@example.com",
                Password = "password123"
            };

            var adminUser = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "admin@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7",
                CustomClaims = new CustomClaims { Admin = true }
            };

            var userDto = new UserDto
            {
                Id = adminUser.Id,
                Email = adminUser.Email
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(adminUser);

            _mapperMock.Setup(x => x.Map<UserDto>(adminUser))
                .Returns(userDto);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();

            // Verify JWT token contains admin role
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(result.Token);
            var roleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            roleClaim.Should().NotBeNull();
            roleClaim!.Value.Should().Be("Admin");
        }

        [Fact]
        public async Task AuthenticateWithAccessCodeAsync_WithValidAccessCode_ShouldReturnLoginResponse()
        {
            // Arrange
            var accessCode = "TEST123";
            var user = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "test@example.com",
                Subscription = new Subscription { AccessCode = accessCode }
            };

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email
            };

            _userRepositoryMock.Setup(x => x.GetByAccessCodeAsync(accessCode))
                .ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map<UserDto>(user))
                .Returns(userDto);

            // Act
            var result = await _authenticationService.AuthenticateWithAccessCodeAsync(accessCode);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
            result.User.Email.Should().Be(user.Email);

            _userRepositoryMock.Verify(x => x.GetByAccessCodeAsync(accessCode), Times.Once);
            _mapperMock.Verify(x => x.Map<UserDto>(user), Times.Once);
        }

        [Fact]
        public async Task AuthenticateWithAccessCodeAsync_WithInvalidAccessCode_ShouldReturnNull()
        {
            // Arrange
            var accessCode = "INVALID123";

            _userRepositoryMock?.Setup(x => x.GetByAccessCodeAsync(accessCode))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateWithAccessCodeAsync(accessCode);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock?.Verify(x => x.GetByAccessCodeAsync(accessCode), Times.Once);
            _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_JwtTokenShouldHaveCorrectClaims()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7"
            };

            var userDto = new UserDto { Id = user.Id, Email = user.Email };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map<UserDto>(user))
                .Returns(userDto);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(result.Token);

            // Verify standard claims
            jsonToken.Subject.Should().Be(user.Id);
            jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value.Should().Be(user.Email);
            jsonToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value.Should().NotBeNullOrEmpty();

            // Verify token properties
            jsonToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jsonToken.Audiences.Should().Contain(_jwtSettings.Audience);
            jsonToken.ValidTo.Should().BeAfter(DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays - 1));
        }

        [Fact]
        public async Task AuthenticateAsync_WithNullPasswordSalt_ShouldHandleGracefully()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "test@example.com",
                PasswordSalt = null!,
                PasswordHash = "somehash"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull(); // Should fail authentication due to password mismatch
            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(loginRequest.Username), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithEmptyPassword_ShouldReturnNull()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = ""
            };

            var user = new User
            {
                Id = "64f1234567890abcdef12345",
                Email = "test@example.com",
                PasswordSalt = "testsalt",
                PasswordHash = "d65e51795992e91e5553467d2474bdff0a1f3dcb2a3ca56fadc7125238a4aba7"
            };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateAsync(loginRequest);

            // Assert
            result.Should().BeNull();
        }
    }
}