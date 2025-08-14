# BitChat MAUI - Agile Requirements Document

## Executive Summary

BitChat is a decentralized, peer-to-peer messaging application that enables secure communication over Bluetooth mesh networks without requiring internet connectivity, servers, or user accounts. This document outlines the requirements for migrating the iOS/macOS Swift application to a cross-platform .NET MAUI implementation while maintaining complete feature parity.

**Vision**: Create a censorship-resistant, privacy-first communication platform that works in situations where traditional infrastructure is unavailable or untrusted.

**Scope**: Full-featured MAUI application supporting iOS, Android, macOS, and Windows platforms with identical functionality to the existing Swift implementation.

---

## 📋 Product Backlog Overview

### Epic Priority Matrix

| Epic                          | Priority        | Complexity | Business Value |
| ----------------------------- | --------------- | ---------- | -------------- |
| Decentralized Mesh Networking | **Must Have**   | High       | Critical       |
| Secure Messaging              | **Must Have**   | High       | Critical       |
| User Identity & Trust         | **Must Have**   | Medium     | High           |
| Social Features               | **Should Have** | Medium     | High           |
| Cross-Transport Communication | **Should Have** | High       | Medium         |
| Privacy & Security            | **Must Have**   | Medium     | Critical       |

---

## 🎯 EPIC 1: Decentralized Mesh Networking

_Enable device-to-device communication through Bluetooth LE mesh networking_

### Feature 1.1: Bluetooth LE Discovery & Connection

#### User Stories:

**US-1.1.1** - Peer Discovery

- **As a** user
- **I want to** automatically discover other BitChat users nearby via Bluetooth
- **So that** I can communicate without manual setup
- **Acceptance Criteria:**
  - [ ] App scans for BitChat devices when Bluetooth is enabled
  - [ ] App advertises its presence to other BitChat devices
  - [ ] Discovery works within ~30m range (typical BLE range)
  - [ ] No user intervention required for peer discovery
- **Priority:** Must Have
- **Story Points:** 8

**US-1.1.2** - Automatic Connection Management

- **As a** user
- **I want** the app to automatically connect to discovered peers
- **So that** I can start communicating immediately
- **Acceptance Criteria:**
  - [ ] App automatically establishes BLE connections to discovered peers
  - [ ] Maintains multiple simultaneous connections (iOS: up to 8, Android: varies)
  - [ ] Handles connection failures gracefully with retry logic
  - [ ] Reconnects to previously known peers when they come back in range
- **Priority:** Must Have
- **Story Points:** 13

### Feature 1.2: Multi-Hop Message Relay

#### User Stories:

**US-1.2.1** - Message Forwarding

- **As a** user
- **I want** my messages to reach distant peers through intermediate devices
- **So that** I can communicate beyond direct Bluetooth range
- **Acceptance Criteria:**
  - [ ] Messages are automatically relayed through connected peers
  - [ ] TTL (Time To Live) prevents infinite message loops (max 7 hops)
  - [ ] Bloom filter prevents duplicate message processing
  - [ ] Message integrity maintained through relay chain
- **Priority:** Must Have
- **Story Points:** 21

**US-1.2.2** - Store and Forward

- **As a** user
- **I want** messages to be cached and delivered when recipients reconnect
- **So that** I don't miss messages when temporarily offline
- **Acceptance Criteria:**
  - [ ] Messages cached locally when recipient is offline
  - [ ] Automatic delivery when recipient reconnects
  - [ ] Cache size limits to prevent memory issues
  - [ ] Cache persistence across app restarts
- **Priority:** Should Have
- **Story Points:** 13

### Feature 1.3: Binary Protocol Implementation

#### User Stories:

**US-1.3.1** - Efficient Message Encoding

- **As a** developer
- **I want** messages encoded in an efficient binary format
- **So that** bandwidth usage is minimized over BLE
- **Acceptance Criteria:**
  - [ ] Custom binary protocol with 1-byte message type identifiers
  - [ ] Variable-length fields with length prefixes
  - [ ] Message fragmentation for payloads > 512 bytes (BLE MTU)
  - [ ] Network byte order for multi-byte values
- **Priority:** Must Have
- **Story Points:** 13

---

## 🔐 EPIC 2: Secure Messaging

_Implement end-to-end encryption for private communications_

### Feature 2.1: Noise Protocol Implementation

#### User Stories:

**US-2.1.1** - Noise XX Handshake

- **As a** user
- **I want** my private messages to be end-to-end encrypted
- **So that** only the intended recipient can read them
- **Acceptance Criteria:**
  - [ ] Implements Noise_XX_25519_ChaChaPoly_SHA256 pattern
  - [ ] Mutual authentication between peers
  - [ ] Forward secrecy through ephemeral keys
  - [ ] Automatic handshake initiation for private messages
- **Priority:** Must Have
- **Story Points:** 21

**US-2.1.2** - Session Management

- **As a** user
- **I want** encrypted sessions to be established transparently
- **So that** I can send private messages without manual key exchange
- **Acceptance Criteria:**
  - [ ] Automatic session establishment on first private message
  - [ ] Session reuse for subsequent messages
  - [ ] Session cleanup on peer disconnection
  - [ ] Race condition handling for simultaneous handshakes
- **Priority:** Must Have
- **Story Points:** 13

### Feature 2.2: Message Encryption

#### User Stories:

**US-2.2.1** - Private Message Encryption

- **As a** user
- **I want** my private messages encrypted before transmission
- **So that** intermediate relay nodes cannot read the content
- **Acceptance Criteria:**
  - [ ] All private messages encrypted with session keys
  - [ ] Public messages remain unencrypted for mesh efficiency
  - [ ] Encryption status clearly indicated in UI
  - [ ] Automatic fallback to public message if encryption fails
- **Priority:** Must Have
- **Story Points:** 8

### Feature 2.3: Cryptographic Identity

#### User Stories:

**US-2.3.1** - Key Generation and Storage

- **As a** user
- **I want** unique cryptographic keys generated for my identity
- **So that** I can be authenticated by other users
- **Acceptance Criteria:**
  - [ ] Curve25519 key pair for Noise protocol
  - [ ] Ed25519 key pair for message signing
  - [ ] Secure key storage in platform keychain/secure storage
  - [ ] Key generation on first app launch
- **Priority:** Must Have
- **Story Points:** 8

---

## 👤 EPIC 3: User Identity & Trust

_Manage user identities and trust relationships_

### Feature 3.1: Identity Management

#### User Stories:

**US-3.1.1** - Nickname Management

- **As a** user
- **I want to** set and change my display nickname
- **So that** other users can recognize me
- **Acceptance Criteria:**
  - [ ] User can set/change nickname in settings
  - [ ] Nickname displayed to other users
  - [ ] Nickname validation (length, characters)
  - [ ] Nickname persistence across app restarts
- **Priority:** Must Have
- **Story Points:** 3

**US-3.1.2** - Ephemeral Identity Rotation

- **As a** user
- **I want** my peer ID to change regularly
- **So that** my communications remain private and untraceable
- **Acceptance Criteria:**
  - [ ] Peer ID rotates periodically (configurable interval)
  - [ ] Cryptographic identity remains stable
  - [ ] Social relationships preserved through rotation
  - [ ] No disruption to active conversations
- **Priority:** Should Have
- **Story Points:** 13

### Feature 3.2: Fingerprint Verification

#### User Stories:

**US-3.2.1** - Fingerprint Display

- **As a** user
- **I want to** view cryptographic fingerprints of my contacts
- **So that** I can verify their identity out-of-band
- **Acceptance Criteria:**
  - [ ] SHA-256 hash of static public key displayed
  - [ ] Formatted for easy reading (4-character groups)
  - [ ] Copy to clipboard functionality
  - [ ] QR code generation for easy sharing
- **Priority:** Should Have
- **Story Points:** 5

**US-3.2.2** - Identity Verification

- **As a** user
- **I want to** mark contacts as verified
- **So that** I can trust their communications
- **Acceptance Criteria:**
  - [ ] Manual verification marking in contact view
  - [ ] Verified status persists across sessions
  - [ ] Visual indicator for verified contacts
  - [ ] Warning for unverified encrypted communications
- **Priority:** Should Have
- **Story Points:** 5

---

## 👥 EPIC 4: Social Features

_Enable user relationships and social interactions_

### Feature 4.1: Favorites System

#### User Stories:

**US-4.1.1** - Add/Remove Favorites

- **As a** user
- **I want to** mark certain contacts as favorites
- **So that** I can easily find and prioritize them
- **Acceptance Criteria:**
  - [ ] Star/unstar contacts in peer list
  - [ ] Favorites appear at top of contact list
  - [ ] Favorites status synchronized with contact
  - [ ] Notification when favorites come online
- **Priority:** Should Have
- **Story Points:** 8

**US-4.1.2** - Mutual Favorites Recognition

- **As a** user
- **I want to** know when someone has favorited me
- **So that** I can understand our mutual connection status
- **Acceptance Criteria:**
  - [ ] Visual indicator for mutual favorite relationships
  - [ ] Notification when someone favorites/unfavorites you
  - [ ] Different UI treatment for one-way vs mutual favorites
  - [ ] Favorites status exchange through secure channel
- **Priority:** Should Have
- **Story Points:** 8

### Feature 4.2: Mentions and Notifications

#### User Stories:

**US-4.2.1** - @Mention System

- **As a** user
- **I want to** mention specific users in group conversations
- **So that** they are notified about messages directed at them
- **Acceptance Criteria:**
  - [ ] @nickname syntax in messages
  - [ ] Autocomplete for known nicknames
  - [ ] Visual highlighting of mentions
  - [ ] Special notification for mentions
- **Priority:** Should Have
- **Story Points:** 8

### Feature 4.3: User Blocking

#### User Stories:

**US-4.3.1** - Block Unwanted Users

- **As a** user
- **I want to** block users who send unwanted messages
- **So that** I don't receive their communications
- **Acceptance Criteria:**
  - [ ] Block user option in contact menu
  - [ ] Blocked users' messages filtered out
  - [ ] Block status persists across sessions
  - [ ] Unblock functionality available
- **Priority:** Should Have
- **Story Points:** 5

---

## 🌐 EPIC 5: Cross-Transport Communication

_Enable communication through multiple transport protocols_

### Feature 5.1: Nostr Integration

#### User Stories:

**US-5.1.1** - Nostr Identity Generation

- **As a** user
- **I want** a Nostr identity derived from my BitChat keys
- **So that** I can communicate with favorites over the internet
- **Acceptance Criteria:**
  - [ ] Nostr keypair derived from Noise static key using BIP-32
  - [ ] Consistent npub across app reinstalls
  - [ ] Nostr identity never leaves device
  - [ ] Integration with favorites system
- **Priority:** Nice to Have
- **Story Points:** 13

**US-5.1.2** - NIP-17 Private Messages

- **As a** user
- **I want** to send encrypted messages to mutual favorites via Nostr
- **So that** I can communicate when out of Bluetooth range
- **Acceptance Criteria:**
  - [ ] NIP-17 gift-wrapped private messages
  - [ ] Ephemeral key usage for metadata privacy
  - [ ] Automatic transport selection (Bluetooth preferred)
  - [ ] Only available for mutual favorites
- **Priority:** Nice to Have
- **Story Points:** 21

### Feature 5.2: Transport Routing

#### User Stories:

**US-5.2.1** - Intelligent Transport Selection

- **As a** user
- **I want** messages automatically routed through the best available transport
- **So that** my messages are delivered reliably
- **Acceptance Criteria:**
  - [ ] Bluetooth mesh preferred when available
  - [ ] Nostr fallback for mutual favorites
  - [ ] Transparent switching between transports
  - [ ] Delivery status across all transports
- **Priority:** Nice to Have
- **Story Points:** 13

---

## 🛡️ EPIC 6: Privacy & Security

_Implement privacy protection and security features_

### Feature 6.1: Message Privacy

#### User Stories:

**US-6.1.1** - Message Padding

- **As a** user
- **I want** my message sizes to be obscured
- **So that** observers cannot infer information from message length
- **Acceptance Criteria:**
  - [ ] PKCS#7-style padding to standard block sizes
  - [ ] Random padding bytes for traffic analysis resistance
  - [ ] Automatic padding for all messages
  - [ ] Configurable block sizes (256, 512, 1024, 2048 bytes)
- **Priority:** Should Have
- **Story Points:** 5

**US-6.1.2** - Ephemeral Messages

- **As a** user
- **I want** messages to exist only in memory
- **So that** they cannot be recovered from device storage
- **Acceptance Criteria:**
  - [ ] No persistent message storage by default
  - [ ] Messages cleared on app termination
  - [ ] Optional message history limit (e.g., 1337 messages)
  - [ ] Clear message history command
- **Priority:** Must Have
- **Story Points:** 3

### Feature 6.2: Emergency Features

#### User Stories:

**US-6.2.1** - Emergency Data Wipe

- **As a** user
- **I want to** quickly erase all app data in emergency situations
- **So that** my communications cannot be compromised
- **Acceptance Criteria:**
  - [ ] Triple-tap on app icon/logo triggers wipe
  - [ ] Immediate clearing of all messages, keys, and settings
  - [ ] App reset to initial state
  - [ ] No recovery possible after wipe
- **Priority:** Should Have
- **Story Points:** 5

### Feature 6.3: Security Monitoring

#### User Stories:

**US-6.3.1** - Security Event Logging

- **As a** developer
- **I want** security events logged for debugging
- **So that** security issues can be identified and resolved
- **Acceptance Criteria:**
  - [ ] Secure logging framework (no sensitive data)
  - [ ] Handshake failures and security events logged
  - [ ] Rate limiting for security events
  - [ ] Log rotation and size limits
- **Priority:** Should Have
- **Story Points:** 8

---

## 💬 EPIC 7: User Interface & Experience

_Provide intuitive and accessible user interface_

### Feature 7.1: Chat Interface

#### User Stories:

**US-7.1.1** - Public Chat View

- **As a** user
- **I want** a main chat view for public conversations
- **So that** I can participate in group discussions
- **Acceptance Criteria:**
  - [ ] Scrollable message list with timestamps
  - [ ] Message input field with send button
  - [ ] Nickname display for each message
  - [ ] Visual indicators for message status
- **Priority:** Must Have
- **Story Points:** 8

**US-7.1.2** - Private Chat Interface

- **As a** user
- **I want** separate views for private conversations
- **So that** I can have one-on-one discussions
- **Acceptance Criteria:**
  - [ ] Slide-over interface for private chats
  - [ ] Encryption status indicator
  - [ ] Unread message notifications
  - [ ] Easy switching between public and private views
- **Priority:** Must Have
- **Story Points:** 13

### Feature 7.2: Command System

#### User Stories:

**US-7.2.1** - IRC-Style Commands

- **As a** user
- **I want** to use commands for advanced features
- **So that** I can efficiently manage conversations and settings
- **Acceptance Criteria:**
  - [ ] `/nick <name>` - Change nickname
  - [ ] `/msg <user> <message>` - Send private message
  - [ ] `/who` - List connected peers
  - [ ] `/clear` - Clear message history
  - [ ] `/help` - Show available commands
  - [ ] Command autocomplete and validation
- **Priority:** Should Have
- **Story Points:** 8

### Feature 7.3: Peer Management

#### User Stories:

**US-7.3.1** - Peer List View

- **As a** user
- **I want** to see all available peers
- **So that** I can start conversations and manage relationships
- **Acceptance Criteria:**
  - [ ] Sidebar with peer list
  - [ ] Connection status indicators
  - [ ] Favorite/unfavorite actions
  - [ ] Peer detail view with fingerprint
- **Priority:** Must Have
- **Story Points:** 8

---

## 📱 EPIC 8: Platform Integration

_Integrate with platform-specific features_

### Feature 8.1: Share Extension

#### User Stories:

**US-8.1.1** - Content Sharing

- **As a** user
- **I want** to share content from other apps to BitChat
- **So that** I can easily discuss links and information
- **Acceptance Criteria:**
  - [ ] Share extension for URLs and text
  - [ ] Automatic app opening after sharing
  - [ ] Content formatting for chat display
  - [ ] Support for web page titles and metadata
- **Priority:** Nice to Have
- **Story Points:** 13

### Feature 8.2: Background Operation

#### User Stories:

**US-8.2.1** - Background Connectivity

- **As a** user
- **I want** the app to maintain connections in the background
- **So that** I can receive messages when not actively using the app
- **Acceptance Criteria:**
  - [ ] Background Bluetooth operations (platform permitting)
  - [ ] Message notifications when app is backgrounded
  - [ ] Efficient battery usage in background mode
  - [ ] Automatic reconnection on app resume
- **Priority:** Should Have
- **Story Points:** 13

---

## 🔧 EPIC 9: Platform-Specific Features

_Handle platform differences and capabilities_

### Feature 9.1: iOS/macOS Features

#### User Stories:

**US-9.1.1** - iOS Integration

- **As an** iOS user
- **I want** native iOS features to work correctly
- **So that** the app feels integrated with my device
- **Acceptance Criteria:**
  - [ ] Keychain integration for secure storage
  - [ ] iOS notification system integration
  - [ ] Background app refresh support
  - [ ] iOS-specific UI guidelines compliance
- **Priority:** Must Have (iOS)
- **Story Points:** 8

### Feature 9.2: Android Features

#### User Stories:

**US-9.2.1** - Android Integration

- **As an** Android user
- **I want** native Android features to work correctly
- **So that** the app feels integrated with my device
- **Acceptance Criteria:**
  - [ ] Android Keystore integration
  - [ ] Android notification channels
  - [ ] Background service limitations handling
  - [ ] Material Design compliance
- **Priority:** Must Have (Android)
- **Story Points:** 8

### Feature 9.3: Windows/Desktop Features

#### User Stories:

**US-9.3.1** - Desktop Integration

- **As a** desktop user
- **I want** desktop-optimized features
- **So that** I can efficiently use the app on larger screens
- **Acceptance Criteria:**
  - [ ] Resizable window with proper layouts
  - [ ] Keyboard shortcuts for common actions
  - [ ] System tray integration (Windows)
  - [ ] Multi-window support where appropriate
- **Priority:** Nice to Have
- **Story Points:** 13

---

## 📊 Non-Functional Requirements

### Performance Requirements

- **Response Time**: Message delivery within 2 seconds under normal conditions
- **Throughput**: Support for 50+ simultaneous connections (platform dependent)
- **Resource Usage**: Maximum 100MB RAM usage during normal operation
- **Battery Life**: Minimal impact on device battery (adaptive duty cycling)

### Security Requirements

- **Encryption**: All private messages must use authenticated encryption
- **Key Management**: Cryptographic keys stored in platform secure storage
- **Authentication**: Mutual authentication for all encrypted sessions
- **Privacy**: No persistent storage of message content by default

### Compatibility Requirements

- **iOS**: iOS 14.0+ (to match current app requirements)
- **Android**: Android API 23+ (Android 6.0)
- **macOS**: macOS 11.0+ (to match current app requirements)
- **Windows**: Windows 10 version 1809+
- **Bluetooth**: Bluetooth LE 4.0+ required

### Reliability Requirements

- **Availability**: App remains functional with 99% uptime when Bluetooth is available
- **Data Integrity**: Message corruption rate < 0.1%
- **Error Recovery**: Automatic recovery from connection failures
- **Graceful Degradation**: Continue operation when some features unavailable

### Scalability Requirements

- **Peer Count**: Support up to 100 known peers simultaneously
- **Message History**: Configurable limit (default 1337 messages)
- **Network Size**: Function in mesh networks up to 50 nodes
- **Geographic Range**: No inherent limitations (depends on mesh density)

---

## 🧪 Acceptance Testing Framework

### Definition of Done

For each user story to be considered complete:

- [ ] All acceptance criteria met
- [ ] Unit tests written and passing (minimum 80% coverage)
- [ ] Integration tests written for cross-component interactions
- [ ] Manual testing completed on target platforms
- [ ] Security review completed for security-related features
- [ ] Documentation updated
- [ ] Code review completed and approved

### Testing Strategy

1. **Unit Testing**: Individual component testing with mocks
2. **Integration Testing**: Service-to-service interaction testing
3. **End-to-End Testing**: Full user workflow testing
4. **Security Testing**: Penetration testing for security features
5. **Performance Testing**: Load testing under various conditions
6. **Compatibility Testing**: Cross-platform functionality verification

---

## 🎯 Success Metrics

### Technical Metrics

- **Code Coverage**: Minimum 95% unit test coverage
- **Performance**: Message delivery latency < 2 seconds
- **Reliability**: Connection success rate > 95%
- **Security**: Zero critical security vulnerabilities

### User Experience Metrics

- **Ease of Use**: New users can send first message within 2 minutes
- **Feature Discovery**: 80% of users discover key features within first session
- **Error Rate**: User-facing errors < 1% of operations

### Business Metrics

- **Feature Parity**: 100% feature compatibility with Swift version
- **Platform Coverage**: Support for iOS, Android, macOS, Windows
- **Maintenance**: Reduced maintenance overhead through shared codebase

---

_This requirements document serves as the definitive specification for the BitChat MAUI migration. All development work should reference these requirements to ensure complete feature parity with the original iOS/macOS application._
