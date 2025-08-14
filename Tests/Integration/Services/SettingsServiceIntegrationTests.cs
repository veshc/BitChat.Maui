using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace BitChat.Maui.Tests.Integration.Services;

/// <summary>
/// Integration tests for ISettingsService implementations focusing on contract compliance
/// and integration scenarios
/// </summary>
public class SettingsServiceIntegrationTests : TestBase
{
    #region Mock-based Contract Tests

    [Fact]
    public async Task GetUserIdentityAsync_WhenCalled_ShouldReturnNonNullIdentity()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var expectedIdentity = new UserIdentity { Nickname = CreateTestNickname() };
        mockService.Setup(s => s.GetUserIdentityAsync()).ReturnsAsync(expectedIdentity);

        // Act
        var result = await mockService.Object.GetUserIdentityAsync();

        // Assert
        result.Should().NotBeNull("GetUserIdentityAsync should always return a user identity");
        result.Should().Be(expectedIdentity, "should return the expected identity");
    }

    [Fact]
    public async Task SetUserIdentityAsync_WithValidIdentity_ShouldNotThrow()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var identity = UserIdentity.WithNickname(CreateTestNickname());
        mockService.Setup(s => s.SetUserIdentityAsync(It.IsAny<UserIdentity>())).Returns(Task.CompletedTask);

        // Act & Assert
        var action = async () => await mockService.Object.SetUserIdentityAsync(identity);
        await action.Should().NotThrowAsync("SetUserIdentityAsync should accept valid identities");
    }

    [Fact]
    public async Task SetUserIdentityAsync_WithNullIdentity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        mockService.Setup(s => s.SetUserIdentityAsync(null!))
            .ThrowsAsync(new ArgumentNullException("identity"));

        // Act & Assert
        var action = async () => await mockService.Object.SetUserIdentityAsync(null!);
        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("identity");
    }

    [Fact]
    public async Task GetNicknameAsync_WhenCalled_ShouldReturnValidString()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var expectedNickname = CreateTestNickname();
        mockService.Setup(s => s.GetNicknameAsync()).ReturnsAsync(expectedNickname);

        // Act
        var result = await mockService.Object.GetNicknameAsync();

        // Assert
        result.Should().NotBeNull("GetNicknameAsync should always return a string");
        result.Should().Be(expectedNickname, "should return the expected nickname");
    }

    [Fact]
    public async Task SetNicknameAsync_WithValidNickname_ShouldNotThrow()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var nickname = CreateTestNickname();
        mockService.Setup(s => s.SetNicknameAsync(nickname)).Returns(Task.CompletedTask);

        // Act & Assert
        var action = async () => await mockService.Object.SetNicknameAsync(nickname);
        await action.Should().NotThrowAsync("SetNicknameAsync should accept valid nicknames");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Invalid@Nickname")]
    [InlineData("TooLongNicknameExceedingMaximumLength")]
    public async Task SetNicknameAsync_WithInvalidNickname_ShouldThrowArgumentException(string? nickname)
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        mockService.Setup(s => s.SetNicknameAsync(It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Invalid nickname"));

        // Act & Assert
        var action = async () => await mockService.Object.SetNicknameAsync(nickname ?? string.Empty);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ClearAllSettingsAsync_WhenCalled_ShouldNotThrow()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        mockService.Setup(s => s.ClearAllSettingsAsync()).Returns(Task.CompletedTask);

        // Act & Assert
        var action = async () => await mockService.Object.ClearAllSettingsAsync();
        await action.Should().NotThrowAsync("ClearAllSettingsAsync should complete successfully");
    }

    #endregion

    #region Event Handling Tests

    [Fact]
    public void UserIdentityChanged_Event_ShouldBeDefinedCorrectly()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var eventRaised = false;
        UserIdentity? capturedIdentity = null;

        // Act
        mockService.Object.UserIdentityChanged += (sender, identity) =>
        {
            eventRaised = true;
            capturedIdentity = identity;
        };

        // Simulate event raising
        var testIdentity = UserIdentity.WithNickname(CreateTestNickname());
        mockService.Raise(s => s.UserIdentityChanged += null, mockService.Object, testIdentity);

        // Assert
        eventRaised.Should().BeTrue("UserIdentityChanged event should be raised");
        capturedIdentity.Should().Be(testIdentity, "event should pass the correct identity");
    }

    [Fact]
    public void UserIdentityChanged_EventHandler_ShouldBeNullableWithoutException()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();

        // Act & Assert - This should not throw
        var action = () =>
        {
            mockService.Object.UserIdentityChanged += null;
            mockService.Object.UserIdentityChanged -= null;
        };
        
        action.Should().NotThrow("event handlers should accept null without throwing");
    }

    #endregion

    #region Service Lifecycle Tests

    [Fact]
    public async Task ServiceLifecycle_FullWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var nickname1 = CreateTestNickname("1");
        var nickname2 = CreateTestNickname("2");
        var identity1 = UserIdentity.WithNickname(nickname1);
        var identity2 = UserIdentity.WithNickname(nickname2);

        // Setup the mock to track state changes
        var currentNickname = "Anonymous";
        var currentIdentity = new UserIdentity { Nickname = currentNickname };

        mockService.Setup(s => s.GetNicknameAsync()).ReturnsAsync(() => currentNickname);
        mockService.Setup(s => s.GetUserIdentityAsync()).ReturnsAsync(() => currentIdentity);
        
        mockService.Setup(s => s.SetNicknameAsync(It.IsAny<string>()))
            .Callback<string>(n => 
            {
                currentNickname = n;
                currentIdentity = new UserIdentity { Nickname = n };
            })
            .Returns(Task.CompletedTask);

        mockService.Setup(s => s.SetUserIdentityAsync(It.IsAny<UserIdentity>()))
            .Callback<UserIdentity>(i => 
            {
                currentIdentity = i;
                currentNickname = i.Nickname;
            })
            .Returns(Task.CompletedTask);

        mockService.Setup(s => s.ClearAllSettingsAsync())
            .Callback(() =>
            {
                currentNickname = "Anonymous";
                currentIdentity = new UserIdentity { Nickname = "Anonymous" };
            })
            .Returns(Task.CompletedTask);

        // Act & Assert - Initial state
        var initialIdentity = await mockService.Object.GetUserIdentityAsync();
        initialIdentity.Nickname.Should().Be("Anonymous", "should start with default nickname");

        // Act & Assert - Set nickname
        await mockService.Object.SetNicknameAsync(nickname1);
        var updatedNickname = await mockService.Object.GetNicknameAsync();
        updatedNickname.Should().Be(nickname1, "nickname should be updated");

        // Act & Assert - Set identity
        await mockService.Object.SetUserIdentityAsync(identity2);
        var updatedIdentity = await mockService.Object.GetUserIdentityAsync();
        updatedIdentity.Nickname.Should().Be(nickname2, "identity should be updated");

        // Act & Assert - Clear settings
        await mockService.Object.ClearAllSettingsAsync();
        var clearedIdentity = await mockService.Object.GetUserIdentityAsync();
        clearedIdentity.Nickname.Should().Be("Anonymous", "settings should be cleared");
    }

    #endregion

    #region Error Handling and Edge Cases

    [Fact]
    public async Task Service_UnderHighLoad_ShouldHandleConcurrentOperations()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var testNickname = CreateTestNickname();
        mockService.Setup(s => s.GetNicknameAsync()).ReturnsAsync(testNickname);
        mockService.Setup(s => s.SetNicknameAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act - Simulate concurrent operations
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var nickname = CreateTestNickname(i.ToString());
            tasks.Add(mockService.Object.SetNicknameAsync(nickname));
            tasks.Add(mockService.Object.GetNicknameAsync());
        }

        // Assert
        var action = async () => await Task.WhenAll(tasks);
        await action.Should().NotThrowAsync("service should handle concurrent operations");
    }

    [Fact]
    public async Task Service_WithInvalidOperations_ShouldProvideDescriptiveErrors()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        mockService.Setup(s => s.SetNicknameAsync("Invalid@Nickname"))
            .ThrowsAsync(new ArgumentException("Invalid nickname: Nickname can only contain letters, numbers, spaces, dots, dashes, and underscores"));

        // Act & Assert
        var action = async () => await mockService.Object.SetNicknameAsync("Invalid@Nickname");
        var exception = await action.Should().ThrowAsync<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid nickname:", "error message should be descriptive");
        exception.Which.Message.Should().Contain("can only contain", "error message should explain the validation rule");
    }

    #endregion

    #region Performance and Resource Tests

    [Fact]
    public async Task Service_RepeatedOperations_ShouldBeEfficient()
    {
        // Arrange
        var mockService = new Mock<ISettingsService>();
        var testNickname = CreateTestNickname();
        mockService.Setup(s => s.GetNicknameAsync()).ReturnsAsync(testNickname);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Perform 1000 operations
        var tasks = new List<Task<string>>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(mockService.Object.GetNicknameAsync());
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
            "1000 async operations should complete quickly with proper mocking");
        
        tasks.Should().AllSatisfy(t => t.Result.Should().Be(testNickname), 
            "all operations should return consistent results");
    }

    #endregion

    #region Interface Compliance Tests

    [Fact]
    public void ISettingsService_Interface_ShouldHaveAllRequiredMembers()
    {
        // Act
        var interfaceType = typeof(ISettingsService);
        var methods = interfaceType.GetMethods();
        var events = interfaceType.GetEvents();

        // Assert
        methods.Should().Contain(m => m.Name == nameof(ISettingsService.GetUserIdentityAsync) && 
            m.ReturnType == typeof(Task<UserIdentity>), "should have GetUserIdentityAsync method");
        
        methods.Should().Contain(m => m.Name == nameof(ISettingsService.SetUserIdentityAsync) && 
            m.GetParameters().Length == 1 && 
            m.GetParameters()[0].ParameterType == typeof(UserIdentity), "should have SetUserIdentityAsync method");
        
        methods.Should().Contain(m => m.Name == nameof(ISettingsService.GetNicknameAsync) && 
            m.ReturnType == typeof(Task<string>), "should have GetNicknameAsync method");
        
        methods.Should().Contain(m => m.Name == nameof(ISettingsService.SetNicknameAsync) && 
            m.GetParameters().Length == 1 && 
            m.GetParameters()[0].ParameterType == typeof(string), "should have SetNicknameAsync method");
        
        methods.Should().Contain(m => m.Name == nameof(ISettingsService.ClearAllSettingsAsync) && 
            m.ReturnType == typeof(Task), "should have ClearAllSettingsAsync method");

        events.Should().Contain(e => e.Name == nameof(ISettingsService.UserIdentityChanged) && 
            e.EventHandlerType == typeof(EventHandler<UserIdentity>), "should have UserIdentityChanged event");
    }

    #endregion
}