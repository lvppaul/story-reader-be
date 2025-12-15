using StoryReader.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterRequest request);
        Task<AuthResultDto> LoginAsync(LoginRequest request);
        Task<AuthResultDto> RefreshAsync(RefreshTokenRequest request);

        Task LogoutAsync(string refreshToken);
        Task LogoutAllAsync(Guid userId);
    }

}
