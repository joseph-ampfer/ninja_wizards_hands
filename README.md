# ninja_wizards_hands

This is just documentation for the game.

Download a demo for Windows and try it out:  https://drive.google.com/file/d/1dMynKtngsOCwr-KwRBChzfMUlerCgxel/view?usp=drive_link


---

```mermaid
graph TD

    A[Startup] --> B[Load Assets]
    B --> C[Main Menu]
    C -->|Start Game| D[Load Game Scene]

    D --> E[Initialize Camera Feed]
    E --> F[Start Gesture Recognition Loop]

    F --> G{Hands Detected?}
    G -->|No| H[Idle / Wait]
    G -->|One Hand| H
    G -->|Two Hands| I[Track Gesture Sequence]

    H --> F

    I --> J[Log Recognized Gestures]
    J --> K[Display Gestures on Screen]
    K --> L{Manual Cast Trigger?}

    L -->|Hands Leave Screen| M[Submit Sequence]
    L -->|Double 🤟 or ✊✊| M
    L -->|Press Spacebar| M
    L -->|No| I

    M --> N{Spell Match?}
    N -->|No| I
    N -->|Yes| O[Cast Spell]

    O --> P[Clear / Reset Gesture Buffer]
    P --> F
```
---

```mermaid
classDiagram

    class GestureRecognizerRunner {
        <<Subject/Publisher>>
        +UnityEvent~string, string~ OnGestureRecognized
        -ProcessResult(GestureRecognizerResult) void
    }
    
    class SpellManager {
        <<Observer/Subscriber>>
        +OnGestureRecognized(string leftStr, string rightStr) void
        -HandleGesture(GestureLabel, GestureLabel) void
        -ConcurrentQueue gestureQueue
    }
    
    class MediaPipeCamera {
        <<External System>>
        Detects hand gestures
    }
    
    class GestureMapper {
        <<Utility>>
        +ToEnum(string) GestureLabel
    }
    
    MediaPipeCamera --> GestureRecognizerRunner : feeds frames
    GestureRecognizerRunner --> SpellManager : fires event
    SpellManager ..> GestureMapper : converts strings
    
    note for GestureRecognizerRunner "Line 278:\nOnGestureRecognized?.Invoke(leftGesture, rightGesture)"
    
    note for SpellManager "Line 43:\npublic void OnGestureRecognized(string leftStr, string rightStr)\nConnected via Unity Inspector"

```

---
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
        L --> M[SpellCaster.Cast]
        L --> N[GestureUI.ShowGesture]
        L --> O[GameObject/Transform access]
    end
    
    style G fill:#C34A6B
    style H fill:#CC3333
    style J fill:#4CAF50
    style F fill:#D2B48C
    style L fill:#4682B4
```

---

![](images/manual-cast-flow.png)

---

1. Core Spell System (Strategy Pattern)
![](images/spell-sytem.png)

---

2. Spell Casting System
![](images/spell-casting-system.png)

---

3. Gesture Recognition System
![](images/gesture-recognition-system.png)

---

4. UI Components
![](images/ui-system.png)

---

5. Combat & Projectile System
![](images/combat-projectile-system.png)

---

6. Overall System Architecture
![](images/overall-system-archit.png)

---

## **Key Patterns**

### **Strategy Pattern ⭐ (Core Pattern):**
![](images/strategy-pattern.png)

Location: Spell.cs holds a SpellBehavior reference, and different behaviors implement different casting logic.
Benefits:
- Easy to add new spell types without modifying existing code
- Each spell behavior is isolated and testable
- Runtime behavior switching possible

---

### **Observer Pattern:**
![](images/observer-pattern.png)

---

#### Detailed observer flow
![](images/observer-swim.png)

---

### **Facade Pattern**
`SpellCaster` acts as a facade, providing a simplified interface to the complex spell-casting subsystem:
![](images/facade-pattern-clients.png)

---

### **Adapter Pattern**
GestureMapper adapts MediaPipe strings to internal enums.
![](images/adapter-pattern.png)


---

### **Flyweight Pattern**
ScriptableObjects share data across multiple instances.
![](images/flyweight-pattern.png)

---

### **Component Pattern (Unity):**
Multiple components attached to same GameObject communicate via GetComponent<T>():
- SpellCaster ↔ Animator
- SpellCaster ↔ ShieldComponent
- ProjectileBase ↔ Rigidbody
- AISpellManager2 requires SpellCaster

### **Manager/Service Pattern:**
- **SpellManager** orchestrates gesture input → spell casting
- **AISpellManager/AISpellManager2** orchestrate AI spell casting
- **CameraEffects** provides camera shake service

---

This architecture follows a clean separation between:
1. **Data** (Spell, SpellBook as ScriptableObjects)
2. **Behavior** (SpellBehavior strategy pattern)
3. **Execution** (SpellCaster, AISpellManager)
4. **Input** (SpellManager, GestureRecognizerRunner)
5. **UI** (GestureUI, GestureUIBuffer, EnemyGestureDisplay)

---

![](images/threads.png)


---