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
        private readonly IUserStore _userStore;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository, IConfiguration configuration, IUserStore userStore, IMapper mapper)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _userStore = userStore;
            _mapper = mapper;
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
                PasswordSalt = salt,
                PasswordHash = this.GetHash(registerDto.Password + salt),
                Subscription = new Subscription { Plan = "free" },
                Metadata = new Metadata { CreationTime = DateTime.Now },
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

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task CreateUserAsync(User user)
        {
            await _userRepository.CreateAsync(user);
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
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

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
