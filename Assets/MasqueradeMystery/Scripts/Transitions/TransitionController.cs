using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeMystery
{
    public class TransitionController : MonoBehaviour
    {
        public static TransitionController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CanvasGroup fadePanel;
        [SerializeField] private Image fadeImage;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize fade panel to transparent
            if (fadePanel != null)
            {
                fadePanel.alpha = 0f;
                fadePanel.blocksRaycasts = false;
            }

            if (fadeImage != null)
            {
                fadeImage.color = fadeColor;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public Coroutine FadeOut(Action onComplete = null)
        {
            return StartCoroutine(FadeOutCoroutine(onComplete));
        }

        public Coroutine FadeIn(Action onComplete = null)
        {
            return StartCoroutine(FadeInCoroutine(onComplete));
        }

        public Coroutine TransitionWithCallback(Action midTransitionAction, Action onComplete = null)
        {
            return StartCoroutine(TransitionCoroutine(midTransitionAction, onComplete));
        }

        private IEnumerator FadeOutCoroutine(Action onComplete)
        {
            IsTransitioning = true;

            if (fadePanel != null)
            {
                fadePanel.blocksRaycasts = true;

                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                    yield return null;
                }
                fadePanel.alpha = 1f;
            }

            IsTransitioning = false;
            onComplete?.Invoke();
        }

        private IEnumerator FadeInCoroutine(Action onComplete)
        {
            IsTransitioning = true;

            if (fadePanel != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    yield return null;
                }
                fadePanel.alpha = 0f;
                fadePanel.blocksRaycasts = false;
            }

            IsTransitioning = false;
            onComplete?.Invoke();
        }

        private IEnumerator TransitionCoroutine(Action midTransitionAction, Action onComplete)
        {
            IsTransitioning = true;

            // Fade out
            if (fadePanel != null)
            {
                fadePanel.blocksRaycasts = true;

                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                    yield return null;
                }
                fadePanel.alpha = 1f;
            }

            // Execute mid-transition action
            midTransitionAction?.Invoke();

            // Brief pause at full black
            yield return new WaitForSeconds(0.1f);

            // Fade in
            if (fadePanel != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    yield return null;
                }
                fadePanel.alpha = 0f;
                fadePanel.blocksRaycasts = false;
            }

            IsTransitioning = false;
            onComplete?.Invoke();
        }

        public void SetFadeImmediate(float alpha)
        {
            if (fadePanel != null)
            {
                fadePanel.alpha = alpha;
                fadePanel.blocksRaycasts = alpha > 0.5f;
            }
        }
    }
}
