using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Tests.Helpers;
using Xunit;

namespace TaskManagement.Tests.Repositories;

/// <summary>
/// Unit tests for UserRepository
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.PasswordHash.Should().Be("hashedpassword");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseSensitive_ReturnsCorrectUser()
    {
        // Arrange
        var user = new User
        {
            Email = "Test@Example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("Test@Example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("Test@Example.com");
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserExists_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "exists@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UserExistsAsync("exists@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.UserExistsAsync("nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenValidUser_AddsUserToDatabase()
    {
        // Arrange
        var user = new User
        {
            Email = "newuser@example.com",
            PasswordHash = "hashedpassword"
        };

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be("newuser@example.com");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var userInDb = await _context.Users.FindAsync(result.Id);
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task AddAsync_SetsCreatedAtTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var user = new User
        {
            Email = "timestamp@example.com",
            PasswordHash = "hashedpassword"
        };

        // Act
        var result = await _repository.AddAsync(user);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeAfter(beforeCreation);
        result.CreatedAt.Should().BeBefore(afterCreation);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Email = "getbyid@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("getbyid@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "user1@example.com", PasswordHash = "hash1", CreatedAt = DateTime.UtcNow },
            new User { Email = "user2@example.com", PasswordHash = "hash2", CreatedAt = DateTime.UtcNow },
            new User { Email = "user3@example.com", PasswordHash = "hash3", CreatedAt = DateTime.UtcNow }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Email == "user1@example.com");
        result.Should().Contain(u => u.Email == "user2@example.com");
        result.Should().Contain(u => u.Email == "user3@example.com");
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesUser()
    {
        // Arrange
        var user = new User
        {
            Email = "update@example.com",
            PasswordHash = "oldhash",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        user.PasswordHash = "newhash";

        // Act
        await _repository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.PasswordHash.Should().Be("newhash");
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_RemovesUser()
    {
        // Arrange
        var user = new User
        {
            Email = "delete@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        // Act
        await _repository.DeleteAsync(user);

        // Assert
        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserExists_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "existscheck@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(u => u.Email == "existscheck@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(u => u.Email == "nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}

