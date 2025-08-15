using Xunit;

namespace BitChat.Maui.Tests.UI;

/// <summary>
/// Test collection definition for UI tests to ensure proper isolation
/// UI tests should not run in parallel to avoid driver conflicts
/// </summary>
[CollectionDefinition("UI Tests", DisableParallelization = true)]
public class UITestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}