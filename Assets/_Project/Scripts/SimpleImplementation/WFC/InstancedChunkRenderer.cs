using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public class InstancedChunkRenderer : MonoBehaviour, IMapGenerationCallbackReceiver {
        public GenerateMapNearPlayer mapGenerator;
        
        private readonly Dictionary<Material, List<Matrix4x4>> _materialToMatrices = new();
        private readonly Dictionary<Material, Mesh> _materialToMesh = new();

        private void Start() {
            mapGenerator.RegisterChunkCallbackReceiver(this);
        }

        public void OnGenerateChunk(Chunk generatedChunk, GenerateMapNearPlayer source) {
            foreach (Slot slot in generatedChunk.slots) {
                if (!slot.IsCollapsed) continue;

                Prototype prototype = slot.Prototype;
                Matrix4x4 matrix = Matrix4x4.TRS(
                    pos: slot.GetPosition() + generatedChunk.chunkPosition,
                    q: slot.GetRotation(),
                    s: slot.GetScale()
                );
                
                
                if (!_materialToMatrices.ContainsKey(prototype.material)) {
                    _materialToMatrices[prototype.material] = new List<Matrix4x4>();
                    _materialToMesh[prototype.material] = prototype.mesh;
                }

                _materialToMatrices[prototype.material].Add(matrix);
            }
        }
        
        private void LateUpdate() {
            foreach (KeyValuePair<Material,List<Matrix4x4>> pair in _materialToMatrices) {
                Material material = pair.Key;
                Mesh mesh = _materialToMesh[material];
                List<Matrix4x4> matrices = pair.Value;
                
                // 1023 represents max num entities per batch
                for (int i = 0; i < matrices.Count; i += 1023) {
                    int count = Mathf.Min(1023, matrices.Count - i);
                    Graphics.DrawMeshInstanced(mesh, 0, material, matrices.GetRange(i, count));
                }
            }
        }

        private void OnDisable() {
            mapGenerator.DeregisterChunkCallbackReceiver(this);
        }
    }
}