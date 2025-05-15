using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public class Chunk {
        public Vector3Int chunkPosition;
        public Vector2Int chunkSize;
        public Slot[,] slots;
        public GameObject parentObj;

        public Chunk(Vector3Int chunkPosition, Vector2Int chunkSize, Slot[,] slots) {
            this.chunkPosition = chunkPosition;
            this.chunkSize = chunkSize;
            this.slots = slots;
            this.parentObj = new GameObject($"Chunk_{chunkPosition.x}_{chunkPosition.z}");
        }

        // IMPORTANT: Cast to collection once method is called -> an article talks about this somewhere
        public IEnumerable<Slot> GetBorderSlots(Orientations.Orientation borderDirection) {
            int width = slots.GetLength(0);
            int height = slots.GetLength(1);

            return borderDirection switch {
                Orientations.Orientation.Left => Enumerable.Range(0, height).Select(y => slots[0, y]),
                Orientations.Orientation.Right => Enumerable.Range(0, height).Select(y => slots[width - 1, y]),
                Orientations.Orientation.Forward => Enumerable.Range(0, width).Select(x => slots[x, height - 1]),
                Orientations.Orientation.Back => Enumerable.Range(0, width).Select(x => slots[x, 0]),
                _ => Enumerable.Empty<Slot>()
            };
        }
    }
}