## Project Plan

### Timeline Overview

**Project Duration:** February 1, 2026 – May 1, 2026 (13 weeks)

```
Feb                        Mar                        Apr                    May
|----|----|----|----|----|----|----|----|----|----|----|----|----| 
  W1   W2   W3   W4   W5   W6   W7   W8   W9  W10  W11  W12  W13
  ├─ Foundation ─┤├─ Lobby ─┤├─ Player ─┤├─ Spells ─┤├─ Combat ─┤├ Polish ┤
```

---

### Phase 1: Foundation & Setup (Weeks 1-2)
**Feb 1 – Feb 14**

| Task | Description | Deliverable |
| ---- | ----------- | ----------- |
| 1.1 | Install Netcode for GameObjects package | Package imported, no compile errors |
| 1.2 | Install Facepunch.Steamworks via NuGet/Unity | Steam SDK integrated |
| 1.3 | Configure Steam App ID for development | steamclient.dll loads without errors |
| 1.4 | Create NetworkManager prefab with SteamTransport | NetworkManager spawns on scene load |
| 1.5 | Test basic client-host connection locally | Two Unity instances connect via Steam |
| 1.6 | Set up development testing workflow | Document dual-instance testing process |

**Milestone 1 (Feb 14):** ✅ Two Unity builds can connect via Steam P2P transport

---

### Phase 2: Steam Lobby & Matchmaking (Weeks 3-4)
**Feb 15 – Feb 28**

| Task | Description | Deliverable |
| ---- | ----------- | ----------- |
| 2.1 | Create `LobbyManager` class with Steam lobby API | Can create/destroy lobbies |
| 2.2 | Implement lobby creation UI | Button creates lobby, shows lobby code |
| 2.3 | Implement Steam friend invite system | Invite sends via Steam overlay |
| 2.4 | Implement lobby join via invite acceptance | Accepting invite joins lobby |
| 2.5 | Display connected players in lobby UI | Player list updates in real-time |
| 2.6 | Implement ready toggle and start game button | Host can start when all ready |
| 2.7 | Write acceptance tests for R1.1–R1.4 | All lobby requirements pass |

**Milestone 2 (Feb 28):** ✅ Players can create lobby, invite friends, ready up, and start match

---

### Phase 3: Player Synchronization (Weeks 5-6)
**Mar 1 – Mar 14**

| Task | Description | Deliverable |
| ---- | ----------- | ----------- |
| 3.1 | Create `NetworkedPlayer` prefab with NetworkObject | Player spawns on both clients |
| 3.2 | Add NetworkTransform for position/rotation sync | Movement visible on remote client |
| 3.3 | Add NetworkAnimator for animation sync | Animations play on all clients |
| 3.4 | Create `NetworkedHealth` component | Health syncs as NetworkVariable |
| 3.5 | Implement health bar UI for all players | Remote player health visible |
| 3.6 | Tune network tick rate for responsiveness | < 100ms perceived latency |
| 3.7 | Write acceptance tests for R2.1–R2.3 | All player sync requirements pass |

**Milestone 3 (Mar 14):** ✅ Players see each other move, animate, and display health in real-time

---

### Phase 4: Networked Spell Casting (Weeks 7-8)
**Mar 15 – Mar 28**

| Task | Description | Deliverable |
| ---- | ----------- | ----------- |
| 4.1 | Create `NetworkedSpellCaster` with ServerRpc | Spell cast request sent to host |
| 4.2 | Implement server-side spell validation | Host validates and spawns spell |
| 4.3 | Create `NetworkedProjectile` prefab | Projectiles sync across network |
| 4.4 | Sync gesture display to remote clients | Opponent sees your gesture indicator |
| 4.5 | Implement networked shield/aura effects | Visual effects appear on all clients |
| 4.6 | Handle spell cooldowns across network | Cooldowns enforced server-side |
| 4.7 | Write acceptance tests for R3.1–R3.3 | All spell casting requirements pass |

**Milestone 4 (Mar 28):** ✅ Spells cast by one player are visible and functional for all players

---

### Phase 5: Combat & Damage System (Weeks 9-10)
**Mar 29 – Apr 11**

| Task | Description | Deliverable |
| ---- | ----------- | ----------- |
| 5.1 | Implement server-authoritative damage calculation | Host computes all damage |
| 5.2 | Add ClientRpc for damage feedback (VFX, sound) | Hit feedback plays on all clients |
| 5.3 | Implement projectile collision with NetworkedHealth | Projectiles damage remote players |
| 5.4 | Sync damage numbers/floating text | Damage popups visible to all |
| 5.5 | Add spell-specific damage modifiers | Different spells deal correct damage |
| 5.6 | Write acceptance tests for R4.1–R4.3 | All combat requirements pass |

**Milestone 5 (Apr 11):** ✅ Full combat loop works—players can damage each other with spells

---

### Phase 6: Game State & Win Conditions (Weeks 11-12)
**Apr 12 – Apr 25**

| Task | Description | Deliverable |
| ---- | ----------- | ----------- |
| 6.1 | Implement death/defeat state sync | Defeated player shown on all clients |
| 6.2 | Create win/lose UI screens | Correct result displayed to each player |
| 6.3 | Implement rematch functionality | Both players restart same lobby |
| 6.4 | Implement return to lobby option | Players can exit to main menu |
| 6.5 | Handle disconnect during match | Notification shown, graceful exit |
| 6.6 | Implement host migration or session cleanup | No orphaned sessions |
| 6.7 | Write acceptance tests for R5.1–R5.3, R6.1–R6.3 | All game state requirements pass |

**Milestone 6 (Apr 25):** ✅ Complete match flow from lobby → combat → victory → rematch/exit

---

### Phase 7: Polish & Final Testing (Week 13)
**Apr 26 – May 1**

| Task | Description | Deliverable |
| ---- | ----------- | ----------- |
| 7.1 | Full integration testing on separate machines | Works outside local network |
| 7.2 | Performance profiling and optimization | Stable 60fps with networking |
| 7.3 | Bug fixing and edge case handling | No critical bugs |
| 7.4 | Record demo video | Video showcasing all features |
| 7.5 | Final documentation update | README complete, demo linked |

**Final Milestone (May 1):** ✅ Project complete, demo video recorded, ready for presentation

---

### Milestone Summary

| Milestone | Date | Deliverable | Requirements |
| --------- | ---- | ----------- | ------------ |
| M1 | Feb 14 | Network foundation working | — |
| M2 | Feb 28 | Lobby & matchmaking complete | R1.1–R1.4 |
| M3 | Mar 14 | Player synchronization complete | R2.1–R2.3 |
| M4 | Mar 28 | Networked spell casting complete | R3.1–R3.3 |
| M5 | Apr 11 | Combat & damage system complete | R4.1–R4.3 |
| M6 | Apr 25 | Game state & win conditions complete | R5.1–R5.3, R6.1–R6.3 |
| M7 | May 1 | Polish, testing, demo video | All 19 requirements |

---

### Risk Mitigation

| Risk | Impact | Mitigation Strategy |
| ---- | ------ | ------------------- |
| Steam transport issues | High | Fallback to Unity Transport with direct IP |
| NAT traversal failures | Medium | Use Steam Relay (enabled by default in Facepunch) |
| Gesture sync latency | Medium | Sync gesture state, not raw input; use prediction |
| Scope creep | High | Strict feature freeze after Week 10 |
| Testing bottleneck | Medium | Recruit 1-2 playtesters for Weeks 11-13 |

---

### Weekly Time Commitment

| Activity | Hours/Week |
| -------- | ---------- |
| Implementation | 8-10 |
| Testing & debugging | 2-3 |
| Documentation | 1-2 |
| **Total** | **11-15** |

---