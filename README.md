# BitChat MAUI

## Project Overview

BitChat MAUI is a cross-platform port of the original iOS/macOS BitChat application, implementing a decentralized, peer-to-peer messaging system that operates over Bluetooth mesh networks. This revolutionary messaging platform requires no internet connectivity, servers, or user accounts - enabling truly private, offline communication.

### Key Features

- **Decentralized Mesh Networking**: Messages hop through multiple Bluetooth connections to reach distant peers
- **Zero Infrastructure**: No servers, no accounts, no phone numbers, no internet required
- **End-to-End Encryption**: Implements Noise Protocol Framework (XX pattern) for secure private messaging
- **Cross-Platform Support**: Runs on iOS, Android, macOS, and Windows
- **Privacy-First**: Ephemeral messages, no persistent storage, emergency data wipe capability
- **Dual Transport**: Automatically switches between Bluetooth mesh and Nostr protocol for mutual favorites
- **IRC-Style Interface**: Familiar commands like `/msg`, `/nick`, `/who` for power users

## Development Approach

This project is being developed through **pair programming** between a human developer and Claude (AI assistant). We're building this MAUI implementation from scratch while maintaining 100% feature parity with the original Swift version.

### Our Process

- **Collaborative Development**: Each feature is discussed, planned, and implemented together
- **Iterative Building**: We work in focused chunks, testing as we go
- **Knowledge Preservation**: All design decisions and implementation details are documented
- **Cross-Platform First**: Every feature is built to work across all target platforms

## Technology Stack

- **.NET 8 MAUI**: Cross-platform UI framework
- **C# 12**: Primary development language
- **Plugin.BLE**: Bluetooth Low Energy functionality
- **NSec.Cryptography**: Curve25519 and ChaCha20-Poly1305 encryption
- **CommunityToolkit.Mvvm**: MVVM pattern implementation
- **SQLite**: Local encrypted storage (when needed)

## Project Structure

```
BitChat.Maui/
├── Platforms/           # Platform-specific implementations
│   ├── iOS/            # iOS-specific Bluetooth and UI
│   ├── Android/        # Android-specific services
│   ├── MacCatalyst/    # macOS implementations
│   └── Windows/        # Windows BLE adapters
├── Core/               # Business logic and protocols
├── Presentation/       # Views and ViewModels
├── Infrastructure/     # Platform abstractions
└── Tests/             # Unit and integration tests
```

## Development Requirements

- .NET 8 SDK
- MAUI Workload (`dotnet workload install maui`)
- Xcode 15+ (for iOS/macOS development)
- Android SDK (API 21+)
- Visual Studio 2022 or VS Code with C# extensions

## Building and Running

```bash
# Restore dependencies
dotnet restore

# Build for all platforms
dotnet build

# Run on iOS Simulator
dotnet build -t:Run -f net8.0-ios

# Run on Android Emulator
dotnet build -t:Run -f net8.0-android

# Run on macOS
dotnet build -t:Run -f net8.0-maccatalyst

# Run on Windows
dotnet run -f net8.0-windows10.0.19041.0
```

---

## Development Log

This log tracks our pair programming sessions and the features we implement together.

### Session 1: Project Setup and Foundation
**Date**: August 13, 2025  
**Time**: 10:00 AM - 11:45 AM PST  
**Participants**: Vesh (Human) & Claude (AI)

#### Objectives
- Initialize MAUI project with proper structure
- Configure platform-specific requirements
- Add essential NuGet packages
- Verify cross-platform compilation and execution

#### Completed Tasks

1. **Project Initialization** (10:00 AM)
   - Created new .NET MAUI project in BitChat.Maui folder
   - Set application ID to `com.bitchat.maui`
   - Configured project for iOS, Android, macOS, and Windows targets

2. **Dependencies Setup** (10:15 AM)
   - Added MVVM and UI packages:
     - CommunityToolkit.Mvvm (8.2.2) for MVVM pattern
     - CommunityToolkit.Maui (7.0.1) for UI components
   - Added Bluetooth support:
     - Plugin.BLE (3.1.0) for cross-platform Bluetooth LE
   - Added Cryptography libraries:
     - NSec.Cryptography (24.4.0) for Curve25519 operations
     - Portable.BouncyCastle (1.9.0) for additional crypto functions
   - Added Networking and Storage:
     - System.Net.Http, System.Net.WebSockets.Client
     - Microsoft.Data.Sqlite (8.0.10)
     - System.Text.Json (8.0.5)
   - Added Testing frameworks:
     - xunit (2.9.2)
     - Moq (4.20.72)

3. **Platform Configuration** (10:30 AM)
   - **iOS Setup**:
     - Added Bluetooth permissions to Info.plist
     - Configured background modes for BLE operations
     - Added NSBluetoothAlwaysUsageDescription and NSBluetoothPeripheralUsageDescription
   - **Android Setup**:
     - Added Bluetooth permissions (BLUETOOTH, BLUETOOTH_ADMIN, BLUETOOTH_SCAN, BLUETOOTH_ADVERTISE, BLUETOOTH_CONNECT)
     - Added location permissions (required for BLE on Android)
     - Declared Bluetooth hardware features as required

4. **Build and Testing** (11:30 AM)
   - Fixed package version conflicts (Microsoft.Extensions.DependencyInjection)
   - Updated MauiProgram.cs to properly initialize CommunityToolkit
   - Successfully built project for all platforms
   - Tested on iOS Simulator (iPhone 16 Pro) - ✅ Success
   - Tested on Android Emulator (Pixel 8 API 34) - ✅ Success

#### Technical Decisions Made

- Chose Plugin.BLE over platform-specific implementations for faster development
- Selected NSec for cryptography to match Swift implementation's Curve25519 usage
- Configured minimum API levels: iOS 11.0, Android 21, macOS 13.1
- Set up proper code signing configuration for iOS development

#### Challenges Resolved

- Fixed NuGet package downgrade warning by updating DependencyInjection to 8.0.1
- Resolved MauiCommunityToolkit initialization order issue
- Handled macOS code signing errors (deferred for later resolution)

#### Next Steps

- Create core project structure (Core, Presentation, Infrastructure folders)
- Implement platform abstraction interfaces
- Begin Bluetooth service implementation
- Set up dependency injection container

#### Notes

- Project successfully runs on both iOS and Android platforms
- All essential dependencies are in place for BitChat feature implementation
- Ready to begin core functionality development in next session

---

*Last Updated: August 13, 2025, 11:45 AM PST*