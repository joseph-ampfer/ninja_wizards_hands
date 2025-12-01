---
title: "System Architecture"
weight: 30
---

# System Architecture

## Overview

Wizards Game is built with a clean, maintainable architecture following best practices and established design patterns.

## High-Level Architecture

The system follows clean separation of concerns across five distinct layers:

1. **Data Layer** - Spell & SpellBook ScriptableObjects
2. **Behavior Layer** - SpellBehavior strategy implementations
3. **Execution Layer** - SpellCaster, AISpellManager
4. **Input Layer** - SpellManager, GestureRecognizerRunner
5. **UI Layer** - GestureUI, GestureUIBuffer, EnemyGestureDisplay

## Game Flow

The complete game flow from startup to spell casting:

```mermaid
graph TD
    A[Startup] --> B[Load Assets]
    B --> C[Main Menu]
    C -->|Start Game| D[Load Game Scene]
    D --> E[Initialize Camera Feed]
    E --> F[Start Gesture Recognition Loop]
    F --> G{Hands Detected?}
    G -->|Two Hands| I[Track Gesture Sequence]
    G -->|No/One Hand| H[Idle / Wait]
    H --> F
    I --> J[Log Recognized Gestures]
    J --> K[Display Gestures on Screen]
    K --> L{Manual Cast Trigger?}
    L -->|✊✊ or Spacebar| M[Submit Sequence]
    L -->|No| I
    M --> N{Spell Match?}
    N -->|Yes| O[Cast Spell]
    N -->|No| I
    O --> P[Clear Gesture Buffer]
    P --> F
```

## Manual Cast Flow

![Manual Cast Flow](../images/manual-cast-flow.png)

## System Components

### 1. Core Spell System (Strategy Pattern)

![Spell System](../images/spell-sytem.png)

The spell system uses the Strategy Pattern for maximum flexibility and extensibility.

**Benefits:**
- Easy to add new spell types without modifying existing code
- Each spell behavior is isolated and testable
- Runtime behavior switching possible

### 2. Spell Casting System

![Spell Casting System](../images/spell-casting-system.png)

The `SpellCaster` component acts as a facade, providing a simplified interface to the complex spell-casting subsystem.

### 3. Gesture Recognition System

![Gesture Recognition System](../images/gesture-recognition-system.png)

**Key Components:**
- **MediaPipeCamera** - External system for hand detection
- **GestureRecognizerRunner** - Publisher in Observer pattern
- **GestureMapper** - Adapter converting strings to enums
- **SpellManager** - Subscriber handling gesture events

### 4. UI Components

![UI System](../images/ui-system.png)

**Key UI Elements:**
- GestureUI - Current gesture display
- GestureUIBuffer - Bottom-left combo display
- EnemyGestureDisplay - Shows AI casting
- Health/Mana bars

### 5. Combat & Projectile System

![Combat and Projectile System](../images/combat-projectile-system.png)

**Key Classes:**
- ProjectileBase - Base projectile behavior
- SpellCaster - Facade for spell casting
- AISpellManager - AI opponent logic

### 6. Overall System Architecture

![Overall System Architecture](../images/overall-system-archit.png)

## Design Patterns

### Strategy Pattern ⭐ (Core Pattern)

![Strategy Pattern](../images/strategy-pattern.png)

**Location:** `Spell.cs` holds a `SpellBehavior` reference

Different behaviors implement different casting logic:
- ProjectileBehavior
- ShieldBehavior
- HealBehavior
- AOEBehavior

Each behavior encapsulates its own logic while sharing the common interface.

### Observer Pattern

![Observer Pattern](../images/observer-pattern.png)

Decouples gesture recognition from spell casting logic.

#### Detailed Observer Flow

![Observer Swim Diagram](../images/observer-swim.png)

Sequence diagram showing the complete event flow from gesture detection to spell execution.

### Facade Pattern

![Facade Pattern](../images/facade-pattern-clients.png)

`SpellCaster` acts as a facade, providing a simplified interface to the complex spell-casting subsystem.

**Benefits:**
- Simplified API for clients
- Hides internal complexity
- Centralized spell casting logic

### Adapter Pattern

![Adapter Pattern](../images/adapter-pattern.png)

**GestureMapper** adapts MediaPipe strings to internal enums.

**Example:**
```csharp
string "Closed_Fist" → GestureLabel.ClosedFist
string "Thumb_Up" → GestureLabel.ThumbUp
```

### Flyweight Pattern

![Flyweight Pattern](../images/flyweight-pattern.png)

**ScriptableObjects** share data across multiple instances.

**Benefits:**
- Memory efficient
- Shared spell data
- Easy to modify in Unity Inspector

### Component Pattern (Unity)

Multiple components attached to same GameObject communicate via `GetComponent<T>()`:

- SpellCaster ↔ Animator
- SpellCaster ↔ ShieldComponent
- ProjectileBase ↔ Rigidbody
- AISpellManager2 requires SpellCaster

**Unity Best Practice:** Composition over inheritance

### Manager/Service Pattern

**Central orchestration points:**

- **SpellManager** - Orchestrates gesture input → spell casting
- **AISpellManager/AISpellManager2** - Orchestrate AI spell casting
- **CameraEffects** - Provides camera shake service

**Benefits:** Centralized control, easy debugging, clear responsibilities

## Threading Architecture

![Threading Model](../images/threads.png)

**Key Challenge:** MediaPipe runs on worker thread, Unity API requires main thread

**Solution:** ConcurrentQueue pattern for thread-safe gesture data transfer

```mermaid
graph TB
    subgraph "Worker Thread MediaPipe"
        A[Camera Frame] --> B[MediaPipe Processing]
        B --> C[GestureRecognizer.RecognizeAsync]
        C --> D[OnGestureOutput Callback]
        D --> E[ProcessResult]
        E --> F[OnGestureRecognized.Invoke]
    end
    
    subgraph "Thread Boundary - ConcurrentQueue"
        F --> G[gestureQueue.Enqueue]
        G --> H{ConcurrentQueue<br/>Thread-Safe Buffer}
    end
    
    subgraph "Main Thread Unity"
        H --> I[Update Loop]
        I --> J[gestureQueue.TryDequeue]
        J --> K[HandleGesture]
        K --> L[Unity API Calls]
    end
```

## Architecture Summary

✅ **Clean separation of concerns** across 5 layers  
✅ **Event-driven architecture** for loose coupling  
✅ **Design patterns** for maintainability and extensibility  
✅ **Thread-safe** gesture recognition integration  
✅ **Scalable** spell system using Strategy pattern

**Result:** Maintainable, extensible, performant architecture

## Learn More

- [Technical Implementation Details](/technical/)
- [Gameplay Tutorial](/gameplay/)
- [GitHub Repository](https://github.com/joseph-ampfer/ninja_wizards_hands)

