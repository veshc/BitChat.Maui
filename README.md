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

## Development Progress

This project is being actively developed through pair programming sessions. For detailed development logs, implementation progress, and technical decisions, see:

📋 **[DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md)** - Complete session-by-session development history

### Latest Accomplishments
- ✅ **Project Foundation Complete** (Session 1) - Cross-platform MAUI setup with all dependencies
- ✅ **CI/CD Pipeline Complete** (Session 2) - 100% automated testing and builds for iOS & Android
- 🚧 **Ready for Feature Development** - Infrastructure foundation ready for BitChat implementation

---