using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using Xunit;

namespace TaskManagement.Tests.Services;

/// <summary>
/// Unit tests for TaskService
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _loggerMock = new Mock<ILogger<TaskService>>();
        _taskService = new TaskService(_taskRepositoryMock.Object, _loggerMock.Object);
    }

    #region GetTasksByUserIdAsync Tests

    [Fact]
    public async Task GetTasksByUserIdAsync_WhenTasksExist_ReturnsTaskDtos()
    {
        // Arrange
        var userId = 1;
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = 1,
                Title = "Task 1",
                Description = "Description 1",
                IsCompleted = false,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = 2,
                Title = "Task 2",
                Description = "Description 2",
                IsCompleted = true,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            }
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetAllByUserIdAsync(userId, null))
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetTasksByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<TaskDto>();
        
        var resultList = result.ToList();
        resultList[0].Id.Should().Be(1);
        resultList[0].Title.Should().Be("Task 1");
        resultList[1].Id.Should().Be(2);
        resultList[1].Title.Should().Be("Task 2");

        _taskRepositoryMock.Verify(repo => repo.GetAllByUserIdAsync(userId, null), Times.Once);
    }

    [Fact]
    public async Task GetTasksByUserIdAsync_WhenNoTasksExist_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        _taskRepositoryMock
            .Setup(repo => repo.GetAllByUserIdAsync(userId, null))
            .ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _taskService.GetTasksByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _taskRepositoryMock.Verify(repo => repo.GetAllByUserIdAsync(userId, null), Times.Once);
    }

    [Fact]
    public async Task GetTasksByUserIdAsync_WithIsCompletedFilter_ReturnsFilteredTasks()
    {
        // Arrange
        var userId = 1;
        var completedTasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = 1,
                Title = "Completed Task",
                IsCompleted = true,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            }
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetAllByUserIdAsync(userId, true))
            .ReturnsAsync(completedTasks);

        // Act
        var result = await _taskService.GetTasksByUserIdAsync(userId, isCompleted: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsCompleted.Should().BeTrue();
        _taskRepositoryMock.Verify(repo => repo.GetAllByUserIdAsync(userId, true), Times.Once);
    }

    #endregion

    #region GetTasksByUserIdPagedAsync Tests

    [Fact]
    public async Task GetTasksByUserIdPagedAsync_WhenTasksExist_ReturnsPaginatedResponse()
    {
        // Arrange
        var userId = 1;
        var page = 1;
        var pageSize = 5;
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1", UserId = userId, CreatedAt = DateTime.UtcNow },
            new TaskItem { Id = 2, Title = "Task 2", UserId = userId, CreatedAt = DateTime.UtcNow },
            new TaskItem { Id = 3, Title = "Task 3", UserId = userId, CreatedAt = DateTime.UtcNow }
        };
        var totalCount = 10;

        _taskRepositoryMock
            .Setup(repo => repo.GetAllByUserIdPagedAsync(userId, null, page, pageSize))
            .ReturnsAsync((tasks, totalCount));

        // Act
        var result = await _taskService.GetTasksByUserIdPagedAsync(userId, null, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(10);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        _taskRepositoryMock.Verify(repo => repo.GetAllByUserIdPagedAsync(userId, null, page, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetTasksByUserIdPagedAsync_WithInvalidPage_NormalizesToPageOne()
    {
        // Arrange
        var userId = 1;
        var invalidPage = 0;
        var pageSize = 5;
        var tasks = new List<TaskItem>();
        var totalCount = 0;

        _taskRepositoryMock
            .Setup(repo => repo.GetAllByUserIdPagedAsync(userId, null, 1, pageSize))
            .ReturnsAsync((tasks, totalCount));

        // Act
        var result = await _taskService.GetTasksByUserIdPagedAsync(userId, null, invalidPage, pageSize);

        // Assert
        result.Page.Should().Be(1);
        _taskRepositoryMock.Verify(repo => repo.GetAllByUserIdPagedAsync(userId, null, 1, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetTasksByUserIdPagedAsync_WithInvalidPageSize_NormalizesToDefault()
    {
        // Arrange
        var userId = 1;
        var page = 1;
        var invalidPageSize = 0;
        var tasks = new List<TaskItem>();
        var totalCount = 0;

        _taskRepositoryMock
            .Setup(repo => repo.GetAllByUserIdPagedAsync(userId, null, page, 5))
            .ReturnsAsync((tasks, totalCount));

        // Act
        var result = await _taskService.GetTasksByUserIdPagedAsync(userId, null, page, invalidPageSize);

        // Assert
        result.PageSize.Should().Be(5);
        _taskRepositoryMock.Verify(repo => repo.GetAllByUserIdPagedAsync(userId, null, page, 5), Times.Once);
    }

    #endregion

    #region GetTaskByIdAsync Tests

    [Fact]
    public async Task GetTaskByIdAsync_WhenTaskExists_ReturnsTaskDto()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            IsCompleted = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(taskId, userId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
        result.IsCompleted.Should().BeFalse();
        _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(taskId, userId), Times.Once);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WhenTaskDoesNotExist_ReturnsNull()
    {
        // Arrange
        var taskId = 999;
        var userId = 1;

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(taskId, userId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId, userId);

        // Assert
        result.Should().BeNull();
        _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(taskId, userId), Times.Once);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WhenTaskBelongsToDifferentUser_ReturnsNull()
    {
        // Arrange
        var taskId = 1;
        var userId = 2;

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(taskId, userId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId, userId);

        // Assert
        result.Should().BeNull();
        _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(taskId, userId), Times.Once);
    }

    #endregion

    #region CreateTaskAsync Tests

    [Fact]
    public async Task CreateTaskAsync_WhenValidRequest_CreatesTaskAndReturnsDto()
    {
        // Arrange
        var userId = 1;
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New Description"
        };

        var createdTask = new TaskItem
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock
            .Setup(repo => repo.AddAsync(It.Is<TaskItem>(t =>
                t.Title == request.Title &&
                t.Description == request.Description &&
                t.IsCompleted == false &&
                t.UserId == userId)))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTaskAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("New Task");
        result.Description.Should().Be("New Description");
        result.IsCompleted.Should().BeFalse();
        _taskRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_SetsIsCompletedToFalse()
    {
        // Arrange
        var userId = 1;
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New Description"
        };

        var createdTask = new TaskItem
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTaskAsync(request, userId);

        // Assert
        result.IsCompleted.Should().BeFalse();
        _taskRepositoryMock.Verify(repo => repo.AddAsync(It.Is<TaskItem>(t => t.IsCompleted == false)), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var userId = 1;
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description"
        };

        var createdTask = new TaskItem
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _taskRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTaskAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdTask.Id);
        result.Title.Should().Be(createdTask.Title);
        result.Description.Should().Be(createdTask.Description);
        result.IsCompleted.Should().Be(createdTask.IsCompleted);
        result.CreatedAt.Should().Be(createdTask.CreatedAt);
        result.UpdatedAt.Should().Be(createdTask.UpdatedAt);
    }

    #endregion

    #region UpdateTaskAsync Tests

    [Fact]
    public async Task UpdateTaskAsync_WhenTaskExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;
        var request = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Original Title",
            Description = "Original Description",
            IsCompleted = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(taskId, userId))
            .ReturnsAsync(existingTask);

        _taskRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, request, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.IsCompleted.Should().BeTrue();
        
        _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(taskId, userId), Times.Once);
        _taskRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<TaskItem>(t =>
            t.Id == taskId &&
            t.Title == request.Title &&
            t.Description == request.Description &&
            t.IsCompleted == request.IsCompleted)), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_WhenTaskDoesNotExist_ReturnsNull()
    {
        // Arrange
        var taskId = 999;
        var userId = 1;
        var request = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(taskId, userId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, request, userId);

        // Assert
        result.Should().BeNull();
        _taskRepositoryMock.Verify(repo => repo.GetByIdAsync(taskId, userId), Times.Once);
        _taskRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTaskAsync_WhenTaskBelongsToDifferentUser_ReturnsNull()
    {
        // Arrange
        var taskId = 1;
        var userId = 2;
        var request = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(taskId, userId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, request, userId);

        // Assert
        result.Should().BeNull();
        _taskRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    #endregion

    #region DeleteTaskAsync Tests

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskExists_ReturnsTrue()
    {
        // Arrange
        var taskId = 1;
        var userId = 1;

        _taskRepositoryMock
            .Setup(repo => repo.DeleteAsync(taskId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, userId);

        // Assert
        result.Should().BeTrue();
        _taskRepositoryMock.Verify(repo => repo.DeleteAsync(taskId, userId), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var taskId = 999;
        var userId = 1;

        _taskRepositoryMock
            .Setup(repo => repo.DeleteAsync(taskId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, userId);

        // Assert
        result.Should().BeFalse();
        _taskRepositoryMock.Verify(repo => repo.DeleteAsync(taskId, userId), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskBelongsToDifferentUser_ReturnsFalse()
    {
        // Arrange
        var taskId = 1;
        var userId = 2;

        _taskRepositoryMock
            .Setup(repo => repo.DeleteAsync(taskId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, userId);

        // Assert
        result.Should().BeFalse();
        _taskRepositoryMock.Verify(repo => repo.DeleteAsync(taskId, userId), Times.Once);
    }

    #endregion
}

