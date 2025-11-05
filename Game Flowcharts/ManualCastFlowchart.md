##  2. Manual Cast Trigger (hands off, double sign, or key press)

* **Examples:**

  * Hands leave screen → sequence is submitted.
  * Double “I Love You” 🤟 or double fist ✊✊ = “Cast now.”
  * Press spacebar = “Cast now.”

* **Pros:**

  * Gives players control, lets them cancel.
  * Allows **longer combos** (since recognition waits until explicit cast signal).
  * Fits RPG progression (you “know” which spell you’re trying to cast).

* **Cons:**

  * Slightly slower.
  * Needs clear tutorial/UI feedback (“Ready to cast! Do X to finish”).

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