using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Sets any values of a Vector3
    /// </summary>
    /// <example>
    /// For example, this code segment sets an x-offset to an object on instantiation
    /// <code>
    /// var @object = Instantiate(prefab, spawnTransform.position.With(x:4f), Quaternion.identity)
    /// </code>
    /// </example>
    public static class Vector3Extensions
    {
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }
    }

    public static class VectorMath
    {
        /// <summary>
        /// Extracts and returns the component of a vector that is the direction of a given vector.
        /// </summary>
        /// <param name="vector">The vector from which to extract the component.</param>
        /// <param name="direction">The direction vector to extract along.</param>
        /// <returns> The component of the vector in the direction of the given vector.</returns>
        public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
        {
            direction.Normalize();
            return direction * Vector3.Dot(vector, direction);
        }
        
        /// <summary>
        /// Removes the component of a vector that is in the direction of a given vector.
        /// </summary>
        /// <param name="vector">The vector from which to remove the component.</param>
        /// <param name="direction">The direction vector whose component should be removed.</param>
        /// <returns>The vector with the specified direction removed.</returns>
        public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction) {
            direction.Normalize();
            return vector - direction * Vector3.Dot(vector, direction);
        }
        
        /// <summary>
        /// Calculates the signed angle between two vectors on a plane defined by a normal vector.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <param name="planeNormal">The normal vector of the plane on which to calculate the angle.</param>
        /// <returns>The signed angle between the vectors in degrees.</returns>
        public static float GetAngle(Vector3 vector1, Vector3 vector2, Vector3 planeNormal) {
            var angle = Vector3.Angle(vector1, vector2);
            var sign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(vector1, vector2)));
            return angle * sign;
        }
    }
}