using UnityEngine;

namespace MasqueradeMystery
{
    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance { get; private set; }

        public float TimeRemaining { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        private float lastTickTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!IsRunning || IsPaused) return;

            TimeRemaining -= Time.deltaTime;

            // Fire tick event once per second
            if (Mathf.Floor(TimeRemaining) < Mathf.Floor(lastTickTime))
            {
                GameEvents.OnTimerTick?.Invoke(TimeRemaining);
            }
            lastTickTime = TimeRemaining;

            if (TimeRemaining <= 0)
            {
                TimeRemaining = 0;
                IsRunning = false;
                GameEvents.OnTimerExpired?.Invoke();
            }
        }

        public void StartTimer(float duration)
        {
            TimeRemaining = duration;
            lastTickTime = duration;
            IsRunning = true;
            IsPaused = false;

            // Fire initial tick
            GameEvents.OnTimerTick?.Invoke(TimeRemaining);
        }

        public void StopTimer()
        {
            IsRunning = false;
            IsPaused = false;
        }

        public void PauseTimer()
        {
            IsPaused = true;
        }

        public void ResumeTimer()
        {
            IsPaused = false;
        }
    }
}
