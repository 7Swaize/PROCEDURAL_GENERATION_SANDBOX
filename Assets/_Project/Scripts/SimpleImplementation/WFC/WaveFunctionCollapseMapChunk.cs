using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;
using static VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation.Orientations;

// ReSharper disable ParameterHidesMember
// ReSharper disable ConvertClosureToMethodGroup

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public class WaveFunctionCollapseMapChunk : WaveFunctionCollapseMapAbstract {
        private Vector2Int _size;
        
        public Vector2Int Size => _size;
        public int MaxEntropy => prototypes.Count;
        
        #region INIT

        public WaveFunctionCollapseMapChunk(Prototype fallBackPrototype) : base(fallBackPrototype) { }
        
        protected override void Init(Vector2Int size, Vector3Int startPosition) {
            LoadResources();
            InitGrid(size, startPosition);
        }

        private void Init(Vector2Int size, Vector3Int startPosition, Dictionary<Vector3Int, Slot[]> dirSlotNeighbors) {
            Init(size, startPosition);
            InitExistingChunkNeighborPropagation(dirSlotNeighbors);
        }

        private void InitExistingChunkNeighborPropagation(Dictionary<Vector3Int, Slot[]> dirSlotNeighbors) {
            foreach (KeyValuePair<Vector3Int, Slot[]> kvp in dirSlotNeighbors) {
                Vector3Int direction = kvp.Key;
                Slot[] externalSlots = kvp.Value;
                Slot[] localEdgeSlots = GetBorderSlots(GetOrientationRepresentation(direction)).ToArray();

                for (int i = 0; i < externalSlots.Length; i++) {
                    Slot externalSlot = externalSlots[i];
                    Slot localSlot = localEdgeSlots[i];
                    Orientation oppositeDir = GetOppositeOrientationRepresentation(direction);

                    List<Prototype> allowedPrototypes = prototypes
                        .AsValueEnumerable()
                        .Where(possiblePrototype => externalSlot.Prototype
                            .possibleConnections[oppositeDir]
                            .Contains(possiblePrototype))
                        .Distinct()
                        .ToList();

                    localSlot.SetPossiblePrototypes(allowedPrototypes);
                }
            }
            
        }

        private void InitGrid(Vector2Int size, Vector3Int startPosition) {
            this.startPosition = startPosition;
            grid = new Slot[size.x, size.y];
            _size = size;

            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z < size.y; z++) {
                    Vector3Int worldPosition = new Vector3Int(x, 0, z);
                    grid[x, z] = new Slot(worldPosition, new List<Prototype>(prototypes), this);
                }
            }
        }

        #endregion
            
        public override void RunAlgorithm(WaveFunctionCollapseSettings generationSettings, Action<Chunk> onChunkGeneratedCallback = null) {
            Init(generationSettings.gridSize, generationSettings.startPosition, generationSettings.chunkDirNeighbors);
            RunCollapseInstant(onChunkGeneratedCallback);
        }
        
        protected override Chunk GetGeneratedResult() => new Chunk(startPosition, _size, grid);
        private void LoadResources() => prototypes = new List<Prototype>(Resources.LoadAll<Prototype>("Data/Prototypes"));
        
        public override Slot GetSlot(Vector3Int position, bool wrapping=false) {
            if (wrapping) {
                return grid[
                    position.x % _size.x + (position.x % _size.x < 0 ? _size.x : 0),
                    position.z % _size.y + (position.z % _size.y < 0 ? _size.y : 0)]; 
            }
            else {
                return position.x < 0 || position.x >= _size.x || position.z < 0 || position.z >= _size.y
                    ? null
                    : grid[position.x, position.z];
            }
        }
        
        private IEnumerable<Slot> GetBorderSlots(Orientation borderDirection) {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            return borderDirection switch {
                Orientation.Left => Enumerable.Range(0, height).Select(y => grid[0, y]),
                Orientation.Right => Enumerable.Range(0, height).Select(y => grid[width - 1, y]),
                Orientation.Forward => Enumerable.Range(0, width).Select(x => grid[x, height - 1]),
                Orientation.Back => Enumerable.Range(0, width).Select(x => grid[x, 0]),
                _ => Enumerable.Empty<Slot>()
            };
        }
        
        protected override Slot FindSlotWithMinimumEntropy() {
            Slot selectedSlot = Enumerable.Range(0, _size.x)
                .AsValueEnumerable()
                .SelectMany(x => Enumerable.Range(0, _size.y), (x, z) => grid[x, z]) 
                .Where(slot => !slot.IsCollapsed)
                .OrderBy(slot => slot.Entropy)
                .First();
            
            return selectedSlot;
        }

        protected override bool AllSlotsAreCollapsed() {
            bool alSlotsCollapsed = Enumerable.Range(0, _size.x)
                .AsValueEnumerable()
                .SelectMany(x => Enumerable.Range(0, _size.y), (x, z) => grid[x, z])
                .All(slot => slot.IsCollapsed);
            
            return alSlotsCollapsed;
        }

        protected override bool TryCollapseSlot(Slot slotToCollapse) {
            if (!slotToCollapse.Collapse()) return false;
            
            Vector3Int spawnPos = slotToCollapse.GetPosition();
            PropagateConstraints(spawnPos);
            return true;
        }

        protected override void PropagateConstraints(Vector3Int position) {
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            queue.Enqueue(position);
            visited.Add(position);

            while (queue.Count > 0) {
                Vector3Int currentPos = queue.Dequeue();
                Slot currentSlot = GetSlot(currentPos);

                foreach (Vector3Int dir in Vectors) {
                    Vector3Int neighborPos = currentPos + dir;
                    if (visited.Contains(neighborPos)) continue;

                    Slot neighborSlot = GetSlot(neighborPos);
                    if (neighborSlot == null || neighborSlot.IsCollapsed) continue;

                    List<Prototype> validNeighborPrototypes = new List<Prototype>();
                    
                    foreach (var prototype in currentSlot.PossiblePrototypes) {
                        IEnumerable<Prototype> compatibleNeighbors = dir == Vector3Int.forward ? prototype.possibleConnections[Orientation.Forward] :
                            dir == Vector3Int.right ? prototype.possibleConnections[Orientation.Right] :
                            dir == Vector3Int.back ? prototype.possibleConnections[Orientation.Back] :
                            prototype.possibleConnections[Orientation.Left];

                        validNeighborPrototypes = validNeighborPrototypes.Concat(compatibleNeighbors).ToList();
                    }

                    List<Prototype> newOptions = neighborSlot.PossiblePrototypes
                        .AsValueEnumerable()
                        .Where(p => validNeighborPrototypes.Contains(p))
                        .Distinct()
                        .ToList();

                    if (newOptions.Count < neighborSlot.PossiblePrototypes.Count) {
                        neighborSlot.SetPossiblePrototypes(newOptions);
                        queue.Enqueue(neighborPos);
                        visited.Add(neighborPos);
                    }
                }
            }
        }
    }
}