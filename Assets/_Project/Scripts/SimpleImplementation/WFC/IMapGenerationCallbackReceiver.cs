using UnityEngine;

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public interface IMapGenerationCallbackReceiver {
        public void OnGenerateChunk(Chunk generatedChunk, GenerateMapNearPlayer source);
    }
}