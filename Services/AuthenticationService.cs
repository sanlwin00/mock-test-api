using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MockTestApi.Services
{
    public class AuthenticationService: IAuthenticationService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public AuthenticationService(IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            INotificationService emailService,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings)
        {
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest loginRequest)
        {
            var user = await userRepository.GetByUsernameAsync(loginRequest.Username);
            if (user == null || !VerifyPassword(user, loginRequest.Password))
                return null;

            return GenerateLoginResponse(user);
        }

        public async Task<LoginResponse> AuthenticateWithAccessCodeAsync(string accessCode)
        {
            var user = await userRepository.GetByAccessCodeAsync(accessCode);
            return user == null ? null : GenerateLoginResponse(user);
        }

        private bool VerifyPassword(User user, string password) =>
            user.PasswordHash == MyUtility.GetHash(password + user.PasswordSalt);

        private LoginResponse GenerateLoginResponse(User user) =>
            new()
            {
                Token = GenerateJwtToken(user),
                User = _mapper.Map<UserDto>(user)
            };

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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays);

            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
