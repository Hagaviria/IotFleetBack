using Domain.DTOs;
using Domain.Models;
namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);
    string GenerateJWTToken(User user);
    UserDTO ValidateToken(string token);
}
