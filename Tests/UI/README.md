# BitChat MAUI UI Testing Documentation

## Overview

This directory contains comprehensive UI tests for the BitChat MAUI application using Appium WebDriver for cross-platform mobile testing. The tests verify the complete user workflows for US-7.1.1 Public Chat View functionality.

## Test Structure

### Test Categories

1. **iOS Tests** (`ChatUITests.cs`) - 12 test methods
   - Core chat interface functionality on iOS platform
   - iOS-specific behaviors and UI patterns

2. **Android Tests** (`ChatUITests.Android.cs`) - 8 test methods  
   - Android-specific functionality and behaviors
   - Platform-specific gestures and interactions

3. **Shared Infrastructure** (`Shared/`)
   - `AppiumTestBase.cs` - Base class with cross-platform driver setup
   - `ChatPageObject.cs` - Page Object Model for chat interface

## Test Coverage

### Core Chat Functionality Tests
- ✅ Chat interface loading and element presence
- ✅ Message sending and display verification
- ✅ Multiple message handling and ordering
- ✅ Empty message validation
- ✅ Long message handling
- ✅ Message list scrolling functionality

### Command System Tests
- ✅ `/nick` command - Nickname changes
- ✅ `/clear` command - Message clearing
- ✅ `/help` command - Help message display
- ✅ `/who` command - Connected peers display

### UI State Management Tests
- ✅ Send button enable/disable states
- ✅ Empty state display
- ✅ Connected peers count display
- ✅ Message input clearing after send

### Platform-Specific Tests (Android)
- ✅ Back button handling
- ✅ Keyboard show/hide interactions
- ✅ Device rotation state persistence
- ✅ Swipe gestures and scrolling
- ✅ Long press gesture handling

## Test Infrastructure

### Appium Configuration

#### iOS Configuration
- **Platform**: iOS
- **Automation**: XCUITest
- **Device**: iPhone 15 Pro (Simulator)
- **App Bundle**: `com.bitchat.maui`
- **App Path**: `bin/Debug/net8.0-ios/iossimulator-arm64/BitChat.Maui.app`

#### Android Configuration
- **Platform**: Android
- **Automation**: UiAutomator2
- **Device**: Android Emulator
- **App Package**: `com.bitchat.maui`
- **App Activity**: `crc64b5e713d400f589b7.MainActivity`
- **APK Path**: `bin/Debug/net8.0-android/com.bitchat.maui-Signed.apk`

### Page Object Model

The `ChatPageObject` class provides a clean abstraction for interacting with the chat interface:

```csharp
// Element interactions
chatPage.SendMessage("Hello World");
chatPage.SendCommand("/nick NewName");
chatPage.ClearMessages();

// Verification methods
chatPage.VerifyMessageExists("Hello World");
chatPage.VerifyEmptyState();
chatPage.VerifyConnectedPeersCount(0);
```

### Cross-Platform Element Locators

Elements are located using platform-specific strategies:
- **iOS**: Accessibility IDs (`MobileBy.AccessibilityId`)
- **Android**: Resource IDs (`MobileBy.Id`)

## Running UI Tests

### Prerequisites

1. **Appium Server**: Install and start Appium server
   ```bash
   npm install -g appium
   appium driver install xcuitest  # For iOS
   appium driver install uiautomator2  # For Android
   appium server
   ```

2. **iOS Simulator**: 
   - Xcode with iOS Simulator
   - Built iOS app at the configured path

3. **Android Emulator**:
   - Android Studio with emulator
   - Built Android APK at the configured path

### Test Execution

```bash
# Run all UI tests (requires simulators/emulators)
dotnet test --filter "FullyQualifiedName~ChatUITests"

# Run iOS-specific tests only
dotnet test --filter "FullyQualifiedName~ChatUITests" --filter "Platform=iOS"

# Run Android-specific tests only  
dotnet test --filter "FullyQualifiedName~ChatUITests" --filter "Platform=Android"
```

### Test Environment Setup

1. **Build Applications**:
   ```bash
   # Build iOS app
   dotnet build -f net8.0-ios
   
   # Build Android app
   dotnet build -f net8.0-android
   ```

2. **Start Simulators/Emulators**:
   - iOS: Launch iOS Simulator
   - Android: Start Android emulator

3. **Start Appium Server**:
   ```bash
   appium server --address 127.0.0.1 --port 4723
   ```

## Test Debugging

### Screenshots
Tests automatically capture screenshots on failure for debugging:
- Location: `Tests/Screenshots/`
- Format: `{TestName}_{Platform}_{Timestamp}.png`

### Common Issues

1. **App Path Not Found**: Ensure apps are built and paths in `AppiumTestBase` are correct
2. **Appium Connection Failed**: Verify Appium server is running on `localhost:4723`
3. **Element Not Found**: Check that UI element automation IDs match between app and tests
4. **Simulator/Emulator Issues**: Restart simulators and ensure proper platform targeting

## Test Maintenance

### Adding New Tests
1. Create test method in appropriate test class (`ChatUITests.cs` or `ChatUITests.Android.cs`)
2. Use Page Object methods for interactions
3. Include platform-specific traits: `[Trait("Platform", "iOS")]`
4. Add comprehensive error handling with screenshots

### Updating Element Locators
1. Update automation IDs in XAML if needed
2. Modify locator methods in `ChatPageObject.cs`
3. Test on both iOS and Android platforms

### Performance Considerations
- Tests use `DisableParallelization = true` to avoid driver conflicts
- Implement proper wait strategies using `WaitForElement` methods
- Use Page Object pattern for maintainable and reusable code

## Integration with CI/CD

UI tests are designed to integrate with CI/CD pipelines:
- Tests can run in headless mode (when supported by Appium)
- Screenshot capture for build artifacts
- Proper test categorization for selective execution
- Cross-platform test matrix support

## US-7.1.1 Acceptance Criteria Coverage

✅ **AC1**: Users can view the chat interface  
✅ **AC2**: Users can send public messages  
✅ **AC3**: Users can see sent messages in the message list  
✅ **AC4**: Users can use IRC-style commands  
✅ **AC5**: The interface shows connection status  
✅ **AC6**: The interface handles empty states appropriately  
✅ **AC7**: Cross-platform compatibility (iOS/Android)

All acceptance criteria for US-7.1.1 Public Chat View are comprehensively tested through the UI test suite.