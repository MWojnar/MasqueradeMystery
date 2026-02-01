using FMOD;
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

        [Header("Accusation Animation")]
        [SerializeField] private float accusationWalkSpeed = 3f;
        [SerializeField] private float accusationStopDistance = 0.8f;
        [SerializeField] private float accusationFrameTime = 0.2f;
        [SerializeField] private float accusationZoomLevel = 2.5f;

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

            // Start music immediately and loop for entire game
            if (!MainMusic.IsNull && !musicInstance.isValid())
            {
                musicInstance = RuntimeManager.CreateInstance(MainMusic);
                musicInstance.start();
            }
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitGame();
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
            // Reset camera zoom for new round
            CameraController.Instance?.ResetZoom();

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
                UnityEngine.Debug.LogError("GameManager: CharacterSpawner reference is missing!");
                return;
            }

            if (allCharacters == null || allCharacters.Count == 0)
            {
                UnityEngine.Debug.LogError("GameManager: No characters were spawned!");
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
                UnityEngine.Debug.Log($"=== ROUND {round} ===");
                UnityEngine.Debug.Log($"Target: {TargetCharacter}");
                UnityEngine.Debug.Log($"Hints:");
                foreach (var hint in CurrentHints)
                {
                    UnityEngine.Debug.Log($"  - {hint.DisplayText}");
                }

                var allCharacterData = allCharacters.Select(c => c.Data).Where(d => !d.IsPlayer).ToList();
                int matchCount = HintEvaluator.CountMatchingCharacters(allCharacterData, CurrentHints);
                UnityEngine.Debug.Log($"Characters matching all hints: {matchCount}");
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
                    UnityEngine.Debug.Log($"Timer started: {time} seconds");
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
                UnityEngine.Debug.Log("Time's up! Round failed.");
            }

            SoundManager.Instance?.PlayFailureJingle();
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
            // Freeze all characters immediately on success
            if (success)
            {
                Character player = spawner?.PlayerCharacter;
                foreach (var character in allCharacters)
                {
                    if (character != player)
                    {
                        var animator = character.GetComponent<CharacterAnimator>();
                        animator?.StopAllActivity();
                    }
                }
            }

			// Pan camera to target (and zoom if success)
			if (CameraController.Instance != null && targetCharacterObject != null)
			{
                // Start zoom simultaneously with pan on success
                if (success)
                {
                    CameraController.Instance.ZoomTo(accusationZoomLevel, 1.5f);
                }
                yield return CameraController.Instance.PanToTarget(targetCharacterObject.transform.position);
            }

            // Play accusation animation sequence on success
            if (success && targetCharacterObject != null)
            {
                yield return StartCoroutine(AccusationSequence(targetCharacterObject));
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

        private IEnumerator AccusationSequence(Character accused)
        {
            // Get references
            Character player = spawner?.PlayerCharacter;
            if (player == null) yield break;

            CharacterAnimator accusedAnimator = accused.GetComponent<CharacterAnimator>();
            CharacterAnimator playerAnimator = player.GetComponent<CharacterAnimator>();
            CharacterVisuals accusedVisuals = accused.GetComponent<CharacterVisuals>();
            CharacterVisuals playerVisuals = player.GetComponent<CharacterVisuals>();

            if (accusedAnimator == null || playerAnimator == null) yield break;

            // Characters already frozen in RoundEndSequence

            // 1. Calculate target position (left of accused, same Y)
            Vector3 targetPosition = accused.transform.position;
            targetPosition.x -= accusationStopDistance;
            
            // Clamp to character bounds
            if (SceneBounds.Instance != null)
            {
                var bounds = SceneBounds.Instance.CharacterBounds;
                targetPosition.x = Mathf.Clamp(targetPosition.x, bounds.xMin, bounds.xMax);
                targetPosition.y = Mathf.Clamp(targetPosition.y, bounds.yMin, bounds.yMax);
            }

            // 2. Walk player to position if not already there
            float distanceToTarget = Vector3.Distance(player.transform.position, targetPosition);
            if (distanceToTarget > 0.2f)
            {
                // Start walking animation
                playerAnimator.SetExternalWalking(true);

                // Face the direction we're walking
                bool walkingRight = targetPosition.x > player.transform.position.x;
                playerVisuals?.SetFlipped(!walkingRight);

                // Move player toward target
                while (Vector3.Distance(player.transform.position, targetPosition) > 0.1f)
                {
                    Vector3 direction = (targetPosition - player.transform.position).normalized;
                    player.transform.position += direction * accusationWalkSpeed * Time.deltaTime;
                    yield return null;
                }

                // Snap to final position
                player.transform.position = targetPosition;
            }

            // Stop walking
            playerAnimator.SetExternalWalking(false);

            // 3. Face each other (player faces right, accused faces left)
            playerVisuals?.SetFlipped(false); // Face right
            accusedVisuals?.SetFlipped(true); // Face left

            // 4. Ensure player renders above accused
            var playerRenderers = player.GetComponentsInChildren<SpriteRenderer>();
            var accusedRenderers = accused.GetComponentsInChildren<SpriteRenderer>();
            int accusedMaxOrder = accusedRenderers.Max(r => r.sortingOrder);
            foreach (var r in playerRenderers)
            {
                r.sortingOrder = accusedMaxOrder + 10 + (r.sortingOrder % 10);
            }

            // 5. Play both animations simultaneously with faster frame time
            bool playerAnimDone = false;
            bool accusedAnimDone = false;

            // Unfreeze accused so their animation can play
            accusedAnimator.Unfreeze();

            playerAnimator.PlayOneShotAnimation(CharacterAnimationState.Accusing, accusationFrameTime, 3, () => playerAnimDone = true);
            accusedAnimator.PlayOneShotAnimation(CharacterAnimationState.Accused, accusationFrameTime, 2, () => accusedAnimDone = true);

            // Play accusation sound at accused character's position
            SoundManager.Instance?.PlayAccusation(accused.transform.position);

            // 6. Wait for both to complete with timeout
            float timeout = 5f; // Max wait time
            float elapsed = 0f;
            while ((!playerAnimDone || !accusedAnimDone) && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 7. Ensure both are frozen on last frame
            playerAnimator.FreezeAnimation();
            accusedAnimator.FreezeAnimation();
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
            // Reset camera zoom
            CameraController.Instance?.ResetZoom();

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
                    UnityEngine.Debug.Log("Correct! Target found!");
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
                    UnityEngine.Debug.Log($"Wrong guess! ({WrongGuesses}/{maxWrongGuesses})");
                }
                GameEvents.OnWrongGuess?.Invoke(character);

                if (WrongGuesses >= maxWrongGuesses)
                {
                    if (showDebugInfo)
                    {
                        UnityEngine.Debug.Log("Game Over - Too many wrong guesses!");
                    }
                    SoundManager.Instance?.PlayFailureJingle();
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
                UnityEngine.Debug.Log($"Game State: {newState}");
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
