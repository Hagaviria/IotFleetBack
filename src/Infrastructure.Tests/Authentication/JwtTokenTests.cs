using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Infrastructure.Tests.Authentication;

public class JwtTokenTests
{
    private readonly string _secretKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456789";
    private readonly string _issuer = "IotFleet";
    private readonly string _audience = "IotFleetUsers";
    private readonly int _expirationMinutes = 60;

    [Fact]
    public void GenerateJwtToken_WithValidUser_ShouldCreateValidToken()
    {
        // Arrange
        var userInfo = new
        {
            Nombre_completo = "Test User",
            Id_perfil = 1
        };

        // Act
        var token = GenerateJWTToken(userInfo);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        jsonToken.Should().NotBeNull();
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "Test User");
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Theory]
    [InlineData(1, "Admin")]
    [InlineData(2, "Operator")]
    [InlineData(3, "User")]
    [InlineData(999, "User")] // Perfil no reconocido
    public void GenerateJwtToken_WithDifferentProfiles_ShouldReturnCorrectRole(int profileId, string expectedRole)
    {
        // Arrange
        var userInfo = new
        {
            Nombre_completo = "Test User",
            Id_perfil = profileId
        };

        // Act
        var token = GenerateJWTToken(userInfo);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        var roleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be(expectedRole);
    }

    [Fact]
    public void ValidateJwtToken_WithValidToken_ShouldReturnValidClaims()
    {
        // Arrange
        var userInfo = new
        {
            Nombre_completo = "Test User",
            Id_perfil = 1
        };

        var token = GenerateJWTToken(userInfo);

        // Act
        var isValid = ValidateToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateJwtToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var isValid = ValidateToken(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateJwtToken_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var userInfo = new
        {
            Nombre_completo = "Test User",
            Id_perfil = 1
        };

        // Generate token with very short expiration
        var token = GenerateJWTTokenWithExpiration(userInfo, -1); // Expired

        // Act
        var isValid = ValidateToken(token);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GenerateJwtToken_WithSpecialCharactersInUserName_ShouldHandleCorrectly()
    {
        // Arrange
        var userInfo = new
        {
            Nombre_completo = "José María O'Connor-Smith",
            Id_perfil = 2
        };

        // Act
        var token = GenerateJWTToken(userInfo);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        var subjectClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        subjectClaim.Should().NotBeNull();
        subjectClaim!.Value.Should().Be("José María O'Connor-Smith");
    }

    [Fact]
    public void GenerateJwtToken_WithNullSecretKey_ShouldThrowException()
    {
        // Arrange
        var userInfo = new
        {
            Nombre_completo = "Test User",
            Id_perfil = 1
        };

        // Act & Assert
        var action = () => GenerateJWTTokenWithSecret(userInfo, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateJwtToken_WithEmptySecretKey_ShouldThrowException()
    {
        // Arrange
        var userInfo = new
        {
            Nombre_completo = "Test User",
            Id_perfil = 1
        };

        // Act & Assert
        var action = () => GenerateJWTTokenWithSecret(userInfo, "");
        action.Should().Throw<ArgumentNullException>();
    }

    private string GenerateJWTToken(dynamic userInfo)
    {
        return GenerateJWTTokenWithSecret(userInfo, _secretKey);
    }

    private string GenerateJWTTokenWithSecret(dynamic userInfo, string secretKey)
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userInfo.Nombre_completo),
            new Claim(ClaimTypes.Role, GetRoleFromProfile(userInfo.Id_perfil)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private string GenerateJWTTokenWithExpiration(dynamic userInfo, int expirationMinutes)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userInfo.Nombre_completo),
            new Claim(ClaimTypes.Role, GetRoleFromProfile(userInfo.Id_perfil)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private bool ValidateToken(string token)
    {
        if (token == null)
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
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
}
