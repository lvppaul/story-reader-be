using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StoryReader.Api.Extensions;
using StoryReader.Api.Extensions;
using StoryReader.Application.Common;
using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
using System.Security.Claims;
using LoginRequest = StoryReader.Application.DTOs.LoginRequest;
using RegisterRequest = StoryReader.Application.DTOs.RegisterRequest;
namespace StoryReader.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ================= PRIVATE =================
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // dev có thể false
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refresh_token", refreshToken, options);
        }

        private string GetRefreshTokenFromCookie()
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out var token))
                throw AppException.Unauthorized(
                    "REFRESH_TOKEN_MISSING",
                    "Refresh token is missing");

            return token;
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete("refresh_token");
        }


        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            SetRefreshTokenCookie(result.RefreshToken);

            return this.OkResponse(new AuthResultDto
            {
                AccessToken = result.AccessToken
            });
        }
        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            SetRefreshTokenCookie(result.RefreshToken);

            return this.OkResponse(new AuthResultDto
            {
                AccessToken = result.AccessToken
            });
        }

        // ---------------- REFRESH TOKEN ----------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = GetRefreshTokenFromCookie();

            var result = await _authService.RefreshAsync(refreshToken);

            SetRefreshTokenCookie(result.RefreshToken);

            return this.OkResponse(new AuthResultDto
            {
                AccessToken = result.AccessToken
            });
        }

        // ---------------- LOGOUT (ONE DEVICE) ----------------
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = GetRefreshTokenFromCookie();

            await _authService.LogoutAsync(refreshToken);

            ClearRefreshTokenCookie();

            return this.OkMessage("Logged out successfully");
        }

        // ---------------- LOGOUT ALL DEVICES ----------------
        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _authService.LogoutAllAsync(userId);

            ClearRefreshTokenCookie();

            return this.OkMessage("Logged out from all devices");
        }
    }

}
