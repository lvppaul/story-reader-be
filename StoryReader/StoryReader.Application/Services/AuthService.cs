using StoryReader.Application.Common;
using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
using StoryReader.Domain.Entities;

namespace StoryReader.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthTokenStore _tokenStore;

        public AuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshTokenRepo,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher,
            IAuthTokenStore tokenStore)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
            _tokenStore = tokenStore;
        }

        // ---------------- REGISTER ----------------
        public async Task<AuthInternalResult> RegisterAsync(RegisterRequest request)
        {
            var normalizedEmail = request.Email.ToUpperInvariant();

            if (await _userRepo.ExistsByEmailAsync(normalizedEmail))
                throw AppException.Conflict(
                   "EMAIL_ALREADY_EXISTS",
                   "Email already exists");

            var passwordHash = _passwordHasher.Hash(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = passwordHash,
                DisplayName = request.DisplayName,
                Role = "user",
                IsActive = true,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);

            var refreshToken = CreateRefreshToken(user.Id);

            // DB (optional)
            await _refreshTokenRepo.AddAsync(refreshToken);

            //  Redis là source chính
            await _tokenStore.SaveAsync(
                refreshToken.Token,
                user.Id,
                TimeSpan.FromDays(7));

            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            return new AuthInternalResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        // ---------------- LOGIN ----------------
        public async Task<AuthInternalResult> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.ToUpperInvariant();
            var user = await _userRepo.GetByEmailAsync(normalizedEmail);

            if (user == null)
                throw AppException.Unauthorized(
                    "INVALID_CREDENTIALS",
                    "Email or password is incorrect");

            if (!user.IsActive)
                throw AppException.Forbidden(
                    "USER_INACTIVE",
                    "User account is inactive");

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw AppException.Unauthorized(
                     "INVALID_CREDENTIALS",
                     "Email or password is incorrect");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            // logout all devices
            //  DB revoke
          //  await _refreshTokenRepo.RevokeAllByUserAsync(user.Id);

            //  Redis revoke all
         //   await _tokenStore.RevokeAllAsync(user.Id);

            var refreshToken = CreateRefreshToken(user.Id);
            await _refreshTokenRepo.AddAsync(refreshToken);

            // Redis save
            await _tokenStore.SaveAsync(
           refreshToken.Token,
           user.Id,
           TimeSpan.FromDays(7));

            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            return new AuthInternalResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        // ---------------- REFRESH TOKEN ----------------
        public async Task<AuthInternalResult> RefreshAsync(string refreshToken)
        {
            //  Reuse attack
            if (await _tokenStore.IsBlacklistedAsync(refreshToken))
                throw AppException.Unauthorized(
                    "REFRESH_TOKEN_REUSE",
                    "Refresh token has been reused");
             
            //  Token hợp lệ?
            var userId = await _tokenStore.GetUserIdAsync(refreshToken);
            if (userId == null)
                throw AppException.Unauthorized(
                    "INVALID_REFRESH_TOKEN",
                    "Refresh token is invalid");

            var user = await _userRepo.GetByIdAsync(userId.Value);
            if (user == null || !user.IsActive)
                throw AppException.Forbidden(
                    "USER_INACTIVE",
                    "User account is inactive");

            //  Rotate token
            await _tokenStore.BlacklistAsync(
                refreshToken,
                TimeSpan.FromDays(7));

            await _tokenStore.RevokeAsync(refreshToken);

            // DB revoke (optional)
            var dbToken = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
            if (dbToken != null)
                await _refreshTokenRepo.RevokeAsync(dbToken.Id);

            var newRefreshToken = CreateRefreshToken(user.Id);

            await _refreshTokenRepo.AddAsync(newRefreshToken);

            //  Redis là source chính
            await _tokenStore.SaveAsync(
                newRefreshToken.Token,
                user.Id,
                TimeSpan.FromDays(7));

            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            return new AuthInternalResult
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
        }
        

        // ---------------- LOGOUT (ONE DEVICE) ----------------
        public async Task LogoutAsync(string refreshToken)
        {
            await _tokenStore.BlacklistAsync(
                refreshToken,
                TimeSpan.FromDays(7));

            await _tokenStore.RevokeAsync(refreshToken);

            var dbToken = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
            if (dbToken != null && !dbToken.IsRevoked)
                await _refreshTokenRepo.RevokeAsync(dbToken.Id);
        }

        // ---------------- LOGOUT ALL DEVICES ----------------
        public async Task LogoutAllAsync(Guid userId)
        {
            await _tokenStore.RevokeAllAsync(userId);
            await _refreshTokenRepo.RevokeAllByUserAsync(userId);
        }


        // ---------------- PRIVATE ----------------
        private static RefreshToken CreateRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }
    }
}