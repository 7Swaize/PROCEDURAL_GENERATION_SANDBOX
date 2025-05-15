using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

// ReSharper disable SeparateLocalFunctionsWithJumpStatement
// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable InconsistentNaming

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public struct WaveFunctionCollapseSettings {
        public Vector2Int gridSize;
        public Vector3Int startPosition;
        public Dictionary<Vector3Int, Slot[]> chunkDirNeighbors;
        
        public WaveFunctionCollapseSettings(Vector2Int size, Vector3Int start, Dictionary<Vector3Int, Slot[]> neighbors) {
            gridSize = size;
            startPosition = start;
            chunkDirNeighbors = neighbors;
        }
    }

    public struct SlotState {
        public readonly Vector3Int position;
        public readonly List<Prototype> possiblePrototypes;
        public readonly Prototype selectedPrototype;

        public SlotState(Vector3Int position, List<Prototype> possiblePrototypes, Prototype selectedPrototype) {
            this.position = position;
            this.possiblePrototypes = possiblePrototypes;
            this.selectedPrototype = selectedPrototype;
        }
    }
    
    public abstract class WaveFunctionCollapseMapAbstract {
        #region INSTANCE VARS
        
        protected Vector3Int startPosition;
        protected Slot[,] grid;
        protected List<Prototype> prototypes;
        protected List<Slot> slotsToPropagate = new();
        
        private readonly Stack<SlotState> _slotStateHistory = new();
        private readonly Prototype _fallBackPrototype;

        #endregion

        #region ABSTRACT METHODS

        public abstract Slot GetSlot(Vector3Int position, bool wrapping=false);
        
        protected abstract void Init(Vector2Int size, Vector3Int startPosition);
        protected abstract Slot FindSlotWithMinimumEntropy();
        protected abstract bool AllSlotsAreCollapsed();
        protected abstract void PropagateConstraints(Vector3Int position);
        protected abstract bool TryCollapseSlot(Slot slotToCollapse);
        protected abstract Chunk GetGeneratedResult();

        #endregion


        protected WaveFunctionCollapseMapAbstract(Prototype fallBackPrototype) {
            this._fallBackPrototype = fallBackPrototype;
        }
        
        public virtual void RunAlgorithm(WaveFunctionCollapseSettings generationSettings, Action<Chunk> onChunkGeneratedCallback = null) {
            Init(generationSettings.gridSize, generationSettings.startPosition);
            RunCollapseInstant(onChunkGeneratedCallback);
        }

        protected void RunCollapseInstant(Action<Chunk> onChunkGeneratedCallback = null, int recursionThreshold = 10) {
            _slotStateHistory.Clear();
            
            Solve(0);
            onChunkGeneratedCallback?.Invoke(GetGeneratedResult());

            // Recursive backtracking if selectedSlot Entropy = 0 -> Utilizes Stack<SlotState>
            void Solve(int depth) {
                if (depth > recursionThreshold) {
                    Slot failedSlot = FindSlotWithMinimumEntropy();
                    ForceCollapse(failedSlot);
                    Solve(0);
                    
                    return;
                }

                if (AllSlotsAreCollapsed()) {
                    return;
                }

                Slot slotToCollapse = FindSlotWithMinimumEntropy();
                List<Prototype> originalPrototypes = new List<Prototype>(slotToCollapse.PossiblePrototypes);
                _slotStateHistory.Push(
                    new SlotState(
                        slotToCollapse.GetPosition(), 
                        new List<Prototype>(originalPrototypes), 
                        slotToCollapse.Prototype)
                );

                if (!TryCollapseSlot(slotToCollapse)) {
                    if (_slotStateHistory.TryPop(out SlotState previousState)) {
                        Slot toRestore = GetSlot(previousState.position);
                        toRestore.SetCollapsed(false);

                        toRestore.SetPossiblePrototypes(previousState.possiblePrototypes
                            .AsValueEnumerable()
                            .Where(p => p != previousState.selectedPrototype)
                            .ToList());

                        PropagateConstraints(previousState.position);
                        Solve(depth + 1);
                        
                        return;
                    }
                    else {
                        ForceCollapse(slotToCollapse);
                        Solve(0);
                        
                        return;
                    }
                }

                Solve(0);
            }
            
            void ForceCollapse(Slot slotToForceCollapse) {
                slotToForceCollapse.CollapseDefault(_fallBackPrototype);
                _slotStateHistory.Push(
                    new SlotState(
                        slotToForceCollapse.GetPosition(), 
                        new List<Prototype>(slotToForceCollapse.PossiblePrototypes),
                        slotToForceCollapse.Prototype)
                );
                
                PropagateConstraints(slotToForceCollapse.GetPosition());
            }
        }
    }
}