using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Domain.DTOs;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public string Create(User user)
    {
        return GenerateJWTToken(user);
    }

    public string GenerateJWTToken(User userInfo)
    {
        string _secretKey = configuration["Jwt:Secret"]!;
        int _expireTime = configuration.GetValue<int>("Jwt:ExpirationInMinutes");
        string _issuerToken = configuration["Jwt:Issuer"]!;
        string _audienceToken = configuration["Jwt:Audience"]!;

        if (string.IsNullOrEmpty(_secretKey))
            throw new ArgumentNullException("The secret Key or the Expire Time of the Token Generator have not been configured.");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Nombre_completo),
                new Claim(ClaimTypes.Role, GetRoleFromProfile(userInfo.Id_perfil)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
        var tokenDescritor = new JwtSecurityToken(
            issuer: _issuerToken,
            audience: _audienceToken,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expireTime),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescritor);
    }

    private static string GetRoleFromProfile(int idPerfil)
    {
        return idPerfil switch
        {
            1 => "Admin",
            2 => "Operator",
            _ => "User"
        };
    }

    public UserDTO ValidateToken(string token)
    {
        if (token == null)
            return null;

        string _secretKey = configuration["Jwt:Secret"]!;
        int _expireTime = configuration.GetValue<int>("Jwt:ExpirationInMinutes");
        string _issuerToken = configuration["Jwt:Issuer"]!;
        string _audienceToken = configuration["Jwt:Audience"]!;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value);

        // return user id from JWT token if validation successful
        return new UserDTO
        {
            //IdUser = int.Parse(jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value),
            //Idclient = int.Parse(jwtToken.Claims.First(x => x.Type == "client").Value),
            Role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value,
        };
    }
}
