using System;
using UnityEngine;

// SOURCES:
// https://www.youtube.com/watch?v=ilvmOQtl57c
// https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern
namespace VS.Utilities.ImprovedTimers {
    
    public abstract class Timer : IDisposable {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; protected set; }
        public float Progress => Mathf.Clamp(CurrentTime / initialTime, 0, 1);

        public Action OnTimerStart = delegate { };
        public Action OnTimerEnd = delegate { };

        protected float initialTime;

        private bool _disposed;


        protected Timer(float value) {
            initialTime = value;
        }

        public virtual void Start() {
            CurrentTime = initialTime;
            if (!IsRunning) {
                IsRunning = true;
                TimerManager.RegisterTimer(this);
                OnTimerStart.Invoke();
            }
        }

        public void Stop() {
            if (IsRunning) {
                IsRunning = false;
                TimerManager.DeregisterTimer(this);
                OnTimerEnd.Invoke();
            }
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public virtual void Reset() => CurrentTime = initialTime;

        public virtual void Reset(float newTime) {
            initialTime = newTime;
            Reset();
        }

        // Tick every update facilitated by Timer Manager
        public abstract void Tick();
        public abstract bool IsFinished { get; }


        ~Timer() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed) return;

            if (disposing) {
                TimerManager.DeregisterTimer(this);
            }

            _disposed = true;
        }
    }

    public class CountdownTimer : Timer {
        public CountdownTimer(float value) : base(value) { }

        public override void Tick() {
            if (IsRunning && CurrentTime >= 0) {
                CurrentTime -= Time.deltaTime;
            }

            if (IsRunning && CurrentTime <= 0) {
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;
    }

    public class StopwatchTimer : Timer {
        public StopwatchTimer(float value) : base(value) { }

        public override void Tick() {
            if (IsRunning) {
                CurrentTime += Time.deltaTime;
            }
        }

        public override bool IsFinished { get; }
    }

    public class TickTimer : Timer {
        public float TickInterval { get; }
        public int TotalTicks { get; }
        public Action OnTick = delegate { };
        private float _nextTickTime;

        public TickTimer(float tickInterval, int totalTicks) : base(tickInterval * totalTicks) {
            TickInterval = tickInterval;
            TotalTicks = totalTicks;
        }

        public override void Start() {
            base.Start();
            _nextTickTime = initialTime - TickInterval;
        }

        public override void Tick() {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= Time.deltaTime;

                if (CurrentTime <= _nextTickTime) {
                    OnTick.Invoke();
                    _nextTickTime -= TickInterval;
                }
            }

            if (IsRunning && CurrentTime <= 0) {
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;
    }
}