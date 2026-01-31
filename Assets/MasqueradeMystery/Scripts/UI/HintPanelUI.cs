using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MasqueradeMystery
{
    public class HintPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform hintContainer;
        [SerializeField] private TMP_Text hintTextPrefab;
        [SerializeField] private TMP_Text titleText;

        [Header("Settings")]
        [SerializeField] private string titleFormat = "Find the target:";

        private List<TMP_Text> hintTexts = new List<TMP_Text>();

        private void Start()
        {
            GameEvents.OnHintsGenerated += DisplayHints;
            GameEvents.OnGameStateChanged += OnGameStateChanged;

            if (titleText != null)
            {
                titleText.text = titleFormat;
            }

            // Hide initially
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnHintsGenerated -= DisplayHints;
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        }

        private void DisplayHints(List<Hint> hints)
        {
            ClearHints();

            if (panel != null)
            {
                panel.SetActive(true);
            }

            foreach (var hint in hints)
            {
                CreateHintText(hint);
            }
        }

        private void CreateHintText(Hint hint)
        {
            if (hintTextPrefab == null || hintContainer == null) return;

            TMP_Text hintText = Instantiate(hintTextPrefab, hintContainer);
            hintText.text = "• " + hint.DisplayText;
            hintText.gameObject.SetActive(true);
            hintTexts.Add(hintText);
        }

        private void ClearHints()
        {
            foreach (var text in hintTexts)
            {
                if (text != null)
                {
                    Destroy(text.gameObject);
                }
            }
            hintTexts.Clear();
        }

        private void OnGameStateChanged(GameState state)
        {
            // Could hide/show based on state
            if (state == GameState.Menu)
            {
                if (panel != null) panel.SetActive(false);
                ClearHints();
            }
        }
    }
}
