using UnityEngine;
using TMPro;

namespace MasqueradeMystery
{
    public class HoverInfoUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text maskText;
        [SerializeField] private TMP_Text clothingText;
        [SerializeField] private TMP_Text accessoryText;
        [SerializeField] private TMP_Text danceText;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(20, -20);
        [SerializeField] private bool keepOnScreen = true;

        private RectTransform panelRect;
        private Canvas canvas;

        private void Awake()
        {
            if (panel != null)
            {
                panelRect = panel.GetComponent<RectTransform>();
                canvas = GetComponentInParent<Canvas>();
            }
        }

        private void Start()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            GameEvents.OnCharacterHoverStart += ShowInfo;
            GameEvents.OnCharacterHoverEnd += HideInfo;
        }

        private void OnDestroy()
        {
            GameEvents.OnCharacterHoverStart -= ShowInfo;
            GameEvents.OnCharacterHoverEnd -= HideInfo;
        }

        private void ShowInfo(Character character)
        {
            if (panel == null || character == null) return;

            panel.SetActive(true);
            UpdateText(character.Data);
        }

        private void HideInfo(Character character)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void Update()
        {
            if (panel != null && panel.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            Vector3 position = Input.mousePosition + (Vector3)offset;

            // Keep panel on screen
            if (keepOnScreen && panelRect != null)
            {
                Vector2 size = panelRect.sizeDelta;
                float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;

                // Adjust if going off right edge
                if (position.x + size.x * scaleFactor > Screen.width)
                {
                    position.x = Input.mousePosition.x - offset.x - size.x * scaleFactor;
                }

                // Adjust if going off bottom edge
                if (position.y - size.y * scaleFactor < 0)
                {
                    position.y = Input.mousePosition.y - offset.y + size.y * scaleFactor;
                }

                // Clamp to screen bounds
                position.x = Mathf.Clamp(position.x, 0, Screen.width - size.x * scaleFactor);
                position.y = Mathf.Clamp(position.y, size.y * scaleFactor, Screen.height);
            }

            panel.transform.position = position;
        }

        private void UpdateText(CharacterData data)
        {
            if (maskText != null)
            {
                maskText.text = GetMaskDescription(data.Mask);
            }

            if (clothingText != null)
            {
                clothingText.text = data.Clothing.ToString();
            }

            if (accessoryText != null)
            {
                accessoryText.text = GetAccessoryDescription(data);
            }

            if (danceText != null)
            {
                danceText.text = GetDanceDescription(data);
            }
        }

        private string GetMaskDescription(MaskIdentifier mask)
        {
            if (mask.IsAnimalMask)
            {
                string categories = "";
                if (mask.IsMammal) categories += "Mammal, ";
                if (mask.IsPredator) categories += "Predator, ";
                if (mask.IsAquatic) categories += "Aquatic, ";
                if (mask.IsPrey) categories += "Prey, ";
                categories = categories.TrimEnd(',', ' ');

                return $"{mask.AnimalMask} ({categories})";
            }
            else
            {
                string traits = "";
                if (mask.HasHat) traits += "Hat, ";
                if (mask.HasMouth) traits += "Mouth, ";
                traits = traits.TrimEnd(',', ' ');

                if (string.IsNullOrEmpty(traits))
                    traits = "No Hat, No Mouth";

                return $"{mask.NonAnimalMask} ({traits})";
            }
        }

        private string GetAccessoryDescription(CharacterData data)
        {
            if (data.HasBowtie) return "Bowtie";
            if (data.HasHairbow) return "Hairbow";
            return "None";
        }

        private string GetDanceDescription(CharacterData data)
        {
            return data.DanceState switch
            {
                DanceState.NotDancing => "Standing alone",
                DanceState.DancingWithSuitPartner => "Dancing with suit-wearer",
                DanceState.DancingWithDressPartner => "Dancing with dress-wearer",
                _ => "Unknown"
            };
        }
    }
}
