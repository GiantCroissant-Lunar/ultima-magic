# Issue 2: Create Overworld Tile Map System

## Summary

Build the Ultima-style top-down tile map system for the overworld. Create tile sets representing different terrain types and assemble a sample overworld map that the player can explore.

## Motivation

The overworld is the primary exploration mode of the game, inspired by classic Ultima titles. A tile-based map is the foundation for player movement, NPC placement, encounter zones, and environmental storytelling.

## Acceptance Criteria

- [ ] A `TileSet` resource is created with at least the following terrain tiles:
  - Grass (walkable)
  - Water (not walkable)
  - Mountain/Rock (not walkable)
  - Forest/Trees (walkable, may trigger encounters)
  - Path/Road (walkable, reduced encounter rate)
  - Town/Building entrance (walkable, triggers town entry)
- [ ] Each tile is **16×16** or **32×32** pixels (consistent size across all tiles).
- [ ] Placeholder pixel art or colored squares are used for tiles (does not need to be final art).
- [ ] A `TileMap` node with `TileMapLayer` children is used (Godot 4.6 architecture for multi-layer tile maps).
- [ ] Navigation/collision layers are configured so non-walkable tiles block the player.
- [ ] A sample overworld map scene (`Overworld.tscn`) is created that is at least **40×30 tiles** in size.
- [ ] The map includes at least one town area, one forest area, one mountain range, and water features.
- [ ] A `Camera2D` is set up to follow the player (can be a placeholder `Sprite2D` for now).
- [ ] The scene loads and renders correctly when run in Godot.

## Technical Details

### Directory Structure

```
Scenes/Overworld/Overworld.tscn       # Main overworld scene
Resources/Tilesets/OverworldTiles.tres  # TileSet resource
Resources/Sprites/tiles/               # Tile sprite images
```

### TileMapLayer Setup

In Godot 4.6, use `TileMapLayer` nodes. Multiple layers can represent:
- **Layer 0 (Ground):** Base terrain (grass, water, paths)
- **Layer 1 (Details):** Trees, rocks, decorations on top of ground

### Collision Configuration

- Use physics layers on the `TileSet` to mark non-walkable tiles.
- Water tiles and mountain tiles should have collision shapes.
- Forest tiles should be walkable but tagged with custom data for encounter rates.

### Custom Data Layers on TileSet

Add custom data layers to tiles:
- `walkable` (bool) — whether the player can step on this tile
- `encounter_rate` (float) — multiplier for random encounter probability (0.0 = no encounters, 1.0 = normal, 2.0 = high)
- `tile_type` (string) — identifier like `"grass"`, `"forest"`, `"water"`, `"town"`

## Labels

`overworld`, `tilemap`, `priority-critical`
