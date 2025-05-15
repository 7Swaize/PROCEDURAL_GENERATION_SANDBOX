    using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation.Orientations;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming

// IMPORTANT: ALL calculations done here are in chunk coords. Any other calculations are in world coords.

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public class GenerateMapNearPlayer : MonoBehaviour {
        private class ChunkEvents {
            public readonly ConcurrentQueue<Chunk> completedChunks; // TODO: ConcurrentQueue for thread safety?
            public readonly List<IMapGenerationCallbackReceiver> mapGenerationCallbackReceivers;
            private readonly GenerateMapNearPlayer source;

            public ChunkEvents(GenerateMapNearPlayer mapSource) {
                completedChunks = new ConcurrentQueue<Chunk>();
                mapGenerationCallbackReceivers = new List<IMapGenerationCallbackReceiver>();
                source = mapSource;
            }

            // TODO: Culling to reduce verts count
            public void Update() {
                if (completedChunks.TryDequeue(out Chunk completedChunk)) {
                    foreach (IMapGenerationCallbackReceiver subscriber in mapGenerationCallbackReceivers) {
                        subscriber.OnGenerateChunk(completedChunk, source);
                    }
                }
            }
        }
        
        [Space, Header("Chunk Configurations")]
        [SerializeField] private Vector2Int chunkSize;
        [SerializeField] private int chunkLoadRadius;
        
        [Space, Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Prototype fallBackPrototype;

        private Dictionary<Vector3Int, Chunk> _chunkMap = new();
        private ChunkEvents _chunkEventManager;
        private WaveFunctionCollapseMapChunk _map;
        
        
        public void RegisterChunkCallbackReceiver(IMapGenerationCallbackReceiver receiver) => _chunkEventManager?.mapGenerationCallbackReceivers.Add(receiver);
        public void DeregisterChunkCallbackReceiver(IMapGenerationCallbackReceiver receiver) => _chunkEventManager?.mapGenerationCallbackReceivers.Remove(receiver);

        private void Awake() {
            _chunkEventManager = new ChunkEvents(this);
            _map = new WaveFunctionCollapseMapChunk(fallBackPrototype);
        }

        private void Update() {
            _chunkEventManager.Update();
            ChunkGeneratorDispatcher();
        }

        private void ChunkGeneratorDispatcher() {
            Vector3Int playerChunkCoord = WorldToChunkCoord(playerTransform.position);

            for (int x = -chunkLoadRadius; x <= chunkLoadRadius; x++) {
                for (int z = -chunkLoadRadius; z <= chunkLoadRadius; z++) {
                    Vector3Int chunkCoord = new Vector3Int(playerChunkCoord.x + x, 0, playerChunkCoord.z + z);

                    if (!_chunkMap.ContainsKey(chunkCoord)) {
                        GenerateChunk(chunkCoord);
                    }
                }
            }
        }
        
        private Dictionary<Vector3Int, Chunk> FindNeighbourChunks(Vector3Int chunkCoord) {
            Dictionary<Vector3Int, Chunk> neighbourChunks = new();
            
            foreach (Vector3Int dir in Vectors) {
                Vector3Int neighborChunkCoord = chunkCoord + dir;
                if (_chunkMap.TryGetValue(neighborChunkCoord, out Chunk neighbourChunk)) {
                    neighbourChunks[neighborChunkCoord] = neighbourChunk;
                }
            }
            
            return neighbourChunks;
        }

        private Dictionary<Vector3Int, Slot[]> FindNeighbourSlotsFromChunks(Dictionary<Vector3Int, Chunk> neighborChunks, Vector3Int chunkCoord) {
            Dictionary<Vector3Int, Slot[]> neighbourSlots = new();

            foreach (KeyValuePair<Vector3Int, Chunk> neighbourChunk in neighborChunks) {
                if (neighbourChunk.Key.x < chunkCoord.x) {
                    neighbourSlots[GetDirectionVector(Orientation.Left)] = neighbourChunk.Value.GetBorderSlots(Orientation.Right).ToArray();
                } else if (neighbourChunk.Key.x > chunkCoord.x) {
                    neighbourSlots[GetDirectionVector(Orientation.Right)] = neighbourChunk.Value.GetBorderSlots(Orientation.Left).ToArray();
                } else if (neighbourChunk.Key.z < chunkCoord.z) {
                    neighbourSlots[GetDirectionVector(Orientation.Back)] = neighbourChunk.Value.GetBorderSlots(Orientation.Forward).ToArray();
                } else {
                    neighbourSlots[GetDirectionVector(Orientation.Forward)] = neighbourChunk.Value.GetBorderSlots(Orientation.Back).ToArray();
                }
            }
            
            return neighbourSlots;
        }

        private void GenerateChunk(Vector3Int chunkCoord) {
            Vector3Int worldPos = ChunkToWorldCoord(chunkCoord);

            WaveFunctionCollapseSettings settings = new WaveFunctionCollapseSettings {
                startPosition = worldPos,
                gridSize = chunkSize,
                chunkDirNeighbors = FindNeighbourSlotsFromChunks(FindNeighbourChunks(chunkCoord), chunkCoord)
            };

            _map.RunAlgorithm(settings, OnChunkGenerationComplete);
        }
        
        // TODO: Later Refactor to UniTask 
        private async Task GenerateChunkAsync(Vector3Int chunkCoord) {
            Vector3Int worldPos = ChunkToWorldCoord(chunkCoord);
            
            WaveFunctionCollapseSettings settings = new WaveFunctionCollapseSettings {
                startPosition = worldPos,
                gridSize = chunkSize,
                chunkDirNeighbors = FindNeighbourSlotsFromChunks(FindNeighbourChunks(chunkCoord), chunkCoord)
            };

            await Task.Run(() => _map.RunAlgorithm(settings, OnChunkGenerationComplete));
        }

        // Lock chunkEventManager for thread-safety
        private void OnChunkGenerationComplete(Chunk generatedChunk) {
            _chunkEventManager.completedChunks.Enqueue(generatedChunk);
            _chunkMap[WorldToChunkCoord(generatedChunk.chunkPosition)] = generatedChunk;
            
        }
        
        private Vector3Int WorldToChunkCoord(Vector3 worldPosition) {
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / chunkSize.x),
                0,
                Mathf.FloorToInt(worldPosition.z / chunkSize.y)
            );
        }

        private Vector3Int ChunkToWorldCoord(Vector3Int chunkCoord) {
            return new Vector3Int(
                Mathf.FloorToInt(chunkCoord.x * chunkSize.x),
                0,
                Mathf.FloorToInt(chunkCoord.z * chunkSize.y)
            );
        }
    }
}