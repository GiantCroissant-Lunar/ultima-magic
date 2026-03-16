# Ultima Magic — Project Roadmap

## Vision

**Ultima Magic** is a hybrid RPG that combines two classic game styles:

- **Overworld Exploration (Ultima-style):** The player navigates a top-down, tile-based world map — walking through towns, dungeons, and wilderness.
- **First-Person Combat (Might & Magic-style):** When the player encounters enemies, the game transitions into a first-person battle view where the player controls a single character in turn-based combat.

The game is built with **Godot 4.6 (Mono / C#)** and uses **Jolt Physics** for 3D.

---

## Issue Dependency Graph

The issues are designed to be tackled roughly in order, though some can be parallelized.

```
Issue 1: Project Structure
    │
    ├──► Issue 2: Overworld Tile Map
    │        │
    │        └──► Issue 3: Player Character & Movement (Overworld)
    │                 │
    │                 ├──► Issue 4: Overworld NPCs & Interactions
    │                 │
    │                 └──► Issue 5: Random Encounter System
    │                          │
    │                          └──► Issue 7: Scene Transition (Overworld ↔ Battle)
    │
    ├──► Issue 6: First-Person Battle Scene
    │        │
    │        └──► Issue 7: Scene Transition (Overworld ↔ Battle)
    │
    └──► Issue 8: Character Stats & Data Models
             │
             ├──► Issue 9: Turn-Based Combat System
             │        │
             │        └──► Issue 10: Enemy System & AI
             │
             └──► Issue 10: Enemy System & AI
```

---

## Issues

The files in [`docs/issues/`](issues/) are now written as **GitHub issue drafts**. Use [`docs/issues/README.md`](issues/README.md) for manual creation steps and [`docs/issues/manifest.json`](issues/manifest.json) for a machine-readable index.

| #  | Title | Priority | Depends On |
|----|-------|----------|------------|
| 1  | [Set Up C# Project Structure and Solution](issues/issue-01-project-structure.md) | 🔴 Critical | — |
| 2  | [Create Overworld Tile Map System](issues/issue-02-overworld-tilemap.md) | 🔴 Critical | #1 |
| 3  | [Implement Player Character and Movement on Overworld](issues/issue-03-player-overworld.md) | 🔴 Critical | #2 |
| 4  | [Add Overworld NPCs and Interactable Objects](issues/issue-04-overworld-npcs.md) | 🟡 Medium | #3 |
| 5  | [Implement Random Encounter System](issues/issue-05-random-encounters.md) | 🔴 Critical | #3 |
| 6  | [Create First-Person Battle Scene](issues/issue-06-battle-scene.md) | 🔴 Critical | #1 |
| 7  | [Implement Scene Transition Between Overworld and Battle](issues/issue-07-scene-transition.md) | 🔴 Critical | #5, #6 |
| 8  | [Design Character Stats and Data Models](issues/issue-08-character-stats.md) | 🔴 Critical | #1 |
| 9  | [Implement Turn-Based Combat System](issues/issue-09-combat-system.md) | 🔴 Critical | #6, #8 |
| 10 | [Create Enemy System with Basic AI](issues/issue-10-enemy-system.md) | 🔴 Critical | #8, #9 |

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Engine | Godot 4.6.1 Mono |
| Language | C# (.NET 8.0) |
| Physics | Jolt Physics (3D) |
| Renderer | Forward Plus |
| Build System | NUKE 10.1.0 |
