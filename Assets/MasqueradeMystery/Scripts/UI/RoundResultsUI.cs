using UnityEngine;
using TMPro;

namespace MasqueradeMystery
{
    public class RoundResultsUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text roundNumberText;
        [SerializeField] private TMP_Text guessCountText;
        [SerializeField] private TMP_Text targetIdentityText;
        [SerializeField] private TMP_Text consecutiveScoreText;
        [SerializeField] private TMP_Text promptText;

        [Header("Colors")]
        [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color failureColor = new Color(0.8f, 0.2f, 0.2f);

        [Header("Text Settings")]
        [SerializeField] private string successTitle = "Target Found!";
        [SerializeField] private string failureTitle = "Failed!";
        [SerializeField] private string continuePrompt = "Click or press Enter to continue";
        [SerializeField] private string returnPrompt = "Click or press Enter to return to title";

        private bool canAcceptInput;
        private bool wasSuccess;

        private void Start()
        {
            GameEvents.OnGameStateChanged += OnGameStateChanged;

            // Hide by default
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        }

        private void Update()
        {
            if (!canAcceptInput) return;
            if (panel == null || !panel.activeSelf) return;

            // Check for input
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
            {
                Continue();
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            // Hide when returning to title, transitioning, or playing
            // Keep visible during RoundEnding, Won, and Lost (results are shown during these)
            if (state == GameState.Title || state == GameState.Transitioning || state == GameState.Playing)
            {
                Hide();
            }
        }

        public void ShowResults(bool success, int roundNumber, int guessesUsed, int maxGuesses, CharacterData target, int consecutiveWins)
        {
            wasSuccess = success;

            if (panel != null)
            {
                panel.SetActive(true);
            }

            // Title
            if (resultTitleText != null)
            {
                resultTitleText.text = success ? successTitle : failureTitle;
                resultTitleText.color = success ? successColor : failureColor;
            }

            // Round number
            if (roundNumberText != null)
            {
                roundNumberText.text = $"Round {roundNumber}";
            }

            // Guesses
            if (guessCountText != null)
            {
                guessCountText.text = $"Guesses: {guessesUsed} / {maxGuesses}";
            }

            // Target identity
            if (targetIdentityText != null && target != null)
            {
                targetIdentityText.text = FormatTargetIdentity(target);
            }

            // Consecutive score (always show, but more prominent on failure)
            if (consecutiveScoreText != null)
            {
                if (success)
                {
                    consecutiveScoreText.text = $"Streak: {consecutiveWins}";
                }
                else
                {
                    consecutiveScoreText.text = $"Final Score: {consecutiveWins} consecutive rounds";
                }
            }

            // Prompt
            if (promptText != null)
            {
                promptText.text = success ? continuePrompt : returnPrompt;
            }

            // Delay before accepting input
            canAcceptInput = false;
            Invoke(nameof(EnableInput), 0.5f);
        }

        private string FormatTargetIdentity(CharacterData target)
        {
            string maskInfo = target.Mask.IsAnimalMask
                ? target.Mask.AnimalMask.ToString()
                : target.Mask.NonAnimalMask.ToString();

            string accessoryInfo = "None";
            if (target.HasBowtie) accessoryInfo = "Bowtie";
            else if (target.HasHairbow) accessoryInfo = "Hairbow";

            string danceInfo = target.DanceState switch
            {
                DanceState.NotDancing => "Not dancing",
                DanceState.DancingWithSuitPartner => "Dancing with suit-wearer",
                DanceState.DancingWithDressPartner => "Dancing with dress-wearer",
                _ => "Unknown"
            };

            return $"Mask: {maskInfo}\n" +
                   $"Clothing: {target.Clothing}\n" +
                   $"Accessory: {accessoryInfo}\n" +
                   $"Dance: {danceInfo}";
        }

        private void EnableInput()
        {
            canAcceptInput = true;
        }

        public void Hide()
        {
            canAcceptInput = false;

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void Continue()
        {
            canAcceptInput = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ContinueFromRoundEnd(wasSuccess);
            }
        }
    }
}
