using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;
using Serilog;
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
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAuthenticationService _authenticationService;

        public UserService(IUserRepository userRepository,
            IUserStore userStore, 
            IEmailService emailService,
            IMapper mapper,
            IAuthenticationService authenticationService
            )
        {
            _userRepository = userRepository;
            _userStore = userStore;
            _mapper = mapper;
            _emailService = emailService;
            _authenticationService = authenticationService;
        }

        public async Task<LoginResponse> RegisterUserAsync(RegisterRequest registerDto)
        {
            var existingUser = await _userStore.GetByUsernameAsync(registerDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already exists. Please sign in.");

            var user = CreateUserObject(registerDto);

            await _userRepository.CreateAsync(user);
            await SendWelcomeEmailAsync(user.Email, user.DisplayName);

            return await _authenticationService.AuthenticateAsync(new LoginRequest
            {
                Username = registerDto.Email,
                Password = registerDto.Password
            });
        }

        public Task<User> GetUserByIdAsync(string id) =>
            _userRepository.GetByIdAsync(id);

        public Task<bool> UpdateUserAsync(User user) =>
            _userRepository.UpdateAsync(user);

        public async Task<bool> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            SetUserFields(user, updateUserDto);
            return await _userRepository.UpdateAsync(user);
        }

        private User CreateUserObject(RegisterRequest registerDto)
        {
            var salt = MyUtility.GetSalt();
            return new User
            {
                Email = registerDto.Email,
                DisplayName = registerDto.DisplayName,
                TimeZone = registerDto.TimeZone,
                PasswordSalt = salt,
                PasswordHash = MyUtility.GetHash(registerDto.Password + salt),
                Subscription = new Subscription { Plan = "free" },
                Metadata = new Metadata { CreationTime = DateTime.UtcNow },
                ProviderData = new List<ProviderData>
                {
                    new ProviderData
                    {
                        DisplayName = registerDto.DisplayName,
                        Email = registerDto.Email,
                        Uid = registerDto.Email,
                        ProviderId = "password"
                    }
                }
            };
        }

        private void SetUserFields(User user, UpdateUserDto updateUserDto)
        {
            if (!string.IsNullOrEmpty(updateUserDto.DisplayName))
                user.DisplayName = updateUserDto.DisplayName;

            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
                user.PhoneNumber = updateUserDto.PhoneNumber;

            if (!string.IsNullOrEmpty(updateUserDto.TimeZone))
                user.TimeZone = updateUserDto.TimeZone;
        }        

        private async Task SendWelcomeEmailAsync(string email, string displayName)
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(email, displayName);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to send welcome email: {ex}", ex);
            }
        }


    }
}
