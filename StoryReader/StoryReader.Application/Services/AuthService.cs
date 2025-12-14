using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
using StoryReader.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<AuthResultDto> RegisterAsync(RegisterRequest request)
        {
            var normalizedEmail = request.Email.ToUpperInvariant();

            if (await _userRepo.ExistsByEmailAsync(normalizedEmail))
                throw new ApplicationException("Email already exists");

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

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        // ---------------- LOGIN ----------------
        public async Task<AuthResultDto> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.ToUpperInvariant();
            var user = await _userRepo.GetByEmailAsync(normalizedEmail);

            if (user == null || !user.IsActive)
                throw new ApplicationException("Invalid credentials");

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new ApplicationException("Invalid credentials");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            // logout all devices (refresh token rotation)
            await _refreshTokenRepo.RevokeAllByUserAsync(user.Id);

            var refreshToken = CreateRefreshToken(user.Id);
            await _refreshTokenRepo.AddAsync(refreshToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        // ---------------- REFRESH TOKEN ----------------
        public async Task<AuthResultDto> RefreshAsync(RefreshTokenRequest request)
        {
            var storedToken = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken);

            if (storedToken == null || storedToken.IsExpired || storedToken.IsRevoked)
                throw new ApplicationException("Invalid refresh token");

            var user = await _userRepo.GetByIdAsync(storedToken.UserId);

            if (user == null || !user.IsActive)
                throw new ApplicationException("User not found");

            // revoke old refresh token
            await _refreshTokenRepo.RevokeAsync(storedToken.Id);

            var newRefreshToken = CreateRefreshToken(user.Id);
            await _refreshTokenRepo.AddAsync(newRefreshToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
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
