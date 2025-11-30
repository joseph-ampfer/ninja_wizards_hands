# ninja_wizards_hands

This is just documentation for the game.

Download a demo for Windows and try it out:  https://drive.google.com/file/d/1dMynKtngsOCwr-KwRBChzfMUlerCgxel/view?usp=drive_link



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