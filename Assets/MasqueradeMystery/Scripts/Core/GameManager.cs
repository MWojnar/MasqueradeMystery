using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MasqueradeMystery
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CharacterSpawner spawner;
        [SerializeField] private GameStatusUI gameStatusUI;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private RoundResultsUI roundResultsUI;

        [Header("Game Settings")]
        [SerializeField] private int hintCount = 3;
        [SerializeField] private int maxWrongGuesses = 3;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        public FMODUnity.EventReference MainMusic;
        public GameState CurrentState { get; private set; }
        public CharacterData TargetCharacter { get; private set; }
        public List<Hint> CurrentHints { get; private set; }
        public int WrongGuesses { get; private set; }

        private List<Character> allCharacters;
        private EventInstance musicInstance;
        private Character targetCharacterObject;

		private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to events
            GameEvents.OnCharacterClicked += HandleCharacterClicked;
            GameEvents.OnTimerExpired += HandleTimerExpired;

            // Connect game over UI
            if (gameOverUI != null)
            {
                gameOverUI.OnRestartRequested += ReturnToTitle;
                gameOverUI.OnQuitRequested += QuitGame;
            }

            // Initialize status UI
            if (gameStatusUI != null)
            {
                gameStatusUI.Initialize(maxWrongGuesses);
            }

            // Start at title screen
            SetState(GameState.Title);
        }

        private void OnDestroy()
        {
            GameEvents.OnCharacterClicked -= HandleCharacterClicked;
            GameEvents.OnTimerExpired -= HandleTimerExpired;

            if (gameOverUI != null)
            {
                gameOverUI.OnRestartRequested -= ReturnToTitle;
                gameOverUI.OnQuitRequested -= QuitGame;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Called from title screen to start a new game session
        public void StartGameFromTitle()
        {
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.StartNewSession();
            }

            // Transition to gameplay
            if (TransitionController.Instance != null)
            {
                TransitionController.Instance.TransitionWithCallback(
                    () => StartNewRound(),
                    () => {
                        SetState(GameState.Playing);
                        StartTimer();
                    }
                );
                SetState(GameState.Transitioning);
            }
            else
            {
                StartNewRound();
                SetState(GameState.Playing);
                StartTimer();
            }
        }

        private void StartNewRound()
        {
            if (!MainMusic.IsNull && !musicInstance.isValid())
            {
                musicInstance = RuntimeManager.CreateInstance(MainMusic);
                musicInstance.start();
            }

            WrongGuesses = 0;

            // Increment round
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.StartRound();
            }

            // Clear existing characters
            ClearCharacters();

            // Spawn new characters
            if (spawner != null)
            {
                allCharacters = spawner.SpawnCharacters();
            }
            else
            {
                Debug.LogError("GameManager: CharacterSpawner reference is missing!");
                return;
            }

            if (allCharacters == null || allCharacters.Count == 0)
            {
                Debug.LogError("GameManager: No characters were spawned!");
                return;
            }

            // Generate hints first, then find/create a unique matching target
            HintGenerator hintGen = new HintGenerator(allCharacters);
            TargetCharacter = hintGen.GenerateHintsAndFindTarget(hintCount);
            CurrentHints = hintGen.GeneratedHints;

            // Find the target character object
            targetCharacterObject = allCharacters.Find(c => c.Data.CharacterId == TargetCharacter.CharacterId);

            // Debug output
            if (showDebugInfo)
            {
                int round = RoundManager.Instance != null ? RoundManager.Instance.CurrentRound : 1;
                Debug.Log($"=== ROUND {round} ===");
                Debug.Log($"Target: {TargetCharacter}");
                Debug.Log($"Hints:");
                foreach (var hint in CurrentHints)
                {
                    Debug.Log($"  - {hint.DisplayText}");
                }

                var allCharacterData = allCharacters.Select(c => c.Data).ToList();
                int matchCount = HintEvaluator.CountMatchingCharacters(allCharacterData, CurrentHints);
                Debug.Log($"Characters matching all hints: {matchCount}");
            }

            // Notify UI
            GameEvents.OnHintsGenerated?.Invoke(CurrentHints);

            // Re-initialize status UI
            if (gameStatusUI != null)
            {
                gameStatusUI.Initialize(maxWrongGuesses);
            }

            // Enable camera input and set follow target
            if (CameraController.Instance != null)
            {
                CameraController.Instance.EnableInput();

                if (spawner.PlayerCharacter != null)
                {
                    CameraController.Instance.SetFollowTarget(spawner.PlayerCharacter.transform);
                }
            }
        }

        private void StartTimer()
        {
            if (TimerManager.Instance != null && RoundManager.Instance != null)
            {
                float time = RoundManager.Instance.GetCurrentRoundTime();
                TimerManager.Instance.StartTimer(time);

                if (showDebugInfo)
                {
                    Debug.Log($"Timer started: {time} seconds");
                }
            }
        }

        private void ClearCharacters()
        {
            if (allCharacters != null)
            {
                foreach (var character in allCharacters)
                {
                    if (character != null)
                    {
                        Destroy(character.gameObject);
                    }
                }
                allCharacters.Clear();
            }
            targetCharacterObject = null;
        }

        private void HandleTimerExpired()
        {
            if (CurrentState != GameState.Playing) return;

            if (showDebugInfo)
            {
                Debug.Log("Time's up! Round failed.");
            }

            EndRound(false);
        }

        // Called when round ends (win or loss)
        private void EndRound(bool success)
        {
            // Stop timer
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.StopTimer();
            }

            // Disable camera input
            if (CameraController.Instance != null)
            {
                CameraController.Instance.DisableInput();
            }

            SetState(GameState.RoundEnding);

            // Start the round end sequence
            StartCoroutine(RoundEndSequence(success));
        }

        private IEnumerator RoundEndSequence(bool success)
        {
            // Pan camera to target
            if (CameraController.Instance != null && targetCharacterObject != null)
            {
                yield return CameraController.Instance.PanToTarget(targetCharacterObject.transform.position);
            }

            // Record round result
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.EndRound(success, WrongGuesses);
            }

            // Show results UI
            if (roundResultsUI != null)
            {
                int round = RoundManager.Instance != null ? RoundManager.Instance.CurrentRound : 1;
                int consecutiveWins = RoundManager.Instance != null ? RoundManager.Instance.ConsecutiveWins : 0;
                roundResultsUI.ShowResults(success, round, WrongGuesses, maxWrongGuesses, TargetCharacter, consecutiveWins);
            }

            // Set win/loss state for other UI components
            SetState(success ? GameState.Won : GameState.Lost);
        }

        // Called from RoundResultsUI when player presses continue
        public void ContinueFromRoundEnd(bool wasSuccess)
        {
            if (wasSuccess)
            {
                // Continue to next round
                if (TransitionController.Instance != null)
                {
                    TransitionController.Instance.TransitionWithCallback(
                        () => StartNewRound(),
                        () => {
                            SetState(GameState.Playing);
                            StartTimer();
                        }
                    );
                    SetState(GameState.Transitioning);
                }
                else
                {
                    StartNewRound();
                    SetState(GameState.Playing);
                    StartTimer();
                }
            }
            else
            {
                // Return to title
                ReturnToTitle();
            }
        }

        private void ReturnToTitle()
        {
            // Stop music
            if (musicInstance.isValid())
            {
                musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                musicInstance.release();
            }

            if (TransitionController.Instance != null)
            {
                TransitionController.Instance.TransitionWithCallback(
                    () => ClearCharacters(),
                    () => SetState(GameState.Title)
                );
                SetState(GameState.Transitioning);
            }
            else
            {
                ClearCharacters();
                SetState(GameState.Title);
            }
        }

        private void HandleCharacterClicked(Character character)
        {
            if (CurrentState != GameState.Playing) return;
            if (character == null || character.Data == null) return;

            if (character.Data.CharacterId == TargetCharacter.CharacterId)
            {
                // Correct guess!
                if (showDebugInfo)
                {
                    Debug.Log("Correct! Target found!");
                }
                GameEvents.OnTargetFound?.Invoke();
                EndRound(true);
            }
            else
            {
                // Wrong guess
                WrongGuesses++;
                if (showDebugInfo)
                {
                    Debug.Log($"Wrong guess! ({WrongGuesses}/{maxWrongGuesses})");
                }
                GameEvents.OnWrongGuess?.Invoke();

                if (WrongGuesses >= maxWrongGuesses)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log("Game Over - Too many wrong guesses!");
                    }
                    EndRound(false);
                }
            }
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            GameEvents.OnGameStateChanged?.Invoke(newState);

            if (showDebugInfo)
            {
                Debug.Log($"Game State: {newState}");
            }
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Public methods for external control
        public void SetHintCount(int count)
        {
            hintCount = Mathf.Max(1, count);
        }

        public void SetMaxWrongGuesses(int max)
        {
            maxWrongGuesses = Mathf.Max(1, max);
        }
    }
}
