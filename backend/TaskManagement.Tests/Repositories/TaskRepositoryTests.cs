using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Tests.Helpers;
using Xunit;

namespace TaskManagement.Tests.Repositories;

/// <summary>
/// Unit tests for TaskRepository
/// </summary>
public class TaskRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TaskRepository _repository;
    private readonly User _testUser;

    public TaskRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new TaskRepository(_context);

        // Create a test user
        _testUser = new User
        {
            Email = "testuser@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WhenTasksExist_ReturnsAllTasksForUser()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Title = "Task 1", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Task 2", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Task 3", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow }
        };
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(t => t.UserId == _testUser.Id);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WhenNoTasksExist_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllByUserIdAsync(_testUser.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WithIsCompletedFilter_ReturnsOnlyCompletedTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Title = "Completed Task", IsCompleted = true, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Active Task", IsCompleted = false, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow }
        };
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserIdAsync(_testUser.Id, isCompleted: true);

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(t => t.IsCompleted == true);
        result.First().Title.Should().Be("Completed Task");
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WithIsCompletedFalseFilter_ReturnsOnlyActiveTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Title = "Completed Task", IsCompleted = true, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Active Task", IsCompleted = false, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow }
        };
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserIdAsync(_testUser.Id, isCompleted: false);

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(t => t.IsCompleted == false);
        result.First().Title.Should().Be("Active Task");
    }

    [Fact]
    public async Task GetAllByUserIdAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Title = "First Task", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new TaskItem { Title = "Second Task", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new TaskItem { Title = "Third Task", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow }
        };
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllByUserIdAsync(_testUser.Id)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Third Task");
        result[1].Title.Should().Be("Second Task");
        result[2].Title.Should().Be("First Task");
    }

    [Fact]
    public async Task GetAllByUserIdPagedAsync_ReturnsCorrectPage()
    {
        // Arrange
        var tasks = new List<TaskItem>();
        for (int i = 1; i <= 10; i++)
        {
            tasks.Add(new TaskItem
            {
                Title = $"Task {i}",
                UserId = _testUser.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserIdPagedAsync(_testUser.Id, page: 2, pageSize: 3);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(10);
        result.Items.Should().OnlyContain(t => t.UserId == _testUser.Id);
    }

    [Fact]
    public async Task GetAllByUserIdPagedAsync_WithFilter_ReturnsFilteredResults()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Title = "Completed 1", IsCompleted = true, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Completed 2", IsCompleted = true, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Active 1", IsCompleted = false, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow }
        };
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByUserIdPagedAsync(_testUser.Id, isCompleted: true, page: 1, pageSize: 5);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(t => t.IsCompleted == true);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskExists_ReturnsTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Test Task",
            Description = "Test Description",
            UserId = _testUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(task.Id, _testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999, _testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskBelongsToDifferentUser_ReturnsNull()
    {
        // Arrange
        var otherUser = new User
        {
            Email = "other@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(otherUser);
        await _context.SaveChangesAsync();

        var task = new TaskItem
        {
            Title = "Other User Task",
            UserId = otherUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(task.Id, _testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WhenValidTask_AddsTaskToDatabase()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "New Task",
            Description = "New Description",
            UserId = _testUser.Id
        };

        // Act
        var result = await _repository.AddAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("New Task");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var taskInDb = await _context.Tasks.FindAsync(result.Id);
        taskInDb.Should().NotBeNull();
        taskInDb!.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task AddAsync_SetsCreatedAtTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var task = new TaskItem
        {
            Title = "Timestamp Task",
            UserId = _testUser.Id
        };

        // Act
        var result = await _repository.AddAsync(task);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeAfter(beforeCreation);
        result.CreatedAt.Should().BeBefore(afterCreation);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_UpdatesTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Original Title",
            Description = "Original Description",
            UserId = _testUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        task.Title = "Updated Title";
        task.Description = "Updated Description";
        task.IsCompleted = true;

        // Act
        await _repository.UpdateAsync(task);

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.Title.Should().Be("Updated Title");
        updatedTask.Description.Should().Be("Updated Description");
        updatedTask.IsCompleted.Should().BeTrue();
        updatedTask.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Update Timestamp Task",
            UserId = _testUser.Id,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;
        task.Title = "Updated";

        // Act
        await _repository.UpdateAsync(task);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        updatedTask!.UpdatedAt.Should().NotBeNull();
        updatedTask.UpdatedAt!.Value.Should().BeAfter(beforeUpdate);
        updatedTask.UpdatedAt.Value.Should().BeBefore(afterUpdate);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskExists_RemovesTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Delete Task",
            UserId = _testUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        var taskId = task.Id;

        // Act
        var result = await _repository.DeleteAsync(taskId, _testUser.Id);

        // Assert
        result.Should().BeTrue();
        var deletedTask = await _context.Tasks.FindAsync(taskId);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999, _testUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskBelongsToDifferentUser_ReturnsFalse()
    {
        // Arrange
        var otherUser = new User
        {
            Email = "other2@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(otherUser);
        await _context.SaveChangesAsync();

        var task = new TaskItem
        {
            Title = "Other User Task",
            UserId = otherUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(task.Id, _testUser.Id);

        // Assert
        result.Should().BeFalse();
        var taskInDb = await _context.Tasks.FindAsync(task.Id);
        taskInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Title = "Task 1", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Task 2", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Task 3", UserId = _testUser.Id, CreatedAt = DateTime.UtcNow }
        };
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ReturnsMatchingTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Title = "Find Me", IsCompleted = true, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Don't Find", IsCompleted = false, UserId = _testUser.Id, CreatedAt = DateTime.UtcNow }
        };
        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(t => t.IsCompleted == true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Find Me");
    }

    [Fact]
    public async Task ExistsAsync_WhenTaskExists_ReturnsTrue()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Exists Check",
            UserId = _testUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(t => t.Title == "Exists Check");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenTaskDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(t => t.Title == "Non Existent");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}

