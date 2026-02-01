using UnityEngine;

namespace MasqueradeMystery
{
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance { get; private set; }

        [Header("Timer Settings")]
        [SerializeField] private float baseTimeLimit = 60f;
        [SerializeField] private float timeReductionPerRound = 5f;
        [SerializeField] private float minimumTimeLimit = 15f;

        public int CurrentRound { get; private set; }
        public int ConsecutiveWins { get; private set; }
        public int LastRoundGuessesUsed { get; private set; }
        public bool LastRoundWasSuccess { get; private set; }

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

        public void StartNewSession()
        {
            CurrentRound = 0;
            ConsecutiveWins = 0;
            LastRoundGuessesUsed = 0;
            LastRoundWasSuccess = false;
        }

        public void StartRound()
        {
            CurrentRound++;
            GameEvents.OnRoundStarted?.Invoke(CurrentRound);
        }

        public void EndRound(bool success, int guessesUsed)
        {
            LastRoundWasSuccess = success;
            LastRoundGuessesUsed = guessesUsed;

            if (success)
            {
                ConsecutiveWins++;
            }

            GameEvents.OnRoundEnded?.Invoke(success, guessesUsed);
        }

        public float GetTimeForRound(int round)
        {
            float time = baseTimeLimit - (timeReductionPerRound * (round - 1));
            return Mathf.Max(time, minimumTimeLimit);
        }

        public float GetCurrentRoundTime()
        {
            return GetTimeForRound(CurrentRound);
        }

        public void ResetSession()
        {
            StartNewSession();
        }
    }
}
