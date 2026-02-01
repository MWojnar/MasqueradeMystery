using UnityEngine;
using TMPro;

namespace MasqueradeMystery
{
    public class GameStatusUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text guessesText;
        [SerializeField] private TMP_Text statusText;

        [Header("Settings")]
        [SerializeField] private string guessesFormat = "Guesses remaining: {0}";
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;

        private int maxGuesses = 3;
        private int wrongGuesses = 0;

        private void Start()
        {
            GameEvents.OnGameStateChanged += OnGameStateChanged;
            GameEvents.OnWrongGuess += OnWrongGuess;

            // Hide initially
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.OnWrongGuess -= OnWrongGuess;
        }

        public void Initialize(int maxWrongGuesses)
        {
            maxGuesses = maxWrongGuesses;
            wrongGuesses = 0;
            UpdateDisplay();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                if (panel != null) panel.SetActive(true);
                wrongGuesses = 0;
                UpdateDisplay();
            }
            else if (state == GameState.Title || state == GameState.Transitioning ||
                     state == GameState.RoundEnding || state == GameState.Won || state == GameState.Lost)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        private void OnWrongGuess(Character character)
        {
            wrongGuesses++;
            UpdateDisplay();

            // Show feedback
            if (statusText != null)
            {
                statusText.text = "Wrong! Try again...";
                statusText.gameObject.SetActive(true);

                // Could add animation or auto-hide
                CancelInvoke(nameof(HideStatus));
                Invoke(nameof(HideStatus), 2f);
            }
        }

        private void HideStatus()
        {
            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
        }

        private void UpdateDisplay()
        {
            int remaining = maxGuesses - wrongGuesses;

            if (guessesText != null)
            {
                guessesText.text = string.Format(guessesFormat, remaining);

                // Color based on remaining guesses
                if (remaining <= 1)
                    guessesText.color = dangerColor;
                else if (remaining <= 2)
                    guessesText.color = warningColor;
                else
                    guessesText.color = normalColor;
            }
        }
    }
}
