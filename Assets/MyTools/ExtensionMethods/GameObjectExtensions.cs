using UnityEngine;

namespace Utilities
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the component of an object if it is already present.
        /// If the desired component is not present on the object, it is added and returned
        /// </summary>
        /// <example>
        /// For Example:
        /// <code>
        /// DesiredComponent desiredComponentName = @object.GetOrAdd<see cref="DesiredComponent"/>();
        /// </code>
        /// </example>
        /// <typeparam name="T"> Desired component </typeparam>
        /// <returns></returns>
        public static T GetOrAdd<T>(this GameObject @object) where T : Component
        {
            T component = @object.GetComponent<T>();
            return component == null ? @object.AddComponent<T>() : component;
        }
    }
}