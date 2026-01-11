using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using Xunit;

namespace TaskManagement.Tests.Services;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Setup configuration for JWT settings
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong" },
            { "JwtSettings:Issuer", "TestIssuer" },
            { "JwtSettings:Audience", "TestAudience" },
            { "JwtSettings:ExpirationInMinutes", "60" }
        });
        _configuration = configurationBuilder.Build();

        // Setup configuration mock
        _configurationMock
            .Setup(c => c["JwtSettings:SecretKey"])
            .Returns("ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong");
        _configurationMock
            .Setup(c => c["JwtSettings:Issuer"])
            .Returns("TestIssuer");
        _configurationMock
            .Setup(c => c["JwtSettings:Audience"])
            .Returns("TestAudience");
        _configurationMock
            .Setup(c => c["JwtSettings:ExpirationInMinutes"])
            .Returns("60");
        _configurationMock
            .Setup(c => c.GetSection("JwtSettings"))
            .Returns(_configuration.GetSection("JwtSettings"));

        _authService = new AuthService(_userRepositoryMock.Object, _configuration, _loggerMock.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WhenUserDoesNotExist_CreatesUserAndReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!"
        };

        var createdUser = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(repo => repo.UserExistsAsync(request.Email))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.Token.Should().NotBeNullOrEmpty();

        // Verify JWT token is valid
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        token.Should().NotBeNull();
        // Check for both short and full claim type names
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.Email || c.Type == "email") && c.Value == request.Email);
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid") && c.Value == "1");

        _userRepositoryMock.Verify(repo => repo.UserExistsAsync(request.Email), Times.Once);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenUserAlreadyExists_ReturnsNull()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!"
        };

        _userRepositoryMock
            .Setup(repo => repo.UserExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(repo => repo.UserExistsAsync(request.Email), Times.Once);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_HashesPasswordBeforeStoring()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!"
        };

        var createdUser = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "", // Will be set by the repository
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(repo => repo.UserExistsAsync(request.Email))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                createdUser.PasswordHash = u.PasswordHash;
                return createdUser;
            });

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u =>
            !string.IsNullOrEmpty(u.PasswordHash) &&
            u.PasswordHash != request.Password)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserWithCorrectEmail()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "testuser@example.com",
            Password = "Password123!"
        };

        var createdUser = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(repo => repo.UserExistsAsync(request.Email))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u =>
            u.Email == request.Email)), Times.Once);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        // Hash the password to match what the service expects
        var passwordHash = HashPassword(request.Password);
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.Token.Should().NotBeNullOrEmpty();

        // Verify JWT token is valid
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        token.Should().NotBeNull();
        // Check for both short and full claim type names
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.Email || c.Type == "email") && c.Value == request.Email);
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid") && c.Value == "1");

        _userRepositoryMock.Verify(repo => repo.GetByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(repo => repo.GetByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidPassword_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = HashPassword("CorrectPassword"),
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(repo => repo.GetByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenCorrectPassword_VerifiesPasswordHash()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var passwordHash = HashPassword(request.Password);
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetUserIdFromToken Tests

    [Fact]
    public void GetUserIdFromToken_WhenValidToken_ReturnsUserId()
    {
        // Arrange
        var userId = 1;
        var email = "user@example.com";
        var token = GenerateTokenForUser(userId, email);

        // Act
        var result = _authService.GetUserIdFromToken(token);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WhenInvalidToken_ThrowsException()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act & Assert
        // The method can throw ArgumentException for invalid token format or UnauthorizedAccessException for validation failure
        var act = () => _authService.GetUserIdFromToken(invalidToken);
        act.Should().Throw<Exception>(); // Accept any exception for invalid token format
    }

    [Fact]
    public void GetUserIdFromToken_WhenTokenMissingUserIdClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        // Create a token without NameIdentifier claim
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong");
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "user@example.com")
                // Missing NameIdentifier claim
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Act & Assert
        var act = () => _authService.GetUserIdFromToken(tokenString);
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("Invalid token");
    }

    [Fact]
    public void GetUserIdFromToken_WhenTokenHasNonNumericUserId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        // Create a token with non-numeric NameIdentifier
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong");
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "not-a-number"),
                new Claim(ClaimTypes.Email, "user@example.com")
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Act & Assert
        var act = () => _authService.GetUserIdFromToken(tokenString);
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("Invalid token");
    }

    #endregion

    #region Helper Methods

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hashedBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashedBytes);
    }

    private string GenerateTokenForUser(int userId, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLong");
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion
}

