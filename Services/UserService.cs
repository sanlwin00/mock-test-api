using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MockTestApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetRepository;
        private readonly IUserStore _userStore;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        public UserService(IUserRepository userRepository, IConfiguration configuration, 
            IUserStore userStore, IMapper mapper, 
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _userStore = userStore;
            _mapper = mapper;
            _passwordResetRepository = passwordResetTokenRepository;
            _emailService = emailService;
        }
        public async Task<LoginResponse> AuthenticateAsync(LoginRequest loginRequest)
        {
            var user = await _userStore.GetByUsernameAsync(loginRequest.Username);
            if (user != null)
            {                
                string passwordHash = this.GetHash(loginRequest.Password + user.PasswordSalt);
                if (user.PasswordHash == passwordHash)
                {
                    UserDto userDto = _mapper.Map<UserDto>(user);

                    var loginResponse = new LoginResponse
                    {
                        Token = GenerateJwtToken(user),
                        User = userDto
                    };
                    return loginResponse;
                }
            }
            return null;
        }

        public async Task<LoginResponse> AuthenticateWithAccessCodeAsync(string accessCode)
        {
            var user = await _userStore.GetByAccessCodeAsync(accessCode);
            if (user != null)
            {
                UserDto userDto = _mapper.Map<UserDto>(user);

                var loginResponse = new LoginResponse
                {
                    Token = GenerateJwtToken(user),
                    User = userDto
                };
                return loginResponse;
            }
            return null;
        }

        public async Task<LoginResponse> RegisterUserAsync(RegisterRequest registerDto)
        {
            // Check if the email already exists
            var existingUser = await _userStore.GetByUsernameAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already exists. Please sign in.");
            }

            var salt = this.GetSalt();
            var user = new User
            {
                Email = registerDto.Email,
                DisplayName = registerDto.DisplayName,
                TimeZone = registerDto.TimeZone,
                PasswordSalt = salt,
                PasswordHash = this.GetHash(registerDto.Password + salt),
                Subscription = new Subscription { Plan = "free" },
                Metadata = new Metadata { CreationTime = DateTime.UtcNow },
                ProviderData = new List<ProviderData>([new ProviderData {
                    DisplayName = registerDto.DisplayName,
                    Email = registerDto.Email,
                    Uid = registerDto.Email,
                    ProviderId = "password"
                }])
            };
            await _userRepository.CreateAsync(user);

            var loginRequest = new LoginRequest { Username = registerDto.Email, Password = registerDto.Password };
            
            return await AuthenticateAsync(loginRequest);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = _mapper.Map<List<UserDto>>(users);

            return userDtos;
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            var userDto = _mapper.Map<UserDto>(user);
            
            return userDto;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // Update only the fields that are not null
            if (!string.IsNullOrEmpty(updateUserDto.DisplayName))
            {
                user.DisplayName = updateUserDto.DisplayName;
            }
            if (updateUserDto.PhoneNumber != null)
            {
                user.PhoneNumber = updateUserDto.PhoneNumber;
            }
            if (updateUserDto.TimeZone != null)
            {
                user.TimeZone = updateUserDto.TimeZone;
            }

            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task CreateUserAsync(User user)
        {
            await _userRepository.CreateAsync(user);
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await _userStore.GetByUsernameAsync(email);
            if (user == null)
            {
                return false; // User not found
            }

            var token = Guid.NewGuid().ToString();
            var expiryDate = DateTime.UtcNow.AddHours(24);

            // Save the reset token in the database
            var resetToken = new PasswordResetToken
            {
                Id = token,
                ExpiryDate = expiryDate,
                UserId = user.Id
            };

            await _passwordResetRepository.CreateAsync(resetToken);

            var resetLink = $"https://yourdomain.com/reset-password?token={token}";

            // Send the email (implement the IEmailSender interface)
            await _emailService.SendEmailAsync(user.Email, "Password Reset", $"Please reset your password by clicking <a href='{resetLink}'>here</a>.");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _passwordResetRepository.GetByIdAsync(token);
            if (resetToken == null || resetToken.ExpiryDate < DateTime.UtcNow)
            {
                return false; // Invalid or expired token
            }

            var user = await _userRepository.GetByIdAsync(resetToken.UserId);
            if (user == null)
            {
                return false; // User not found
            }

            // Hash the new password and update
            var salt = GetSalt();
            user.PasswordSalt = salt;
            user.PasswordHash = GetHash(newPassword + salt);

            await _userRepository.UpdateAsync(user);
            await _passwordResetRepository.DeleteAsync(token);

            return true;
        }


        private string GetHash(string text)
        {
            // SHA512 is disposable by inheritance.  
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                // Get the hashed string.  
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private string GetSalt()
        {
            byte[] bytes = new byte[128 / 8];
            using (var keyGenerator = RandomNumberGenerator.Create())
            {
                keyGenerator.GetBytes(bytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private string GenerateJwtToken(User user)
        {
            // Define standard claims
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Conditionally add the "Admin" role claim if the user is an admin
            if (user.CustomClaims?.Admin == true)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            // Create the token with a signing key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}
