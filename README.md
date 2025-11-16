# Wave Function Collapse - Unity Implementation

A proof of concept for procedural generation using the Wave Function Collapse (WFC) algorithm to generate infinite road networks and city-like environments in Unity.

## What It Does

- Generates chunked terrain around the player as they move
- Uses WFC to ensure valid tile adjacency (roads connect properly, buildings align correctly)
- GPU instanced rendering for performance
- Handles chunk boundaries by propagating constraints between neighboring chunks
- Includes backtracking when the algorithm gets stuck (entropy hits zero)

## Core Components

**WaveFunctionCollapseMapChunk.cs** - Main WFC solver. Collapses slots based on entropy, propagates constraints to neighbors.

**GenerateMapNearPlayer.cs** - Manages chunk generation around player position. Keeps track of loaded chunks and dispatches generation for new ones.

**Slot.cs** - Represents a grid position. Tracks possible prototypes and handles collapse logic with validation.

**Prototype.cs** - ScriptableObject defining tiles. Contains mesh/material for instancing and adjacency rules (which prototypes can connect in each direction).

**InstancedChunkRenderer.cs** - Batches meshes by material and renders using `Graphics.DrawMeshInstanced`.

**Chunk.cs** - Container for a grid of slots with utility methods for border access.

## How It Works

1. Player moves, dispatcher checks which chunks need generating
2. For each new chunk, gather border slots from existing neighbors
3. Initialize new chunk grid with constraints from neighbors
4. Run WFC: find lowest entropy slot, collapse it, propagate constraints
5. If collapse fails (no valid options), backtrack using state history
6. If backtracking fails, force collapse with fallback prototype
7. When complete, enqueue chunk for rendering
8. Renderer batches instances by material and draws each frame
