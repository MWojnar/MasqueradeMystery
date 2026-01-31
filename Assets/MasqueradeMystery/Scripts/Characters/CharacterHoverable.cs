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

            // Ensure we have a collider for mouse detection
            var collider = GetComponent<Collider2D>();
            if (collider == null)
            {
                Debug.LogWarning($"CharacterHoverable on {gameObject.name} requires a Collider2D!");
            }
        }

        private void OnMouseEnter()
        {
            if (character == null) return;

            isHovered = true;
            GameEvents.OnCharacterHoverStart?.Invoke(character);

            if (visuals != null)
            {
                visuals.SetHighlight(true);
            }
        }

        private void OnMouseExit()
        {
            if (character == null) return;

            isHovered = false;
            GameEvents.OnCharacterHoverEnd?.Invoke(character);

            if (visuals != null)
            {
                visuals.SetHighlight(false);
            }
        }

        private void OnMouseDown()
        {
            if (character == null) return;

            GameEvents.OnCharacterClicked?.Invoke(character);
        }

        // Cleanup on disable
        private void OnDisable()
        {
            if (isHovered && character != null)
            {
                GameEvents.OnCharacterHoverEnd?.Invoke(character);
                isHovered = false;
            }
        }
    }
}
