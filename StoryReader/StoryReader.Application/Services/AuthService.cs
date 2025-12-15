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

        public AuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshTokenRepo,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
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
                IsActive = true,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);

            var refreshToken = CreateRefreshToken(user.Id);
            await _refreshTokenRepo.AddAsync(refreshToken);

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
            await _refreshTokenRepo.RevokeAllByUserAsync(user.Id);

            var refreshToken = CreateRefreshToken(user.Id);
            await _refreshTokenRepo.AddAsync(refreshToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            return new AuthInternalResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        // ---------------- REFRESH TOKEN ----------------
        public async Task<AuthInternalResult> RefreshAsync( string refreshToken)
        {
            var storedToken = await _refreshTokenRepo.GetByTokenAsync(refreshToken);

            if (storedToken == null)
                throw AppException.Unauthorized(
                    "INVALID_REFRESH_TOKEN",
                    "Refresh token is invalid");

            if (storedToken.IsExpired)
                throw AppException.Unauthorized(
                    "REFRESH_TOKEN_EXPIRED",
                    "Refresh token has expired");

            if (storedToken.IsRevoked)
                throw AppException.Unauthorized(
                    "REFRESH_TOKEN_REVOKED",
                    "Refresh token has been revoked");

            var user = await _userRepo.GetByIdAsync(storedToken.UserId);
            if (user == null)
                throw AppException.NotFound(
                    "USER_NOT_FOUND",
                    "User not found");

            if (!user.IsActive)
                throw AppException.Forbidden(
                    "USER_INACTIVE",
                    "User account is inactive");

            // revoke old refresh token
            await _refreshTokenRepo.RevokeAsync(storedToken.Id);

            var newRefreshToken = CreateRefreshToken(user.Id);
            await _refreshTokenRepo.AddAsync(newRefreshToken);

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
            var storedToken = await _refreshTokenRepo.GetByTokenAsync(refreshToken);

            if (storedToken == null)
                return; // idempotent (logout nhiều lần cũng OK)

            if (!storedToken.IsRevoked)
                await _refreshTokenRepo.RevokeAsync(storedToken.Id);
        }

        // ---------------- LOGOUT ALL DEVICES ----------------
        public async Task LogoutAllAsync(Guid userId)
        {
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