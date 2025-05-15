using System;
using UnityEngine;

namespace VS.Utilities
{
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;
        
        public SerializableVector3(float rX, float rY, float rZ) {
            x = rX;
            y = rY;
            z = rZ;
        }
        
        public static implicit operator Vector3(SerializableVector3 vec) => new Vector3(vec.x, vec.y, vec.z);
        public static implicit operator SerializableVector3(Vector3 vec) => new SerializableVector3(vec.x, vec.y, vec.z);
    }
    
    [Serializable]
    public struct SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2(float rX, float rY)
        {
            x = rX;
            y = rY;
        }
        
        public static implicit operator Vector2(SerializableVector2 vec) => new Vector2(vec.x, vec.y);
        public static implicit operator SerializableVector2(Vector2 vec) => new SerializableVector2(vec.x, vec.y);
    }
}