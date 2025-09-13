using Domain.DTOs;
using Domain.Models;
namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string GenerateJWTToken(User user);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    UserDTO ValidateToken(string token);
}
