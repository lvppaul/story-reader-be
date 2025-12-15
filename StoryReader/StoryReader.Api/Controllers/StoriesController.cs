using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryReader.Api.Extensions;
using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
using System.Security.Claims;

namespace StoryReader.Api.Controllers
{
    [ApiController]
    [Route("api/stories")]
    public class StoriesController : ControllerBase
    {
        private readonly IStoryService _service;

        public StoriesController(IStoryService service)
        {
            _service = service;
        }

        // ---------- CREATE ----------
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateStoryRequest request)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _service.CreateAsync(userId, request);
            return this.OkResponse(result);
        }

        // ---------- LIST ----------
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 10)
        {
            var result = await _service.GetAllAsync(page, pageSize);
            return this.OkResponse(result);
        }

        // ---------- DETAIL ----------
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetDetail(string slug)
        {
            var result = await _service.GetBySlugAsync(slug);
            return this.OkResponse(result);
        }
    }
}
