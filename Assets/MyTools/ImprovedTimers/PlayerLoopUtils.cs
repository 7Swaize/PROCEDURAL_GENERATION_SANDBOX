using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace VS.Utilities.LowLevel
{
    public static class PlayerLoopUtils
    {
        /// <summary>
        /// Removes a specified system from the player loop system hierarchy.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the system to be removed. Used for identifying the context in which the system is being removed.
        /// </typeparam>
        /// <param name="playerLoopSystem">
        /// A reference to the root <see cref="PlayerLoopSystem"/> from which the system should be removed.
        /// </param>
        /// <param name="systemToRemove">
        /// The <see cref="PlayerLoopSystem"/> instance representing the system to be removed. 
        /// The removal is based on matching the system's <c>type</c> and <c>updateDelegate</c>.
        /// </param>
        /// <remarks>
        /// This method iterates through the <paramref name="playerLoopSystem"/>'s <c>subSystemList</c> to locate
        /// and remove the system that matches the specified criteria. If the system is found and removed, 
        /// the <c>subSystemList</c> is updated. The method also processes the removal in nested subsystems
        /// using <see cref="HandleSubSystemLoopForRemoval{T}"/>.
        /// </remarks>
        public static void RemoveSystem<T>(ref PlayerLoopSystem playerLoopSystem, in PlayerLoopSystem systemToRemove)
        {
            if (playerLoopSystem.subSystemList == null) return;
            
            var playerLoopSystemList = new List<PlayerLoopSystem>(playerLoopSystem.subSystemList);
            for (int i = 0; i < playerLoopSystemList.Count; ++i)
            {
                if (playerLoopSystemList[i].type == systemToRemove.type &&
                    playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate)
                {
                    playerLoopSystemList.RemoveAt(i);
                    playerLoopSystem.subSystemList = playerLoopSystemList.ToArray();
                }
            }

            HandleSubSystemLoopForRemoval<T>(ref playerLoopSystem, systemToRemove);
        }

        /// <summary>
        /// Recursively processes the subsystems of a <see cref="PlayerLoopSystem"/> to remove a specified system.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the system to be removed. Used for contextual identification during the recursive removal process.
        /// </typeparam>
        /// <param name="playerLoopSystem">
        /// A reference to the parent <see cref="PlayerLoopSystem"/> whose subsystems are to be checked and processed.
        /// </param>
        /// <param name="systemToRemove">
        /// The <see cref="PlayerLoopSystem"/> instance representing the system to be removed. 
        /// The removal is performed if a matching system is found in the subsystems.
        /// </param>
        /// <remarks>
        /// This method iterates through the <c>subSystemList</c> of the provided <paramref name="playerLoopSystem"/>.
        /// For each subsystem, it calls <see cref="RemoveSystem{T}"/> to handle the removal process.
        /// If the <c>subSystemList</c> is null, the method exits early.
        /// </remarks>
        private static void HandleSubSystemLoopForRemoval<T>(ref PlayerLoopSystem playerLoopSystem, in PlayerLoopSystem systemToRemove)
        {
            if (playerLoopSystem.subSystemList == null) return;

            for (int i = 0; i < playerLoopSystem.subSystemList.Length; ++i)
            {
                RemoveSystem<T>(ref playerLoopSystem.subSystemList[i], systemToRemove);
            }
        }

        /// <summary>
        /// Inserts a specified system into the player loop system hierarchy at a given index.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the system into which the insertion is being performed. Used to identify the correct level in the player loop hierarchy.
        /// </typeparam>
        /// <param name="playerLoopSystem">
        /// A reference to the root <see cref="PlayerLoopSystem"/> where the system should be inserted.
        /// </param>
        /// <param name="systemToInsert">
        /// The <see cref="PlayerLoopSystem"/> instance representing the system to be inserted.
        /// </param>
        /// <param name="index">
        /// The zero-based index at which the system should be inserted in the <c>subSystemList</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the system was successfully inserted; <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// If the type of the provided <paramref name="playerLoopSystem"/> matches the type <typeparamref name="T"/>,
        /// the system is inserted at the specified index in its <c>subSystemList</c>. Otherwise, the method
        /// delegates the insertion to the subsystems using <see cref="HandleSubSystemLoop{T}"/>.
        /// </remarks>
        public static bool InsertSystem<T>(ref PlayerLoopSystem playerLoopSystem, in PlayerLoopSystem systemToInsert,
            int index)
        {
            if (playerLoopSystem.type != typeof(T)) return HandleSubSystemLoop<T>(ref playerLoopSystem, systemToInsert, index);
            
            var playerLoopSystemList = new List<PlayerLoopSystem>(); 
            if (playerLoopSystem.subSystemList != null) playerLoopSystemList.AddRange(playerLoopSystem.subSystemList);
            playerLoopSystemList.Insert(index, systemToInsert);
            playerLoopSystem.subSystemList = playerLoopSystemList.ToArray();
            return true;
        }

        /// <summary>
        /// Recursively searches through the subsystems of a <see cref="PlayerLoopSystem"/> to insert a specified system.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the system into which the insertion is being performed. Used to determine the correct context for the insertion.
        /// </typeparam>
        /// <param name="playerLoopSystem">
        /// A reference to the parent <see cref="PlayerLoopSystem"/> whose subsystems are to be checked for insertion.
        /// </param>
        /// <param name="systemToInsert">
        /// The <see cref="PlayerLoopSystem"/> instance representing the system to be inserted.
        /// </param>
        /// <param name="index">
        /// The zero-based index at which the system should be inserted in the <c>subSystemList</c> of the matching subsystem.
        /// </param>
        /// <returns>
        /// <c>true</c> if the system was successfully inserted into one of the subsystems; <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// This method iterates through the <c>subSystemList</c> of the provided <paramref name="playerLoopSystem"/>. 
        /// For each subsystem, it attempts to insert the specified system by calling <see cref="InsertSystem{T}"/>. 
        /// If the insertion succeeds in any subsystem, the method immediately returns <c>true</c>. If no matching 
        /// subsystem is found or the <c>subSystemList</c> is null, the method returns <c>false</c>.
        /// </remarks>
        private static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem playerLoopSystem,
            in PlayerLoopSystem systemToInsert, int index)
        {
            if (playerLoopSystem.subSystemList == null) return false;

            for (int i = 0; i < playerLoopSystem.subSystemList.Length; ++i)
            {
                if (!InsertSystem<T>(ref playerLoopSystem.subSystemList[i], in systemToInsert, index)) continue;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Prints the structure of the Unity Player Loop system to the console.
        /// </summary>
        /// <param name="loopSystem">
        /// The root <see cref="PlayerLoopSystem"/> whose subsystems will be printed recursively.
        /// </param>
        /// <remarks>
        /// This method creates a structured representation of the Unity Player Loop hierarchy and logs it to the console using <see cref="Debug.Log"/>. 
        /// It iterates through the <c>subSystemList</c> of the given <paramref name="loopSystem"/>, calling the <c>PrintSubSystem</c> method to 
        /// handle each subsystem recursively. The output is formatted using a <see cref="StringBuilder"/> for efficiency.
        /// </remarks>
        public static void PrintPlayerLoop(PlayerLoopSystem loopSystem)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Unity Player Loop");

            foreach (PlayerLoopSystem loopSubSystem in loopSystem.subSystemList)
            {
                PrintSubSystem(loopSubSystem, sb, 0);
            }
            // Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Recursively appends the structure of a <see cref="PlayerLoopSystem"/> and its subsystems to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="loopSystem">
        /// The <see cref="PlayerLoopSystem"/> whose information is to be appended.
        /// </param>
        /// <param name="sb">
        /// The <see cref="StringBuilder"/> used to build the formatted output of the player loop hierarchy.
        /// </param>
        /// <param name="level">
        /// The current recursion depth, used to determine the indentation level for visual clarity in the output.
        /// </param>
        /// <remarks>
        /// This method appends the type of the current <paramref name="loopSystem"/> to the <paramref name="sb"/>, indented based on the recursion depth.
        /// If the <c>subSystemList</c> of the current loop system is null or empty, the method terminates for that branch.
        /// Otherwise, it recursively processes each subsystem by calling itself with an incremented <paramref name="level"/>.
        /// </remarks>
        private static void PrintSubSystem(PlayerLoopSystem loopSystem, StringBuilder sb, int level)
        {
            sb.Append(' ', level * 2).AppendLine(loopSystem.type.ToString());
            if (loopSystem.subSystemList == null || loopSystem.subSystemList.Length == 0) return;

            foreach (PlayerLoopSystem subSystem in loopSystem.subSystemList)
            {
                PrintSubSystem(subSystem, sb, level + 1);
            }
        }
    }
}