using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MasqueradeMystery
{
    public class TitleScreenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private Image logoImage;

        [Header("Settings")]
        [SerializeField] private string gameTitle = "Masquerade Mystery";
        [SerializeField] private string startPrompt = "Click or press Enter to start";

        private bool canAcceptInput;

        private void Start()
        {
            GameEvents.OnGameStateChanged += OnGameStateChanged;

            // Use logo image if assigned, otherwise fall back to title text
            if (logoImage != null && logoImage.sprite != null)
            {
                logoImage.gameObject.SetActive(true);
                if (titleText != null)
                {
                    titleText.gameObject.SetActive(false);
                }
            }
            else
            {
                if (logoImage != null)
                {
                    logoImage.gameObject.SetActive(false);
                }
                if (titleText != null)
                {
                    titleText.text = gameTitle;
                }
            }

            if (promptText != null)
            {
                promptText.text = startPrompt;
            }

            // Check initial state
            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
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
                StartGame();
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            bool showTitle = state == GameState.Title;

            if (panel != null)
            {
                panel.SetActive(showTitle);
            }

            // Small delay before accepting input to prevent accidental clicks
            if (showTitle)
            {
                canAcceptInput = false;
                Invoke(nameof(EnableInput), 0.3f);
            }
            else
            {
                canAcceptInput = false;
            }
        }

        private void EnableInput()
        {
            canAcceptInput = true;
        }

        private void StartGame()
        {
            canAcceptInput = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGameFromTitle();
            }
        }
    }
}
