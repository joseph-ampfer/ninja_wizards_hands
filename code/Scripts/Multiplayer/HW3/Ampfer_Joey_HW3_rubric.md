---
marp: true
size: 4:3
paginate: true
title: HW3
---

# HW3 Rubric

- Ready for the Sprint 1 presentation (S1P) by reserving your time slot; absense of S1P presentations without permission will result in 0 points for this assignment.
- All-or-nothing grading for each assignment (No partial points)
- Make sure to change the rubric file name correctly before submission
- Make sure you fill in all the (?) or ? marks with correct information (also check marks (V) for OK and (X) for not OK, or other requested information)

---

## Information

- Name: Joey Ampfer
- Email: ampferj2@nku.edu

---

## Summary

- Why my project is important to solve: Games like War of Wizards require a VR headset. Many people don’t have one, but desire a similar experience. VR Headsets are expensive. A novel way to play a game New skill mastery
- What is my approach to solve this problem: Using Netcode for GameObjects for state synchronization and Facepunch.Steamworks for Steam-based transport and matchmaking.
- What are the two Learning with AI topics:
  - Topic 1: Networking Fundamentals for Multiplayer Games
  - Topic 2: Unity Netcode for GameObjects

---

## 1. Share your progress with the class (10 pts)

**Make a Marp slide for your Sprint 1 presentation.**

- My presentation scheduled on : 3/2/26
- My presentation slide (pdf) is available at (GitHub URL): https://github.com/joseph-ampfer/ninja_wizards_hands/blob/main/presentation/Sprint1_Presentation.pdf 

**Points:** (10)/10

---

## 2. Reflect on Sprint 1

Mark for your three managers to evaluate your progress effectively: I will share this rubric with your managers when they ask it for your evaluation.

- (V) I have regularly updated my individual progress in Sprint 1.
- (V) I have regularly updated my project artifacts in Sprint 1.
- (V) I have regularly updated my learning with AI tools in Sprint 1.
- (V) If I have any changes, I have updated them for the Sprint so as not to surprise my managers.
- (V) My update is accessible so my managers can clearly and easily see the progress and learning in Sprint 1.

Make a simple summary of your individual progress in Sprint 1 (Use N/A if you have no progress in that area)

- Week 1: Installed Netcode for GameObjects and Facepunch.Steamworks SDK. Created multiplayer scene with NetworkManager. Set up GUI connection buttons and dual-instance testing.
- Week 2: Built GameSpawner for server-authoritative player spawning. Created networked player prefab with NetworkTransform. Implemented PlayerNetworkSetup for camera/audio management.
- Week 3: Created NetworkSpellCaster with ServerRpc pipeline. Implemented NetworkProjectileBase with server-authoritative lifecycle. Added networked Cast() to ProjectileSpellBehavior.
- Week 4: Implemented impact VFX sync via OnNetworkDespawn. Added NetworkVariable health with server-authoritative damage. End-to-end combat loop working.

- **Points:** (10)/10

---

## 3. Make ready for the final deployment in Sprint 2

Use this section to make it ready for your final and deployment in Sprint 2. This is also important for your managers to evaluate your progress effectively.

Choose your case by marking (V) and (X).

(X) Case 1: If you are still unclear about your final goal and presentation/deployment.

- (X) I will schedule a meeting with the professor to discuss my final goal and presentation/deployment for Sprint 2: ? (Date and time)
- Write down about your current concern: ?

(V) Case 2: If you know your final goal and presentation/deployment, you can narrow the scope of your project.

- In short, my final project goal for Sprint 2 is: Publish and present
  - It's OK if you narrow your scope down, change direction, or even change your project (in this case, you should talk with the professor about this). Just make sure your managers know about this so they can effectively evaluate your progress/learning/contribution in Sprints 1 and 2.
- In short, my final two learning goals with AI for Sprint 2 are: Networking Fundamentals for Multiplayer Games and Unity Netcode for GameObjects
  - The same rule applies; make sure your managers know about your goal for fair evaluation.
- My deployment goal for Sprint 2 is to use: NKU Celebration, Link for download online
  - This is important as it will be a part of your resume/portfolio
  - For example: NKU Celebration, Research Paper, Poster, Technical Report, Building a public web server, and any form of publication

- **Points:** (10)/10

---

## 4. Update and finalize schedules and milestones for Sprint 2

- (V) I have narrowed down (choose only one of the four choices and remove the rest).
- (V) I have updated my schedule and milestones for Sprint 2 on Canvas.
- (V) My schedule and milestones for Sprint 2 are easily accessible by anyone, including my managers.

This is my milestone and schedule for Sprint 2 copied from Canvas: 

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


- Project submissions deadline: 4/26/26
- Final Presentation (FP): 4/1/26
- HW4 Publication/Deployment deadline: 4/24/26

- **Points:** (10)/10

---

## 5. Make peer-review reports

- I am managing these students by checking their weekly progress and artifacts about their project and learning with AI tools:
  - Student 1: Parker Groneck
  - Student 2: Dillon Carpenter
  - Student 3: Josh Day
- (V) I will submit my peer review report in one week after the HW3 deadline.
- (V) I will evaluate my peers in a professional, fair, and honest manner; I will not be biased by my friendship or any other factors.
- (V) I will evaluate my peers to help them improve their projects and learning, not to punish them.
- (V) I will use the accompanying peer review rubric file for the evaluation.
- (V) I understand that neglecting peer evaluation may result in 0 points for my final project evaluation.

---

## Total Grading

| Assignment                                                | Points          | Comment |
| --------------------------------------------------------- | --------------- | ------- |
| Share your progress with the class                        | ( (10) / 10)     |         |
| Reflect on Sprint 1.                                      | ( (10) / 10)     |         |
| Make ready for the final deployment in Sprint 2           | ( (10) / 10)     |         |
| Update and finalize schedules and milestones for Sprint 2 | ( (10) / 10)     |         |
| **Total Points**                                          | **( (40) / 40)** |         |

## Checklist Before the Submission

- (V) I reserved my time slot for the S1P presentation, and I understand that I earn 0 points if I miss attendance of the presentations.
- (V) I checked that all of my rubrics are graded; in other words, there is no ? mark left.
- (V) I filled in all the links for the requested information.
- (V) I understand the grading rules and have graded by the rubric guidelines; I will ask any questions if I have any rules that are confusing or unclear.
- (V) I uploaded only this grading rubric file with the name properly changed, following the name convention (Doe_John_HW3_rubric.md).
- (V) I will upload my peer evaluation within one week after the HW3 deadline.
- (V) I understand that this assignment will be regraded and points can be deducted (up to 100%) if
  - Any violation of the integrity rule is detected
  - Not following the guideline
  - The content is of low quality
