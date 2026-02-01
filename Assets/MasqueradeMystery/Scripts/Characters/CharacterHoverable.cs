using UnityEngine;

namespace MasqueradeMystery
{
    [RequireComponent(typeof(Collider2D))]
    public class CharacterHoverable : MonoBehaviour
    {
        private Character character;
        private CharacterVisuals visuals;
        private bool isHovered;

        private void Awake()
        {
            character = GetComponent<Character>();
            visuals = GetComponent<CharacterVisuals>();

            var collider = GetComponent<Collider2D>();
            if (collider == null)
            {
                Debug.LogWarning($"CharacterHoverable on {gameObject.name} requires a Collider2D!");
            }
        }

        private void OnMouseEnter()
        {
            if (character == null) return;

            // Only show hover effects during gameplay
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            isHovered = true;
            GameEvents.OnCharacterHoverStart?.Invoke(character);

            if (visuals != null)
            {
                visuals.SetOutline(true);
            }
        }

        private void OnMouseExit()
        {
            if (character == null) return;

            isHovered = false;
            GameEvents.OnCharacterHoverEnd?.Invoke(character);

            if (visuals != null)
            {
                visuals.SetOutline(false);
            }
        }

        private void OnMouseDown()
        {
            if (character == null) return;

            // Only allow clicks during gameplay
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            GameEvents.OnCharacterClicked?.Invoke(character);
        }

        private void OnDisable()
        {
            if (isHovered && character != null)
            {
                GameEvents.OnCharacterHoverEnd?.Invoke(character);

                if (visuals != null)
                {
                    visuals.SetOutline(false);
                }

                isHovered = false;
            }
        }
    }
}
