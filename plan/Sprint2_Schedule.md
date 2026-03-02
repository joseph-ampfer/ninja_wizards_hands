# Sprint 2 Schedule and Milestones — Spell Arena Multiplayer

**Sprint Duration:** March 1, 2026 – May 1, 2026 (9 weeks)
**Developer:** Joey Ampfer

---

## Sprint 2 Goals

| # | Feature | Requirements | Description |
|---|---------|-------------|-------------|
| 1 | Steam Lobby & Matchmaking | R1.1–R1.4 | Replace dev GUI with Steam lobby creation, friend invites, ready-up, and match start |
| 2 | Full Player Synchronization | R2.2–R2.3, R3.1 | Animation sync, health bar UI visible to all, gesture display on remote clients |
| 3 | Expanded Networked Spells | R3.3 | Network all spell types (AoE, shields, auras), cooldowns |
| 4 | Game Flow & Resilience | R5.1–R5.3, R6.1–R6.3 | Win/lose conditions, rematch, disconnect handling |

---

## Sprint 2 Metrics

| Metric | Value |
|--------|-------|
| Features Planned | 4 |
| Requirements Planned | 14 |

---

## Week-by-Week Schedule

### Week 5 (Mar 1–7)
- Create `LobbyManager` class with Steam lobby API
- Implement lobby creation UI (button creates lobby, shows lobby code)
- Begin Steam friend invite system

### Week 6 (Mar 8–14)
- Implement lobby join via invite acceptance
- Display connected players in lobby UI
- Implement ready toggle and start game button
- **Milestone: Steam lobby working (R1.1–R1.4)**

### Week 7 (Mar 15–21)
- Add NetworkAnimator for animation sync (R2.2)
- Implement health bar UI visible to all players (R2.3)
- Sync gesture display to remote clients (R3.1)

### Week 8 (Mar 22–28)
- Network AoE spell behavior
- Network shield/aura visual effects (R3.3)
- Implement networked spell cooldowns
- **Milestone: All spells networked (R3.1, R3.3)**

### Week 9 (Mar 29 – Apr 4)
- Expand damage system to all spell types (R4.1–R4.2 full coverage)
- Implement server-authoritative damage for AoE and shield interactions
- Add damage feedback VFX/sound via ClientRpc

### Week 10 (Apr 5–11)
- Implement death/defeat state sync (R5.1)
- Create win/lose UI screens (R5.2)
- **Milestone: Full combat + win conditions (R5.1–R5.2)**

### Week 11 (Apr 12–18)
- Implement rematch functionality (R5.3)
- Implement return to lobby option
- Handle disconnect during match — notification shown (R6.1)

### Week 12 (Apr 19–25)
- Implement return to lobby after opponent leaves (R6.2)
- Implement host migration or graceful session cleanup (R6.3)
- **Milestone: Disconnect handling + polish (R6.1–R6.3)**

### Week 13 (Apr 26 – May 1)
- Full integration testing on separate machines
- Performance profiling and bug fixes
- Record demo video
- Final documentation update
- **Milestone: Demo video + final submission**

---

## Milestone Summary

| Milestone | Target Date | Requirements |
|-----------|-------------|-------------|
| Steam lobby working | Mar 14 | R1.1–R1.4 |
| All spells networked | Mar 28 | R2.2–R2.3, R3.1, R3.3 |
| Full combat + win conditions | Apr 11 | R4.1–R4.2 expanded, R5.1–R5.2 |
| Disconnect handling + polish | Apr 25 | R5.3, R6.1–R6.3 |
| Demo video + final submission | May 1 | All 19 requirements |

---

## Requirement Mapping

| Requirement | Description | Target Week |
|-------------|-------------|-------------|
| R1.1 | Create a Steam lobby | 5–6 |
| R1.2 | Join friend's lobby via Steam invite | 5–6 |
| R1.3 | See connected players in lobby | 5–6 |
| R1.4 | Host starts match when all ready | 5–6 |
| R2.2 | See other players' animations synchronized | 7 |
| R2.3 | See other players' health values | 7 |
| R3.1 | See other players' gesture inputs displayed | 7 |
| R3.3 | See spell effects (shields, auras) on all players | 8 |
| R5.1 | See when an opponent is defeated | 10 |
| R5.2 | See a win/lose screen at match end | 10 |
| R5.3 | Rematch or return to lobby | 11 |
| R6.1 | See when an opponent disconnects | 11 |
| R6.2 | Return to lobby after opponent leaves | 12 |
| R6.3 | Handle host migration if host disconnects | 12 |

---

## Changes from Initial Plan

- Steam lobby (originally Phase 2, Weeks 3–4) moved to Sprint 2 Weeks 5–6 — Sprint 1 focused on proving core combat networking first
- Health/damage proof-of-concept (originally Phase 5) was pulled forward into Sprint 1, so Sprint 2 expands it to all spell types rather than building from scratch
- Unity Transport used for local dev testing; Steam transport integration happens in Sprint 2 alongside lobby work

---

## Key Dates

| Date | Event |
|------|-------|
| Mar 1 | Sprint 2 begins |
| Mar 14 | Steam lobby milestone |
| Mar 28 | All spells networked milestone |
| Apr 11 | Full combat + win conditions milestone |
| Apr 25 | Polish + disconnect handling milestone |
| May 1 | Demo video recorded, final submission |

---

## Deployment Goal

- NKU Celebration presentation
- Public download link for the game

