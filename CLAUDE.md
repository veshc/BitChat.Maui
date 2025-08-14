# AI Context for BitChat MAUI

This document provides essential context for AI assistants working on the BitChat MAUI migration project. Read this first to understand the project's architecture, design decisions, and migration strategy from the original Swift implementation.

## Project Overview

BitChat MAUI is a cross-platform port of the original iOS/macOS BitChat application, implementing a decentralized, peer-to-peer messaging system that operates over Bluetooth mesh networks without requiring internet connectivity, servers, or user accounts. The MAUI version extends support to Android and Windows while maintaining 100% feature parity with the Swift implementation.

### Key Features

- **Cross-Platform Bluetooth Mesh Networking**: Multi-hop message relay over BLE for iOS, Android, macOS, Windows
- **Privacy-First Design**: No accounts, no persistent identifiers, ephemeral message storage
- **End-to-End Encryption**: Uses Noise Protocol Framework (XX pattern) for private messages
- **Dual Transport System**: Automatic switching between Bluetooth mesh and Nostr for mutual favorites
- **Store & Forward**: Messages cached for offline peers with automatic delivery on reconnection
- **IRC-Style Commands**: Familiar `/msg`, `/who`, `/nick` interface
- **Social Trust Layer**: Favorites system with mutual recognition and verification
- **Emergency Security**: Triple-tap data wipe and privacy protection features

## Migration Context

### Source Analysis

The original Swift application has been thoroughly analyzed with:

- **51 Swift source files** covering all functionality
- **Comprehensive requirements document** with 9 Epics, 25+ Features, 50+ User Stories
- **Detailed architecture specification** for MAUI implementation
- **Complete protocol documentation** including binary and Noise implementations

### Migration Goals

1. **100% Feature Parity**: Every feature from the Swift app must be implemented
2. **Cross-Platform Compatibility**: Support iOS, Android, macOS, Windows
3. **Shared Business Logic**: Maximum code reuse across platforms
4. **Security Preservation**: Maintain all cryptographic and privacy features
5. **Performance Equivalence**: Match or exceed Swift app performance

## Architecture Overview

### High-Level Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                        MAUI App Layer                            │
│           (Platform-specific UI and platform services)           │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                    Presentation Layer                            │
│          (ViewModels, Views, Converters, MVVM Pattern)          │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                     Business Logic Layer                         │
│          (Services, Managers, Command Processors)                │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                    Protocol & Security Layer                     │
│        (Noise Protocol, Binary Protocol, Nostr Protocol)        │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                  Platform Abstraction Layer                      │
│         (Bluetooth, Crypto, Storage, Networking)                 │
└─────────────────────────────────────────────────────────────────┘
                                   │
┌─────────────────────────────────────────────────────────────────┐
│                    Platform-Specific Layer                       │
│           (iOS, Android, macOS, Windows Implementations)         │
└─────────────────────────────────────────────────────────────────┘
```

### Project Structure

```
BitChat.Maui/
├── Platforms/
│   ├── iOS/                     # iOS-specific implementations
│   ├── Android/                 # Android-specific implementations
│   ├── MacCatalyst/            # macOS implementations
│   └── Windows/                # Windows implementations
├── Core/
│   ├── Models/                 # Data models and entities
│   ├── Services/               # Business logic services
│   ├── Protocols/              # Protocol implementations
│   ├── Security/               # Cryptography and security
│   ├── Utils/                  # Utility classes and extensions
│   └── Interfaces/             # Service abstractions
├── Presentation/
│   ├── ViewModels/             # MVVM ViewModels
│   ├── Views/                  # XAML Views and Pages
│   ├── Converters/             # Value converters
│   └── Behaviors/              # UI behaviors
├── Infrastructure/
│   ├── Bluetooth/              # Bluetooth abstraction
│   ├── Storage/                # Secure storage abstraction
│   ├── Networking/             # Network abstraction
│   └── Logging/                # Logging infrastructure
└── Tests/
    ├── Unit/                   # Unit tests
    ├── Integration/            # Integration tests
    └── EndToEnd/               # E2E tests
```

## Core Components Migration

### 1. Data Models

Original Swift models map directly to C# records and classes:

```csharp
// BitchatMessage - Core message entity
public record BitchatMessage
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Sender { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public bool IsPrivate { get; init; }
    public bool IsRelay { get; init; }
    public string? RecipientNickname { get; init; }
    public string? OriginalSender { get; init; }
}

// BitchatPeer - Network peer representation
public record BitchatPeer
{
    public string Id { get; init; } = string.Empty;
    public byte[] NoisePublicKey { get; init; } = Array.Empty<byte>();
    public string Nickname { get; init; } = string.Empty;
    public DateTime LastSeen { get; init; } = DateTime.UtcNow;
    public bool IsConnected { get; init; }
    public FavoriteRelationship? FavoriteStatus { get; init; }
    public string? NostrPublicKey { get; init; }
    public ConnectionState ConnectionState { get; init; }
}
```

### 2. Service Layer Architecture

Business logic services maintain similar responsibilities to Swift counterparts:

- **BluetoothMeshService**: Manages BLE connections and mesh networking
- **MessageRouter**: Intelligent routing between Bluetooth and Nostr transports
- **NoiseEncryptionService**: End-to-end encryption coordination
- **IdentityManager**: Cryptographic identity and key management
- **FavoritesService**: Social relationship management
- **NotificationService**: Cross-platform notification handling

### 3. Protocol Implementations

#### Binary Protocol Service

Maintains exact compatibility with Swift binary format:

- 13-byte fixed header with version, type, TTL, timestamp, flags
- Variable-length fields for sender ID, recipient ID, payload, signature
- PKCS#7 padding to standard block sizes (256, 512, 1024, 2048 bytes)
- Message fragmentation for BLE MTU limitations

#### Noise Protocol Service

Implements `Noise_XX_25519_ChaChaPoly_SHA256` pattern:

- Curve25519 for Diffie-Hellman operations
- ChaCha20-Poly1305 for authenticated encryption
- SHA-256 for hash operations
- Complete XX handshake implementation with mutual authentication

#### Nostr Protocol Service

NIP-17 gift-wrapped private messages for mutual favorites:

- Double-encrypted messages with ephemeral keys
- Metadata privacy through gift wrap structure
- Automatic transport fallback when Bluetooth unavailable
- Integration with favorites system for mutual authentication

## Technology Stack Mapping

### Core Framework Migration

| Swift Component | MAUI Equivalent       | Implementation Notes                |
| --------------- | --------------------- | ----------------------------------- |
| SwiftUI         | MAUI/XAML             | MVVM pattern with data binding      |
| Combine         | CommunityToolkit.Mvvm | Observable properties and messaging |
| Foundation      | .NET 8 BCL            | Core data types and collections     |
| UserDefaults    | Preferences API       | Settings and configuration storage  |

### Cryptography Migration

| Swift CryptoKit   | .NET Equivalent           | Implementation              |
| ----------------- | ------------------------- | --------------------------- |
| Curve25519        | NSec.Cryptography         | Third-party library for ECC |
| ChaCha20-Poly1305 | ChaCha20Poly1305 (.NET 8) | Built-in AEAD cipher        |
| SHA256            | SHA256 (.NET)             | Built-in hash function      |
| HMAC              | HMAC (.NET)               | Built-in MAC function       |

### Platform Services Migration

| iOS/macOS Service | MAUI Abstraction     | Platform Implementation                 |
| ----------------- | -------------------- | --------------------------------------- |
| CoreBluetooth     | IBluetooth interface | Plugin.BLE + custom wrappers            |
| Keychain Services | ISecureStorage       | Microsoft.Maui.Essentials.SecureStorage |
| UserNotifications | INotification        | Microsoft.Maui.Essentials.Notifications |
| Network.framework | INetworking          | System.Net + HttpClient                 |

## Key Design Patterns

### 1. Platform Abstraction

All platform-specific functionality accessed through interfaces:

```csharp
public interface IBluetoothService
{
    event EventHandler<PeerDiscoveredEventArgs> PeerDiscovered;
    event EventHandler<DataReceivedEventArgs> DataReceived;
    Task<bool> StartScanningAsync();
    Task<bool> ConnectToPeerAsync(string peerId);
    Task<bool> SendDataAsync(string peerId, byte[] data);
}
```

### 2. MVVM with Data Binding

ViewModels use CommunityToolkit.Mvvm for reactive UI:

```csharp
public partial class ChatViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<BitchatMessage> messages = new();

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        // Message sending logic
    }
}
```

### 3. Dependency Injection

Services registered and injected throughout the application:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBitChatServices(this IServiceCollection services)
    {
        services.AddSingleton<ICryptographicService, CryptographicService>();
        services.AddSingleton<IBluetoothMeshService, BluetoothMeshService>();
        services.AddSingleton<IMessageRouter, MessageRouter>();
        // ... other services
        return services;
    }
}
```

## Security Considerations

### Cryptographic Implementation

- **Key Generation**: Secure random number generation for all key material
- **Key Storage**: Platform secure storage (Keychain/Keystore/DPAPI)
- **Session Management**: Proper cleanup of sensitive data in memory
- **Forward Secrecy**: Ephemeral keys for each Noise session

### Privacy Features

- **Message Padding**: Consistent with Swift implementation
- **Timing Obfuscation**: Random delays to prevent traffic analysis
- **Emergency Wipe**: Complete data erasure on triple-tap
- **No Persistent Storage**: Messages stored only in memory by default

## Development Guidelines

### 1. Feature Parity Requirements

Every feature from the Swift implementation must be included:

- All message types and protocol features
- Complete command system (/nick, /msg, /who, /clear, /help, etc.)
- Identical security and privacy characteristics
- Same user experience patterns and behaviors

### 2. Cross-Platform Considerations

- Use platform abstractions for all native functionality
- Implement platform-specific optimizations where beneficial
- Maintain consistent behavior across all platforms
- Handle platform capability differences gracefully

### 3. Testing Strategy

- **Unit Tests**: Minimum 95% code coverage requirement
- **Integration Tests**: Cross-service interaction validation
- **End-to-End Tests**: Complete user workflow testing
- **Security Tests**: Cryptographic implementation validation
- **Platform Tests**: Functionality verification on all target platforms

### 4. Performance Requirements

- **Message Delivery**: < 2 seconds under normal conditions
- **Memory Usage**: < 100MB during normal operation
- **Battery Impact**: Minimal battery drain through efficient BLE usage
- **Connection Limits**: Support 50+ simultaneous connections

## CRITICAL DEVELOPMENT DISCIPLINE

### ⚠️ MANDATORY FAILURE RESPONSE PROTOCOL

**When ANY of the following fail, STOP ALL OTHER WORK immediately:**

- Build failures (any platform)
- Test failures (unit, integration, UI)  
- CI/CD pipeline failures
- Deployment issues
- Functionality not working as expected
- Performance regressions

**REQUIRED RESPONSE:**
1. **HALT**: Stop all other development work
2. **ANALYZE**: Perform thorough root cause analysis (not quick fixes)
3. **SOLVE**: Implement complete solution that addresses root cause
4. **VERIFY**: Test complete resolution across all affected areas
5. **ONLY THEN**: Resume other development work

**❌ NEVER:**
- Move to next task with known failures
- Accept partial solutions or workarounds
- Postpone failure resolution ("we'll fix this later")
- Suggest band-aid solutions for complex problems

### 🛑 NO PREMATURE TASK SWITCHING

**A task is NEVER complete until ALL of the following are true:**
- ✅ Code builds successfully on all target platforms
- ✅ All tests pass (unit, integration, UI)
- ✅ Functionality manually verified working
- ✅ No compiler warnings or errors
- ✅ CI/CD pipeline passes completely
- ✅ Documentation updated
- ✅ All acceptance criteria met

**❌ NEVER move to the next task if:**
- Current task has any failing builds or tests
- Current task functionality is partially working
- Current task has known issues or bugs
- Current task is "mostly done" but not 100% complete

### 🧪 TEST-FEATURE DEPENDENCY RULES

**ONLY write tests for features that exist and are implemented:**

✅ **CORRECT SEQUENCE:**
1. Implement feature completely
2. Verify feature works manually
3. Write comprehensive tests for existing functionality
4. Ensure all tests pass

❌ **NEVER DO:**
- Write UI tests for non-existent user interfaces
- Write tests for user stories that haven't been implemented
- Create tests for planned but not-yet-built features
- Write speculative tests for future functionality

**FOLLOW STRICT TDD:**
- Red (write failing test for existing feature) 
- Green (make test pass)
- Refactor (improve code while keeping tests passing)

### 🔍 PERSISTENCE REQUIREMENTS FOR COMPLEX PROBLEMS

**When encountering complex issues, apply "ULTRATHINK" approach:**

1. **DEEP ANALYSIS**: Don't accept surface-level explanations
2. **ROOT CAUSE**: Keep digging until fundamental cause is found
3. **COMPLETE SOLUTION**: Address the underlying issue, not symptoms
4. **THOROUGH TESTING**: Verify solution works in all scenarios
5. **PREVENT RECURRENCE**: Implement safeguards against similar issues

**❌ NEVER:**
- Give up on complex problems without complete resolution
- Accept "it's working now" without understanding why it failed
- Implement quick fixes that don't address root cause
- Move on when user indicates something "isn't working"

**✅ ALWAYS:**
- Persist until complete understanding is achieved
- Document the root cause and complete solution
- Verify the solution prevents the issue from recurring
- Learn from the problem to avoid similar issues

### 🚨 MANDATORY STOPPING CONDITIONS

**DEVELOPMENT MUST HALT when any of these occur:**

1. **Build Failures**: Any platform fails to compile
2. **Test Failures**: Any test in any category fails
3. **CI/CD Failures**: Pipeline shows any red status
4. **Functionality Broken**: Features stop working as expected
5. **User Feedback**: User indicates something "isn't working"
6. **Performance Issues**: Significant performance degradation
7. **Security Concerns**: Any potential security vulnerability

**RESUMPTION ONLY AFTER:**
- Issue completely resolved
- Root cause identified and documented
- Solution tested and verified
- Prevention measures implemented

### 📋 STRENGTHENED DEFINITION OF DONE

**A task is COMPLETE only when ALL criteria are met:**

#### Code Completion Criteria
- [ ] All acceptance criteria implemented and verified
- [ ] Code builds on iOS, Android, macOS, Windows
- [ ] Zero compiler warnings or errors
- [ ] All functionality manually tested and working
- [ ] Code follows established patterns and conventions

#### Testing Completion Criteria  
- [ ] Unit tests written with ≥95% coverage for new code
- [ ] Integration tests written for all service interactions
- [ ] UI tests written for all user-facing workflows (if applicable)
- [ ] All tests passing (unit, integration, UI)
- [ ] Manual testing completed on all target platforms

#### Quality Completion Criteria
- [ ] CI/CD pipeline passes completely (all green)
- [ ] Performance requirements met
- [ ] Security review completed (if applicable)
- [ ] Accessibility requirements met
- [ ] Error handling implemented for all scenarios

#### Documentation Completion Criteria
- [ ] Code documentation updated
- [ ] README/development log updated
- [ ] Architectural decisions documented
- [ ] User-facing documentation updated (if applicable)

**NO EXCEPTIONS: If ANY criterion is not met, the task is NOT complete.**

### 🎯 DEVELOPMENT SESSION PROTOCOL

**For each development session:**

#### Session Start
1. **Define Clear Completion Criteria**: What exactly constitutes "done"
2. **Identify Stopping Conditions**: What would require halting work
3. **Plan Testing Strategy**: How will completion be verified
4. **Set Success Metrics**: Measurable criteria for success

#### During Development
1. **Regular Check-ins**: Verify current task is on track every 30 minutes
2. **Immediate Failure Response**: Apply failure protocol if anything breaks
3. **No Task Switching**: Complete current task 100% before considering next
4. **Document Issues**: Track any problems and their complete resolutions

#### Session End
1. **Completion Verification**: Verify ALL completion criteria met
2. **Quality Check**: Run full test suite and verify CI/CD
3. **Documentation Update**: Update development log with session details
4. **Clean State**: Ensure no pending issues or partial work

**NEVER end a session with:**
- Failing builds or tests
- Partially implemented features
- Known bugs or issues
- Incomplete testing
- Pending CI/CD failures

---

## Implementation Roadmap

### Phase 1: Foundation

- Project setup and dependency injection
- Core models and service interfaces
- Basic cryptographic services
- Platform abstraction layer

### Phase 2: Bluetooth Core

- Bluetooth platform implementations
- Binary protocol encoding/decoding
- Basic mesh networking and peer discovery
- Connection management

### Phase 3: Security & Encryption

- Complete Noise protocol implementation
- Identity management and key storage
- Session management and cleanup
- Security testing and validation

### Phase 4: User Interface

- MVVM ViewModels and XAML views
- Chat interface and message display
- Peer management and favorites UI
- Command system implementation

### Phase 5: Advanced Features

- Nostr integration and NIP-17 implementation
- Message routing and transport selection
- Social features (favorites, blocking, verification)
- Share extensions and platform integration

### Phase 6: Platform Polish

- Platform-specific features and optimizations
- Comprehensive testing and bug fixes
- Performance optimization and profiling
- Documentation and deployment preparation

## Common Development Tasks

### Adding a New Service

1. Define interface in `Core/Interfaces/`
2. Implement service in `Core/Services/`
3. Register in dependency injection container
4. Add unit tests with mocked dependencies
5. Update integration tests if cross-service interaction

### Implementing Platform-Specific Functionality

1. Add method to platform abstraction interface
2. Implement in each platform-specific project
3. Use conditional compilation if needed
4. Test on all target platforms
5. Document platform-specific behavior differences

### Adding New Message Types

1. Define enum value in `MessageType`
2. Update binary protocol encoding/decoding
3. Add handling in `MessageRouter` and `ChatViewModel`
4. Update UI if user-facing feature
5. Add protocol and integration tests

### Security Implementation

1. Follow secure coding practices (no sensitive data in logs)
2. Use platform secure storage for all key material
3. Clear sensitive data from memory after use
4. Implement proper session cleanup
5. Add security-specific tests and validation

## Git Workflow and Best Practices

### Core Git Principles

1. **Commit Early and Often**: Make small, atomic commits that represent a single logical change
2. **Write Descriptive Messages**: Every commit message should clearly explain what and why
3. **Never Break the Build**: Ensure all tests pass before committing
4. **Review Before Committing**: Use `git diff` to review changes before staging
5. **Keep History Clean**: Use interactive rebase to clean up commits before merging

### Commit Message Guidelines

#### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

#### Types

- **feat**: New feature implementation
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code formatting (no functional changes)
- **refactor**: Code restructuring without changing functionality
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Maintenance tasks, dependency updates
- **security**: Security-related changes
- **platform**: Platform-specific implementations

#### Examples

```bash
# Good commit messages
git commit -m "feat(bluetooth): implement peer discovery for iOS platform"
git commit -m "fix(encryption): resolve key derivation issue in Noise handshake"
git commit -m "test(message-router): add unit tests for private message routing"
git commit -m "security(wipe): implement triple-tap emergency data wipe"
git commit -m "perf(mesh): optimize message relay algorithm for battery efficiency"

# Detailed commit with body
git commit -m "feat(favorites): add mutual favorites recognition system

- Implement favorite relationship management
- Add UI indicators for mutual favorites
- Enable automatic Nostr transport for mutual connections
- Include verification badge display

Implements: USER-STORY-23"
```

#### Commit Message Rules

1. **Subject Line**: 50 characters max, imperative mood
2. **Body**: 72 characters per line, explain what and why
3. **Reference Issues**: Include ticket/issue numbers
4. **Breaking Changes**: Note with BREAKING CHANGE: prefix
5. **Co-authors**: Credit pair programming or collaboration

### Branching Strategy

#### Branch Types

```bash
# Feature branches
feature/user-story-id-description
feature/US-123-private-messaging
feature/US-456-bluetooth-discovery

# Bug fix branches
fix/issue-id-description
fix/BUG-789-connection-timeout
fix/BUG-234-encryption-failure

# Platform-specific branches
platform/ios-bluetooth-implementation
platform/android-background-service
platform/windows-ble-adapter

# Release branches
release/v1.0.0
release/v1.1.0-beta

# Hotfix branches
hotfix/critical-security-patch
hotfix/v1.0.1-crash-fix
```

#### Branch Rules

1. **Always branch from main/develop**: Never commit directly to main
2. **Keep branches short-lived**: Merge within 2-3 days maximum
3. **Delete after merging**: Clean up merged branches
4. **Rebase before merging**: Keep history linear when possible
5. **Update frequently**: Pull latest changes daily

### Git Workflow Commands

#### Daily Development Workflow

```bash
# Start new feature
git checkout -b feature/US-123-new-feature
git push -u origin feature/US-123-new-feature

# Regular commits during development
git add -p  # Review changes interactively
git commit -m "feat(component): implement specific functionality"

# Keep branch updated
git fetch origin
git rebase origin/main

# Before creating PR
git rebase -i origin/main  # Clean up commit history
git push --force-with-lease  # Update remote branch safely
```

#### Commit Frequency Guidelines

```bash
# Commit after each logical unit of work
- After implementing a new method/function
- After writing tests for a component
- After fixing a specific bug
- After refactoring a module
- After updating documentation

# Example development session
git commit -m "feat(peer): add peer discovery interface"
git commit -m "feat(peer): implement iOS bluetooth discovery"
git commit -m "test(peer): add discovery service unit tests"
git commit -m "docs(peer): update discovery documentation"
```

### Code Review Preparation

#### Pre-Commit Checklist

```bash
# Run before EVERY commit
dotnet format  # Format code
dotnet build  # Ensure compilation
dotnet test --filter "Category=Unit"  # Run unit tests
git diff --cached  # Review staged changes

# Automated pre-commit script
#!/bin/bash
echo "Running pre-commit checks..."
dotnet format --verify-no-changes
dotnet build --no-restore
dotnet test --filter "Category=Unit" --no-build
```

#### Pull Request Guidelines

```markdown
## PR Title Format
[Type] User Story ID: Brief description
Example: [Feature] US-123: Implement private messaging

## PR Description Template
### Summary
Brief description of changes

### Changes Made
- List of specific changes
- Implementation details
- Architecture decisions

### Testing
- [ ] Unit tests passing (95% coverage)
- [ ] Integration tests passing
- [ ] UI tests passing
- [ ] Tested on iOS
- [ ] Tested on Android
- [ ] Tested on macOS
- [ ] Tested on Windows

### Screenshots/Videos
(if applicable)

### Related Issues
Closes #123
Implements USER-STORY-123
```

### Git Hooks Configuration

#### Pre-Commit Hook (.git/hooks/pre-commit)

```bash
#!/bin/bash
set -e

echo "🔍 Running pre-commit checks..."

# Check for build errors
echo "📦 Building project..."
dotnet build --no-restore

# Run unit tests
echo "🧪 Running unit tests..."
dotnet test --filter "Category=Unit" --no-build

# Check code formatting
echo "💅 Checking code format..."
dotnet format --verify-no-changes

# Check for security issues
echo "🔒 Checking for secrets..."
git diff --cached --name-only | xargs -I {} sh -c 'git show :"{}" | grep -E "(password|secret|key|token).*=.*[\"'\'']" && exit 1 || true'

echo "✅ All pre-commit checks passed!"
```

#### Commit-Msg Hook (.git/hooks/commit-msg)

```bash
#!/bin/bash

# Check commit message format
commit_regex='^(feat|fix|docs|style|refactor|perf|test|chore|security|platform)(\([a-z0-9-]+\))?: .{1,50}$'
commit_message=$(cat "$1")
first_line=$(echo "$commit_message" | head -n1)

if ! echo "$first_line" | grep -qE "$commit_regex"; then
    echo "❌ Invalid commit message format!"
    echo "Expected: <type>(<scope>): <subject>"
    echo "Example: feat(bluetooth): add peer discovery"
    exit 1
fi

echo "✅ Commit message format valid"
```

### Git Configuration for Project

```bash
# Set up local git configuration
git config core.autocrlf input  # Handle line endings
git config pull.rebase true  # Rebase on pull
git config fetch.prune true  # Remove deleted remote branches
git config diff.algorithm histogram  # Better diff algorithm
git config merge.conflictstyle diff3  # Better conflict markers

# Set up aliases for common operations
git config alias.co checkout
git config alias.br branch
git config alias.ci commit
git config alias.st status
git config alias.unstage 'reset HEAD --'
git config alias.last 'log -1 HEAD'
git config alias.visual '!gitk'
```

### Handling Merge Conflicts

```bash
# When conflicts occur
git status  # See conflicted files
git diff  # Review conflicts

# Resolve conflicts in editor
# Look for <<<<<<< HEAD, =======, >>>>>>> markers

# After resolving
git add <resolved-file>
git rebase --continue  # or git merge --continue

# If things go wrong
git rebase --abort  # or git merge --abort
```

### Git Best Practices Summary

1. **Commit Frequency**: At least 3-5 commits per day during active development
2. **Commit Size**: Each commit should be reviewable in < 5 minutes
3. **Commit Atomicity**: One commit = one logical change
4. **Message Quality**: Anyone should understand what and why from the message
5. **Branch Hygiene**: No branch should live > 3 days
6. **Review Discipline**: All code must be reviewed before merging
7. **Test Coverage**: Never commit code that reduces coverage
8. **Documentation**: Update docs in the same commit as code changes

---

## User Story Development Workflow

This section defines the mandatory process for implementing user stories to ensure quality, consistency, and proper testing coverage.

### Pre-Development Checklist

Before starting ANY user story development:

1. **Create Feature Branch**: Always create a new branch before any code changes
2. **Review Requirements**: Understand all acceptance criteria
3. **Plan Testing Strategy**: Identify unit tests, integration tests, and UI tests needed
4. **Estimate Testing Effort**: Include test development time in story estimation

### User Story Branch Naming Convention

```bash
# Format: feature/US-X.X.X-brief-description
feature/US-3.1.1-nickname-management
feature/US-1.1.1-peer-discovery  
feature/US-2.1.1-noise-handshake
```

### Development Process Steps

#### Step 1: Branch Creation
```bash
# Create and switch to feature branch
git checkout -b feature/US-X.X.X-story-description
git push -u origin feature/US-X.X.X-story-description
```

#### Step 2: Implementation
- Implement feature according to acceptance criteria
- Follow established architecture patterns (MVVM, DI, interfaces)
- Write code with testing in mind (dependency injection, mockable interfaces)
- Commit frequently with descriptive messages

#### Step 3: Unit Testing (MANDATORY)
- **Coverage Requirement**: ≥95% for new code
- **Test All Logic**: Business logic, validation rules, service methods
- **Mock Dependencies**: Use Moq for external dependencies
- **Test Edge Cases**: Invalid inputs, error conditions, boundary values

#### Step 4: UI Testing (MANDATORY)
- **Framework**: Use Appium for cross-platform UI testing
- **Test User Workflows**: Complete user scenarios from acceptance criteria
- **Test Validation**: UI validation messages, error states
- **Cross-Platform**: Test on iOS and Android minimum

#### Step 5: Integration Testing (MANDATORY)
- **Service Interactions**: Test service-to-service communication
- **Data Persistence**: Test settings/data storage and retrieval
- **Platform Integration**: Test platform-specific functionality

### Definition of Done (DoD) Checklist

A user story is **NEVER** complete until ALL of the following are met:

#### Code Requirements
- [ ] All acceptance criteria implemented and working
- [ ] Code follows established patterns and conventions
- [ ] No compiler warnings or errors
- [ ] Code builds on all target platforms (iOS, Android, macOS, Windows)

#### Testing Requirements
- [ ] **Unit Tests**: ≥95% code coverage for new code
- [ ] **Unit Tests**: All business logic tested with edge cases
- [ ] **UI Tests**: All user workflows tested with Appium
- [ ] **Integration Tests**: Service interactions tested
- [ ] **Cross-Platform Tests**: Functionality verified on iOS and Android
- [ ] **All Tests Passing**: No failing tests in any category

#### Quality Requirements
- [ ] **Manual Testing**: Feature manually tested on target platforms
- [ ] **Performance**: No performance regressions introduced
- [ ] **Security**: Security review completed (if applicable)
- [ ] **Accessibility**: UI accessibility verified
- [ ] **Error Handling**: All error scenarios handled gracefully

#### Documentation Requirements
- [ ] **Code Documentation**: Public APIs documented
- [ ] **README Updated**: Development log updated with session details
- [ ] **Architecture**: Any architectural decisions documented

### Testing Framework Setup

#### Unit Testing
```bash
# Test project structure
Tests/
├── Unit/
│   ├── Models/
│   ├── Services/
│   └── ViewModels/
├── Integration/
│   ├── Services/
│   └── Persistence/
└── UI/
    ├── iOS/
    ├── Android/
    └── Shared/
```

#### Required Testing Packages
```xml
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="FluentAssertions" Version="6.12.1" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
<PackageReference Include="Appium.WebDriver" Version="5.0.0" />
```

### Story Completion Process

#### Before Marking Complete
1. **Run Full Test Suite**: All unit, integration, and UI tests must pass
2. **Verify Coverage**: Use `dotnet test --collect:"XPlat Code Coverage"`
3. **Manual Verification**: Test on real devices/simulators
4. **Code Review**: Self-review all changes before committing
5. **Update Documentation**: Update README development log

#### Final Commit and PR
```bash
# Final commit with comprehensive message
git add .
git commit -m "feat(user-story): complete US-X.X.X implementation with full test coverage

- Implement all acceptance criteria
- Add comprehensive unit tests (≥95% coverage)  
- Add Appium UI tests for all user workflows
- Add integration tests for service interactions
- Test on iOS and Android platforms
- Update documentation and development log

Closes: US-X.X.X

🤖 Generated with Claude Code

Co-Authored-By: Claude <noreply@anthropic.com>"

# Create pull request
gh pr create --title "feat: US-X.X.X Story Title" --body "$(cat <<'EOF'
## User Story
US-X.X.X: Story description

## Implementation Summary
- [Acceptance criteria met]
- [Key implementation details]

## Testing Completed
- [x] Unit tests written and passing (≥95% coverage)
- [x] UI tests written for all user workflows  
- [x] Integration tests written for service interactions
- [x] Manual testing completed on iOS and Android
- [x] All tests passing

## Test Coverage
- Unit Tests: X% coverage
- Total Tests: X passing, 0 failing

🤖 Generated with Claude Code
EOF
)"
```

### Common Mistakes to Avoid

❌ **NEVER do these:**
- Start coding without creating a feature branch
- Mark a story complete without unit tests
- Mark a story complete without UI tests  
- Skip testing on target platforms
- Ignore test coverage requirements
- Commit untested code

✅ **ALWAYS do these:**
- Create feature branch before any code changes
- Write tests as you implement features
- Test on multiple platforms
- Verify all acceptance criteria before marking complete
- Update documentation as part of the story

### Quality Gates

Each user story must pass through these quality gates:

1. **Development Gate**: Feature implemented and manually tested
2. **Unit Test Gate**: ≥95% coverage, all tests passing
3. **UI Test Gate**: All user workflows tested with Appium
4. **Integration Gate**: Service interactions tested
5. **Platform Gate**: Tested on iOS and Android
6. **Documentation Gate**: README and docs updated

**Only after ALL gates pass can a user story be marked complete.**

---

## Testing Considerations

### Unit Testing

```csharp
[TestClass]
public class MessageRouterTests
{
    private Mock<IBluetoothMeshService> _mockMesh;
    private Mock<INostrService> _mockNostr;
    private MessageRouter _router;

    [TestInitialize]
    public void Setup()
    {
        _mockMesh = new Mock<IBluetoothMeshService>();
        _mockNostr = new Mock<INostrService>();
        _router = new MessageRouter(_mockMesh.Object, _mockNostr.Object);
    }
}
```

### Integration Testing

- Test service interactions with real implementations
- Validate protocol compatibility with Swift version
- Test platform-specific implementations
- Verify security and encryption functionality

### End-to-End Testing

- Complete user workflows on all platforms
- Message delivery across different transport combinations
- Social features and favorites system
- Emergency wipe and security features

## UI Testing Framework

### Test Framework Selection

For MAUI UI testing, use the following frameworks:

- **Appium**: Cross-platform UI testing for iOS, Android, Windows
- **MAUI Test Framework**: Native MAUI UI testing capabilities
- **Xamarin.UITest**: Legacy support for platform-specific tests (if needed)

### UI Test Structure

```csharp
[TestClass]
public class ChatViewUITests
{
    private IApp _app;

    [TestInitialize]
    public void BeforeEachTest()
    {
        _app = ConfigureApp
            .iOS // or .Android, .Windows
            .StartApp();
    }

    [TestMethod]
    public void SendMessage_ShouldDisplayInChatView()
    {
        // Arrange
        _app.EnterText("MessageInput", "Test message");
        
        // Act
        _app.Tap("SendButton");
        
        // Assert
        _app.WaitForElement(c => c.Text("Test message"));
        Assert.IsTrue(_app.Query("MessageList").Any(m => m.Text == "Test message"));
    }
}
```

### UI Testing Best Practices

1. **Page Object Pattern**: Create page objects for each screen
2. **Test Data Isolation**: Use unique test data for each test
3. **Wait Strategies**: Implement proper waits for async operations
4. **Cross-Platform Tests**: Write platform-agnostic tests when possible
5. **Accessibility Testing**: Verify screen reader compatibility
6. **Visual Regression**: Capture screenshots for visual validation

### Platform-Specific UI Testing

```csharp
// iOS-specific test
[TestMethod]
[Platform(Platform.iOS)]
public void TripleTap_ShouldTriggerEmergencyWipe_iOS()
{
    _app.TripleTap("MainView");
    _app.WaitForElement("EmergencyWipeConfirmation");
    Assert.IsTrue(_app.Query("DataWipedMessage").Any());
}

// Android-specific test
[TestMethod]
[Platform(Platform.Android)]
public void BackgroundService_ShouldMaintainConnection_Android()
{
    _app.SendAppToBackground(TimeSpan.FromSeconds(10));
    _app.Reactivate();
    Assert.IsTrue(_app.Query("ConnectionStatus").First().Text == "Connected");
}
```

## User Story Testing Requirements

### Definition of Done for User Stories

Every user story MUST meet the following testing criteria before being marked as complete:

1. **Unit Test Coverage**: Minimum 95% code coverage for new code
2. **Integration Tests**: All service interactions tested
3. **UI Tests**: User-facing functionality has automated UI tests
4. **Cross-Platform Validation**: Tested on all target platforms
5. **Security Review**: Security-related stories undergo security testing
6. **Performance Validation**: Performance metrics meet requirements

### User Story Test Checklist

```markdown
## User Story: [Story Name]

### Testing Requirements
- [ ] Unit tests written and passing (≥95% coverage)
- [ ] Integration tests for service interactions
- [ ] UI tests for user workflows
- [ ] Tested on iOS
- [ ] Tested on Android
- [ ] Tested on macOS
- [ ] Tested on Windows
- [ ] Performance benchmarks met
- [ ] Security tests passed (if applicable)
- [ ] Accessibility tests passed
- [ ] Edge cases covered
- [ ] Error scenarios tested
```

### Example User Story Test Implementation

```csharp
// User Story: As a user, I want to send private messages
public class PrivateMessageUserStoryTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void EncryptMessage_ShouldUseNoiseProtocol() { }
        
        [TestMethod]
        public void RoutePrivateMessage_ShouldSelectCorrectRecipient() { }
    }

    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void SendPrivateMessage_ShouldDeliverToRecipient() { }
        
        [TestMethod]
        public void PrivateMessage_ShouldNotBeVisibleToOthers() { }
    }

    [TestClass]
    public class UITests
    {
        [TestMethod]
        public void SendPrivateMessage_UIWorkflow() { }
        
        [TestMethod]
        public void PrivateMessageIndicator_ShouldBeVisible() { }
    }
}
```

## Test Coverage Requirements and Verification

### Coverage Targets

- **Overall Project**: ≥90% code coverage
- **New Code**: ≥95% code coverage
- **Critical Components**: 100% code coverage
  - Cryptographic services
  - Security features
  - Protocol implementations
  - Message routing

### Coverage Verification Commands

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate coverage report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport

# Check coverage thresholds
dotnet test /p:CollectCoverage=true /p:Threshold=95 /p:ThresholdType=line
```

### Automated Coverage Gates

```yaml
# Azure DevOps Pipeline
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    arguments: '--collect:"XPlat Code Coverage" --settings:coverlet.runsettings'
    
- task: PublishCodeCoverageResults@1
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
    failIfCoverageEmpty: true
```

### Coverage Exclusions

Acceptable exclusions from coverage requirements:

```csharp
[ExcludeFromCodeCoverage]
public class PlatformSpecificImplementation { }

[ExcludeFromCodeCoverage(Justification = "UI-only code tested via UI tests")]
public class ViewConverter { }
```

## Automated Test Execution

### Pre-Commit Testing

Every commit MUST pass the following tests:

```bash
# Run before each commit
dotnet test --filter "Category=Unit" --no-build
dotnet test --filter "Category=Integration" --no-build
```

### Continuous Integration Testing

```yaml
# GitHub Actions workflow
name: Test Suite
on: [push, pull_request]

jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Run Unit Tests
      run: dotnet test --filter "Category=Unit"
    
    - name: Run Integration Tests
      run: dotnet test --filter "Category=Integration"
    
    - name: Run UI Tests
      run: dotnet test --filter "Category=UI"
    
    - name: Check Coverage
      run: |
        dotnet test /p:CollectCoverage=true /p:Threshold=95
        
    - name: Security Tests
      run: dotnet test --filter "Category=Security"
```

### Test Execution Commands for Development

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=UI"
dotnet test --filter "Category=Security"
dotnet test --filter "Category=Performance"

# Run tests for specific platform
dotnet test --runtime ios-arm64
dotnet test --runtime android-arm64
dotnet test --runtime osx-x64
dotnet test --runtime win-x64

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutput=coverage.json

# Run tests in parallel
dotnet test --parallel

# Run tests with detailed logging
dotnet test --logger "console;verbosity=detailed"
```

### Test Failure Protocol

When tests fail:

1. **Immediate Action**: Fix failing tests before proceeding
2. **Root Cause Analysis**: Identify why tests failed
3. **Update Tests**: Modify tests if requirements changed
4. **Document Changes**: Update test documentation
5. **Re-run Full Suite**: Ensure no regression

### Performance Testing

```csharp
[TestClass]
public class PerformanceTests
{
    [TestMethod]
    [Timeout(2000)] // 2 seconds max
    public void MessageDelivery_ShouldMeetPerformanceTarget()
    {
        var stopwatch = Stopwatch.StartNew();
        // Send message
        stopwatch.Stop();
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000);
    }
}
```

## Test Documentation Requirements

### Test Plan Template

For each feature implementation:

```markdown
# Test Plan: [Feature Name]

## Test Scope
- Components covered
- Components excluded
- Test environment requirements

## Test Strategy
- Unit testing approach
- Integration testing approach
- UI testing approach
- Performance testing approach
- Security testing approach

## Test Cases
1. [Test Case 1]: Description, steps, expected result
2. [Test Case 2]: Description, steps, expected result

## Test Data Requirements
- Required test data
- Data setup procedures
- Data cleanup procedures

## Success Criteria
- All tests passing
- Coverage targets met
- Performance benchmarks achieved
- No critical/high severity bugs
```

## Debugging and Troubleshooting

### Common Issues

1. **Bluetooth Connection Problems**: Check platform permissions and BLE availability
2. **Message Delivery Failures**: Verify protocol compatibility and TTL settings
3. **Encryption Errors**: Validate key generation and Noise session state
4. **UI Binding Issues**: Check ViewModel property notifications and data context

### Debugging Tools

- **Bluetooth Analyzers**: Platform-specific BLE debugging tools
- **Protocol Analyzers**: Custom tools for BitChat binary protocol
- **Security Testing**: Cryptographic implementation validation tools
- **Performance Profilers**: Memory and CPU usage analysis

## Security Threat Model

### Threats Addressed

- **Man-in-the-Middle**: Noise protocol mutual authentication
- **Eavesdropping**: End-to-end encryption for private messages
- **Traffic Analysis**: Message padding and timing obfuscation
- **Device Compromise**: Emergency wipe and ephemeral storage
- **Replay Attacks**: Nonce-based protection in Noise sessions

### Security Testing

- Static code analysis for security vulnerabilities
- Cryptographic implementation review
- Penetration testing of protocol implementations
- Security audit of key management and storage

## Performance Optimization

### Bluetooth Optimization

- Connection pooling and reuse
- Efficient scanning and advertising patterns
- Message batching for improved throughput
- Adaptive duty cycling for battery conservation

### Memory Management

- Object pooling for expensive cryptographic operations
- Weak references to prevent memory leaks
- Proper disposal of unmanaged resources
- Configurable message history limits

### Cross-Platform Performance

- Platform-specific optimizations where beneficial
- Efficient data structures and algorithms
- Background processing for non-UI operations
- Lazy loading and virtualization for large data sets

---

## Quick Start for AI Assistants

1. **Understand the Migration Context**: This is a Swift-to-MAUI port, not a new application
2. **Reference the Requirements**: All features are documented in `requirements/BITCHAT_REQUIREMENTS.md`
3. **Follow the Architecture**: Detailed technical specs in `architecture/BITCHAT_MAUI_ARCHITECTURE.md`
4. **Maintain Feature Parity**: Every Swift feature must have a MAUI equivalent
5. **Prioritize Security**: This is critical infrastructure for users in challenging environments
6. **Use Platform Abstractions**: All native functionality goes through interfaces
7. **Test Thoroughly**: Security and protocol compatibility are non-negotiable

When implementing features, always reference the original Swift implementation for behavior and the requirements document for acceptance criteria. Prioritize security, privacy, and cross-platform compatibility in all development decisions.

The goal is to create a MAUI application that is indistinguishable from the Swift version in functionality while extending support to Android and Windows platforms.
