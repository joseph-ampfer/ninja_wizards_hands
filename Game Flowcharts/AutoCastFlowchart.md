##  1. Auto-cast as soon as a valid spell is detected

* **Pros:**

  * Very fluid and fast — players can “discover” spells by accident.
  * Fits a “chaotic wizard duel” vibe.
* **Cons:**

  * Hard to cancel mistakes.
  * Limits more complex combos (you’d need to filter “partial matches”).
  * Doesn’t mesh well with unlock/paywall design (since players may fire unknown spells).

---

```mermaid

---
title: Auto-cast as soon as a valid spell is detected
---

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
    K --> L{Spell Match?}

    L -->|No| I
    L -->|Yes| M[Cast Spell]

    M --> N[Clear / Reset Gesture Buffer]
    N --> F
```