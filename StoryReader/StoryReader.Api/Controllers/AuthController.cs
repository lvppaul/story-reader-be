using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
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

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<ActionResult<AuthResultDto>> Register(
            [FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDto>> Login(
            [FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        // ---------------- REFRESH TOKEN ----------------
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResultDto>> Refresh(
            [FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshAsync(request);
            return Ok(result);
        }
    }

}
