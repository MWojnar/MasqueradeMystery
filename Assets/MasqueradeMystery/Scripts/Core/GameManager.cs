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

        [Header("Game Settings")]
        [SerializeField] private int hintCount = 4;
        [SerializeField] private int maxWrongGuesses = 3;
        [SerializeField] private bool autoStartOnAwake = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        public GameState CurrentState { get; private set; }
        public CharacterData TargetCharacter { get; private set; }
        public List<Hint> CurrentHints { get; private set; }
        public int WrongGuesses { get; private set; }

        private List<Character> allCharacters;

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

            // Connect game over UI
            if (gameOverUI != null)
            {
                gameOverUI.OnRestartRequested += StartNewGame;
                gameOverUI.OnQuitRequested += QuitGame;
            }

            // Initialize status UI
            if (gameStatusUI != null)
            {
                gameStatusUI.Initialize(maxWrongGuesses);
            }

            if (autoStartOnAwake)
            {
                StartNewGame();
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnCharacterClicked -= HandleCharacterClicked;

            if (gameOverUI != null)
            {
                gameOverUI.OnRestartRequested -= StartNewGame;
                gameOverUI.OnQuitRequested -= QuitGame;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void StartNewGame()
        {
            WrongGuesses = 0;

            // Clear and spawn characters
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

            // Select random target
            Character targetChar = allCharacters[Random.Range(0, allCharacters.Count)];
            TargetCharacter = targetChar.Data;

            // Generate hints
            var allCharacterData = allCharacters.Select(c => c.Data).ToList();
            HintGenerator hintGen = new HintGenerator(TargetCharacter, allCharacterData);
            CurrentHints = hintGen.GenerateHints(hintCount);

            // Debug output
            if (showDebugInfo)
            {
                Debug.Log($"=== NEW GAME ===");
                Debug.Log($"Target: {TargetCharacter}");
                Debug.Log($"Hints:");
                foreach (var hint in CurrentHints)
                {
                    Debug.Log($"  - {hint.DisplayText}");
                }

                // Show how many characters match all hints
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

            SetState(GameState.Playing);
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
                SetState(GameState.Won);
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
                    SetState(GameState.Lost);
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
