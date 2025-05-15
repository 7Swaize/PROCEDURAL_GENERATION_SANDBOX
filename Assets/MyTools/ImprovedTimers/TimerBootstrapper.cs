using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using VS.Utilities.LowLevel;

namespace VS.Utilities.ImprovedTimers
{
    internal static class TimerBootstrapper
    {
        private static PlayerLoopSystem _timerSystem;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop(); // Gets current state of player loop
            
            // T will be of type update
            // Our system will become a subsystem of the update loop system
            if (!InsertTimerManager<Update>(ref playerLoopSystem, 0))
            {
                Debug.LogWarning("Improved Timers not initialized, unable to register TimerManager into the Update loop");
                return;
            }
            
            PlayerLoop.SetPlayerLoop(playerLoopSystem);
            PlayerLoopUtils.PrintPlayerLoop(playerLoopSystem);

            // Cleans everything up when coming out of playmode
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;

            static void OnPlayModeState(PlayModeStateChange state)
            {
                if (!Enum.IsDefined(typeof(PlayModeStateChange), state))
                    throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(PlayModeStateChange));
                
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    PlayerLoopSystem currentPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
                    RemoveTimerManager<Update>(ref currentPlayerLoopSystem);
                    PlayerLoop.SetPlayerLoop(currentPlayerLoopSystem);
                    
                    // Clears static list of timers on playmode exit
                    TimerManager.Clear();
                }
            }
#endif
        }

        private static void RemoveTimerManager<T>(ref PlayerLoopSystem playerLoopSystem)
        {
            // 'in' keyword means we are passing in something by reference, but it is read only within the context fo the method it is being passed into
            PlayerLoopUtils.RemoveSystem<T>(ref playerLoopSystem, in _timerSystem);
        }

        // Generic helper method because T will represent which subsystem in the player loop we want our system to be a subsystem of
        // Update() in this case
        private static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            _timerSystem = new PlayerLoopSystem()
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers, // Which method is going to get called inside the class
                subSystemList = null
            };
            
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in _timerSystem, index);
        }
    }
}
