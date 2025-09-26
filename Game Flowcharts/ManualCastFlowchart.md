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