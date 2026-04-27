# Sprint 1 Plan — Spell Arena Multiplayer

**Sprint Duration:** February 1, 2026 – March 1, 2026 (4 weeks)
**Developer:** Joey Ampfer

---

## Sprint 1 Goal

Establish the core networking foundation and prove out the end-to-end multiplayer combat loop: two players connect, spawn into the arena, cast networked projectile spells at each other, deal synchronized damage, and see health update on both clients.

---

## Sprint Backlog

### Goal 1: Network Foundation and Setup

| ID  | Task                                                        | Status |
| --- | ----------------------------------------------------------- | ------ |
| S1.1 | Install and configure Netcode for GameObjects package      | Done   |
| S1.2 | Install Facepunch.Steamworks SDK                           | Done   |
| S1.3 | Create multiplayer scene with NetworkManager               | Done   |
| S1.4 | Implement GUI connection system (Host/Client/Server) for dev testing | Done |
| S1.5 | Establish dual-instance local testing workflow              | Done   |

### Goal 2: Player Spawning and Synchronization

| ID  | Task                                                        | Status |
| --- | ----------------------------------------------------------- | ------ |
| S1.6 | Create `GameSpawner` — server-authoritative player spawning with character selection and spawn points | Done |
| S1.7 | Create networked player prefab with `NetworkObject` and `NetworkTransform` | Done |
| S1.8 | Implement `PlayerNetworkSetup` — local vs. remote camera and audio listener management | Done |
| S1.9 | Implement `NetworkGameManager` singleton for player registration and opponent assignment | Done |

### Goal 3: Core Networked Combat Proof-of-Concept

| ID   | Task                                                        | Status |
| ---- | ----------------------------------------------------------- | ------ |
| S1.10 | Create `NetworkSpellCaster` with `ServerRpc` spell casting pipeline | Done |
| S1.11 | Create `NetworkSpellController` with ownership-gated input  | Done |
| S1.12 | Implement `NetworkProjectileBase` with server-authoritative lifecycle and despawn | Done |
| S1.13 | Add networked `Cast()` overload to `ProjectileSpellBehavior` using `NetworkObject.Spawn()` | Done |
| S1.14 | Implement impact VFX synchronization via `OnNetworkDespawn` (explosion visible on all clients) | Done |
| S1.15 | Implement networked health with `NetworkVariable` — server-authoritative damage, synced to all clients | Done |

---

## Requirement Traceability

| Sprint Task | Project Requirement | Coverage |
| ----------- | ------------------- | -------- |
| S1.6, S1.7, S1.8 | R2.1 — Player positions visible in real-time | Partial (position sync via NetworkTransform) |
| S1.10–S1.14 | R3.2 — Spell projectiles visible on all clients | Complete for projectile type |
| S1.15 | R4.1 — Deal damage to remote players with spells | Partial (one spell type, server-authoritative) |
| S1.15 | R4.2 — Receive damage from remote players' spells | Partial (health syncs via NetworkVariable) |
| S1.15 | R4.3 — Damage calculation is authoritative | Complete (host computes damage) |

---

## Burndown

| Metric             | Value |
| ------------------ | ----- |
| Tasks Planned      | 15    |
| Tasks Completed    | 15    |
| Completion Rate    | 100%  |

```
Tasks Remaining
15 |X
   | X
10 |  X
   |   X
   |    X
 5 |      X
   |       X
   |         X
 0 |__________X___
   W1  W2  W3  W4
```

---

## Sprint 1 Deliverables

1. **Multiplayer Scene** — Dedicated scene with `NetworkManager`, `GameSpawner`, and spawn points configured.
2. **Networked Player Prefab** — Two character variants (Sapphi, Pico) with `NetworkObject`, `NetworkTransform`, `PlayerNetworkSetup`, `NetworkSpellCaster`, and `NetworkSpellController`.
3. **Networked Projectile Prefab** — Projectile with `NetworkProjectileBase` supporting server-authoritative spawning, movement sync, collision, and despawn with impact VFX on all clients.
4. **Server-Authoritative Health** — `NetworkVariable<float>` health on `NetworkSpellCaster`, damage applied on server via projectile collision, health value synced to all clients.
5. **Development Testing Workflow** — GUI buttons for Host/Client/Server connection; local testing with two Unity editor instances.

---

## Sprint 1 Retrospective Notes

- Focused on establishing the networking architecture foundation before building higher-level features (lobby, matchmaking, full combat loop).
- Steamworks SDK is installed but testing uses Unity Transport locally for faster iteration; Steam transport integration deferred to Sprint 2.
- Proved out the most technically complex piece first — server-authoritative projectile spawning with `NetworkObject.Spawn()` and synchronized despawn/VFX.
- Single-player spell system (`SpellCaster`, `SpellBehavior`, `SpellBook`) was refactored into networked equivalents (`NetworkSpellCaster`, networked `Cast()` overloads) using the strategy pattern, preserving the original single-player code path.

---

## Sprint 2 Preview

Remaining project requirements to be addressed in Sprint 2 (Mar 1 – May 1):

- **R1.1–R1.4:** Steam lobby and matchmaking (replace dev GUI with Steam lobby flow)
- **R2.2–R2.3:** Animation and health bar UI synchronization
- **R3.1, R3.3:** Gesture display sync, shield/aura effects
- **R4.1–R4.2:** Expand damage to all spell types
- **R5.1–R5.3:** Win/lose conditions, rematch flow
- **R6.1–R6.3:** Disconnect handling, host migration

