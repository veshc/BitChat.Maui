using Microsoft.Extensions.Logging;
using Moq;

namespace BitChat.Maui.Tests;

/// <summary>
/// Base class for all tests providing common utilities and setup
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    /// <typeparam name="T">The type to create a logger for</typeparam>
    /// <returns>A mock logger instance</returns>
    protected Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Creates a test ID for unique identification in tests
    /// </summary>
    /// <returns>A unique test ID string</returns>
    protected string CreateTestId()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    /// <summary>
    /// Creates a test nickname for testing purposes
    /// </summary>
    /// <param name="suffix">Optional suffix to append</param>
    /// <returns>A valid test nickname</returns>
    protected string CreateTestNickname(string suffix = "")
    {
        return $"TestUser{suffix}{CreateTestId()}";
    }

    /// <summary>
    /// Verifies that a mock logger was called with a specific log level
    /// </summary>
    /// <typeparam name="T">The logger type</typeparam>
    /// <param name="mockLogger">The mock logger</param>
    /// <param name="logLevel">The expected log level</param>
    /// <param name="times">The expected number of times (default: at least once)</param>
    protected void VerifyLoggerCalled<T>(Mock<ILogger<T>> mockLogger, LogLevel logLevel, Times? times = null)
    {
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times ?? Times.AtLeastOnce());
    }
}