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
        Task<AuthInternalResult> RegisterAsync(RegisterRequest request);
        Task<AuthInternalResult> LoginAsync(LoginRequest request);
        Task<AuthInternalResult> RefreshAsync(string refreshToken);

        Task LogoutAsync(string refreshToken);
        Task LogoutAllAsync(Guid userId);
    }

}
