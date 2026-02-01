using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MasqueradeMystery
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        [Header("Win Settings")]
        [SerializeField] private string winTitle = "Target Found!";
        [SerializeField] private string winMessage = "Excellent work! You identified the target.";
        [SerializeField] private Color winColor = new Color(0.2f, 0.8f, 0.2f);

        [Header("Lose Settings")]
        [SerializeField] private string loseTitle = "Mission Failed";
        [SerializeField] private string loseMessage = "Too many wrong guesses. The target escaped.";
        [SerializeField] private Color loseColor = new Color(0.8f, 0.2f, 0.2f);

        public System.Action OnRestartRequested;
        public System.Action OnQuitRequested;

        private void Start()
        {
            GameEvents.OnGameStateChanged += OnGameStateChanged;

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestart);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(HandleQuit);
            }

            // Hide initially
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            // GameOverUI is now disabled - RoundResultsUI handles all end-of-round display
            // Keep hidden for all states
            if (state == GameState.Playing || state == GameState.Title ||
                state == GameState.Transitioning || state == GameState.RoundEnding ||
                state == GameState.Won || state == GameState.Lost)
            {
                Hide();
            }
        }

        private void ShowWin()
        {
            if (panel != null) panel.SetActive(true);

            if (titleText != null)
            {
                titleText.text = winTitle;
                titleText.color = winColor;
            }

            if (messageText != null)
            {
                messageText.text = winMessage;
            }
        }

        private void ShowLose()
        {
            if (panel != null) panel.SetActive(true);

            if (titleText != null)
            {
                titleText.text = loseTitle;
                titleText.color = loseColor;
            }

            if (messageText != null)
            {
                messageText.text = loseMessage;
            }
        }

        private void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void HandleRestart()
        {
            OnRestartRequested?.Invoke();
        }

        private void HandleQuit()
        {
            OnQuitRequested?.Invoke();
        }
    }
}
