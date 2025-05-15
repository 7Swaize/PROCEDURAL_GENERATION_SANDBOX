using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VS.ProceduralGeneration.WaveFunctionCollapse.SimpleImplementation {
    public static class Orientations {
        public enum Orientation {
            Left = 0,
            Right = 1,
            Forward = 2,
            Back = 3
        }

        private static Quaternion[] _rotations;
        private static Vector3Int[] _vectors;

        public static Vector3Int[] Vectors {
            get {
                if (_vectors == null) {
                    Initialize();
                }

                return _vectors;
            }
        }

        public static readonly Orientation[] HorizontalDirections = { Orientation.Left, Orientation.Right, Orientation.Back, Orientation.Forward };

        private static void Initialize() {
            _vectors = new Vector3Int[] {
                Vector3Int.left,   
                Vector3Int.right, 
                Vector3Int.back,    
                Vector3Int.forward, 
            };
        }

        public static Vector3Int GetDirectionVector(Orientation orientation) {
            return orientation switch {
                Orientation.Left => Vector3Int.left,
                Orientation.Right => Vector3Int.right,
                Orientation.Back => Vector3Int.back,    
                Orientation.Forward => Vector3Int.forward,
                _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
            };
        }

        // Literally just returns Orientation representation. I just wanted to compress the logic as much as possible.
        public static Orientation GetOrientationRepresentation(Vector3Int vector) {
            return vector switch {
                _ when vector == Vector3Int.left => Orientation.Left,
                _ when vector == Vector3Int.right => Orientation.Right,
                _ when vector == Vector3Int.forward => Orientation.Forward,
                _ when vector == Vector3Int.back => Orientation.Back,
                _ => throw new ArgumentOutOfRangeException(nameof(vector), vector, null)
            };
        }

        public static Orientation GetOppositeOrientationRepresentation(Vector3Int vector) {
            return vector switch {
                _ when vector == Vector3Int.left => Orientation.Right,
                _ when vector == Vector3Int.right => Orientation.Left,
                _ when vector == Vector3Int.forward => Orientation.Back,
                _ when vector == Vector3Int.back => Orientation.Forward,
                _ => throw new ArgumentOutOfRangeException(nameof(vector), vector, null)
            };
        }

        public static Orientation GetOppositeDirectionVector(Orientation direction) {
            return direction switch {
                Orientation.Left => Orientation.Right,
                Orientation.Right => Orientation.Left,
                Orientation.Forward => Orientation.Back,
                Orientation.Back => Orientation.Forward,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}