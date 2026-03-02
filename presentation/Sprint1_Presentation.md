---
marp: true
size: 4:3
paginate: true
title: Sprint 1 Review — Spell Arena Multiplayer
---

# Spell Arena

### Real-Time Multiplayer Gesture-Based Combat

**Sprint 1 Review**

Joey Ampfer
March 1, 2026

---

## Problem & Motivation

**The Problem:** Adding multiplayer to an existing gesture-controlled wizard combat game

**Why It's Hard:**
- Real-time state synchronization across clients
- Server-authoritative damage to prevent cheating
- Spell projectiles must be visible and consistent for all players
- Gesture recognition adds latency complexity

**My Approach:**
- **Netcode for GameObjects** for state sync and RPCs
- **Facepunch.Steamworks** for Steam P2P transport and matchmaking
- **Host-Client (P2P)** topology — no dedicated server needed

---

## Sprint 1 Goal

> Establish the core networking foundation and prove out the end-to-end multiplayer combat loop.

**Target:** Two players connect, spawn into the arena, cast networked projectile spells at each other, deal synchronized damage, and see health update on both clients.

---

## What Was Built

1. **Multiplayer Scene** — NetworkManager, GameSpawner, spawn points
2. **Networked Player Prefab** — Two character variants (Sapphi, Pico) with NetworkObject, NetworkTransform, NetworkSpellCaster
3. **Networked Projectile** — Server-authoritative spawn, movement sync, collision, impact VFX on all clients
4. **Server-Authoritative Health** — `NetworkVariable<float>` health, damage applied on server, synced to all clients via callback
5. **Dev Testing Workflow** — GUI Host/Client/Server buttons, dual Unity editor instances

---

## Architecture

```
 ┌──────────────┐    Netcode RPCs    ┌──────────────┐
 │  Host/Server │◄──────────────────►│    Client    │
 │              │   NetworkVariables  │              │
 └──────┬───────┘                    └──────┬───────┘
        │                                   │
   ServerRpc:                          Sees synced:
   - TrySpellServerRPC()              - Projectile spawn
   - TakeDamage() on server           - Health changes
   - NetworkObject.Spawn()            - Impact VFX
        │                                   │
   ClientRpc:                          NetworkVariable:
   - OnDamagedClientRpc()             - NetworkHealth
   - OnNetworkDespawn() VFX           - NetworkTransform
```

---

## Damage Flow

```
Client presses F
       │
       ▼
 ServerRpc: TrySpellServerRPC()
       │
       ▼
 Server spawns projectile (NetworkObject.Spawn)
       │
       ▼
 Projectile hits opponent (server-side collision)
       │
       ▼
 NetworkSpellCaster.TakeDamage() — server only
       │
       ▼
 NetworkVariable<float> health updated
       │
       ▼
 OnHealthChanged callback fires on ALL clients
 + OnDamagedClientRpc for hit feedback
```

---

## Sprint 1 Metrics

| Metric | Value |
|--------|-------|
| Lines of Code (new multiplayer) | ~1,196 across 8 .cs files |
| Features Planned | 3 |
| Features Completed | 3 |
| **Feature Burndown** | **100%** |
| Requirements Addressed | 5 (R2.1, R3.2, R4.1, R4.2, R4.3) |
| Requirements Completed | 5 |
| **Requirement Burndown** | **100%** |

---

## Burndown Chart

```
Tasks Remaining
15 |X
   | X
10 |   X
   |    X
   |      X
 5 |       X
   |        X
   |         X
 0 |__________X
   W1   W2   W3   W4
```

All 15 sprint tasks completed across 4 weeks.

---

## Sprint 1 Retrospective

### What Went Well
- Server-authoritative architecture established early — projectile spawning and damage are host-validated
- Proved the hardest technical piece first (networked spell casting with `NetworkObject.Spawn()`)
- Preserved single-player code path while adding networked overloads

### What Went Wrong
- Steam transport not yet tested — using Unity Transport locally
- Lobby/matchmaking deferred; still using manual Host/Client GUI buttons
- Spent time on architecture decisions that delayed visible feature work

### Improvement Plan
- Prioritize visible features earlier in Sprint 2
- Integrate Steam transport sooner to catch NAT issues early
- Set weekly mini-deadlines instead of end-of-sprint goals

---

## Weekly Progress

- **Week 1:** Installed Netcode for GameObjects and Facepunch.Steamworks SDK. Created multiplayer scene with NetworkManager. Set up GUI connection buttons and dual-instance testing.
- **Week 2:** Built GameSpawner for server-authoritative player spawning. Created networked player prefab with NetworkTransform. Implemented PlayerNetworkSetup for camera/audio management.
- **Week 3:** Created NetworkSpellCaster with ServerRpc pipeline. Implemented NetworkProjectileBase with server-authoritative lifecycle. Added networked Cast() to ProjectileSpellBehavior.
- **Week 4:** Implemented impact VFX sync via OnNetworkDespawn. Added NetworkVariable health with server-authoritative damage. End-to-end combat loop working.

---

## Sprint 2 Goals

1. **Steam Lobby & Matchmaking** — Replace dev GUI with Steam lobby creation, friend invites, ready-up, and match start (R1.1-R1.4)
2. **Full Player Synchronization** — Animation sync, health bar UI visible to all, gesture display on remote clients (R2.2-R2.3, R3.1)
3. **Expanded Networked Spells** — Network all spell types (AoE, shields, auras), cooldowns (R3.3)
4. **Game Flow & Resilience** — Win/lose conditions, rematch, disconnect handling (R5.1-R5.3, R6.1-R6.3)

---

## Sprint 2 Metrics

| Metric | Value |
|--------|-------|
| Features Planned | 4 |
| Requirements Planned | 14 |

---

## Sprint 2 Timeline

- **Week 5-6 (Mar 1-14):** Steam lobby integration, friend invites, ready-up UI
- **Week 7-8 (Mar 15-28):** Animation sync, health bar UI, gesture display, expand networked spells
- **Week 9-10 (Mar 29 - Apr 11):** Full combat loop — all spell types deal damage, win/lose conditions
- **Week 11-12 (Apr 12-25):** Disconnect handling, rematch flow, polish
- **Week 13 (Apr 26 - May 1):** Final testing, bug fixes, demo video

---

## Sprint 2 Key Dates

| Milestone | Date |
|-----------|------|
| Steam lobby working | Mar 14 |
| All spells networked | Mar 28 |
| Full combat + win conditions | Apr 11 |
| Disconnect handling + polish | Apr 25 |
| Demo video + final submission | May 1 |

---

## Changes from Initial Plan

- Deferred Steam lobby (Phase 2) to Sprint 2 — focused Sprint 1 on proving core combat networking
- Health/damage (Phase 5) pulled forward into Sprint 1 as proof-of-concept
- Using Unity Transport for local testing instead of Steam transport during development

---

## Learning with AI

### Topic 1: Networking Fundamentals

- Learned about **client/server authority** — the server validates all actions to prevent cheating
- Learned **RPCs (Remote Procedure Calls)**:
  - `[ServerRpc]` — client requests the server to run a function (e.g. "I want to cast a spell")
  - `[ClientRpc]` — server tells all clients to run a function (e.g. "play this animation")
- Studied **latency compensation** — client-side prediction, server event timestamps, fast-forwarding delayed events on clients

---

## Learning with AI (cont.)

### Topic 2: Unity Netcode for GameObjects

- **Spawning/Despawning NetworkObjects** — spawning creates a synced copy on every client; each client runs the code independently. Despawning triggers `OnNetworkDespawn` callback (used for explosion VFX)
- **NetworkVariables** — synced values across server/clients with `OnValueChanged` listener to react on every client (e.g. update health bar when health changes)
- **Refactoring single-player to multiplayer** — preserved existing SpellBehavior strategy pattern while adding networked `Cast()` overloads for `NetworkSpellCaster`

---

# Thank You

**Questions?**

