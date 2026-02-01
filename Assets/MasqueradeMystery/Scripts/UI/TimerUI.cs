using UnityEngine;
using TMPro;

namespace MasqueradeMystery
{
    public class TimerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text timerText;

        [Header("Color Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color dangerColor = Color.red;
        [SerializeField] private float dangerThreshold = 10f;

        private void Start()
        {
            GameEvents.OnTimerTick += OnTimerTick;
            GameEvents.OnGameStateChanged += OnGameStateChanged;

            // Hide by default
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnTimerTick -= OnTimerTick;
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnTimerTick(float timeRemaining)
        {
            UpdateDisplay(timeRemaining);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (panel != null)
            {
                panel.SetActive(state == GameState.Playing);
            }
        }

        private void UpdateDisplay(float timeRemaining)
        {
            if (timerText == null) return;

            // Display as integer seconds
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = seconds.ToString();

            // Color based on time remaining - red at 10 seconds or less
            if (timeRemaining <= dangerThreshold)
            {
                timerText.color = dangerColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
    }
}
