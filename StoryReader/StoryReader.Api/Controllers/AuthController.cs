using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StoryReader.Api.Extensions;
using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
using LoginRequest = StoryReader.Application.DTOs.LoginRequest;
using RegisterRequest = StoryReader.Application.DTOs.RegisterRequest;
using StoryReader.Api.Extensions;
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

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return this.OkResponse(result);
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return this.OkResponse(result);
        }

        // ---------------- REFRESH TOKEN ----------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshAsync(request);
            return this.OkResponse(result);
        }
    }

}
