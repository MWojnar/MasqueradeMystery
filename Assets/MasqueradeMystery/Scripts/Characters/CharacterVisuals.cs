using UnityEngine;

namespace MasqueradeMystery
{
    public class CharacterVisuals : MonoBehaviour
    {
        [Header("Sprite Renderers")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer maskRenderer;
        [SerializeField] private SpriteRenderer accessoryRenderer;
        [SerializeField] private SpriteRenderer partnerLineRenderer;

        [Header("Placeholder Colors - Clothing")]
        [SerializeField] private Color suitColor = new Color(0.2f, 0.2f, 0.4f);
        [SerializeField] private Color dressColor = new Color(0.6f, 0.2f, 0.4f);

        [Header("Placeholder Colors - Animal Masks")]
        [SerializeField] private Color foxColor = new Color(1f, 0.5f, 0.2f);
        [SerializeField] private Color rabbitColor = new Color(1f, 0.7f, 0.8f);
        [SerializeField] private Color sharkColor = new Color(0.5f, 0.5f, 0.6f);
        [SerializeField] private Color fishColor = new Color(0.3f, 0.6f, 1f);

        [Header("Placeholder Colors - Non-Animal Masks")]
        [SerializeField] private Color plainEyesColor = new Color(0.95f, 0.95f, 0.95f);
        [SerializeField] private Color plainFullFaceColor = new Color(0.95f, 0.9f, 0.8f);
        [SerializeField] private Color crownedColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color jesterColor = new Color(0.6f, 0.2f, 0.8f);

        [Header("Placeholder Colors - Accessories")]
        [SerializeField] private Color bowtieColor = new Color(0.8f, 0.1f, 0.1f);
        [SerializeField] private Color hairbowColor = new Color(1f, 0.4f, 0.6f);

        public void UpdateVisuals(CharacterData data)
        {
            UpdateBody(data.Clothing);
            UpdateMask(data.Mask);
            UpdateAccessory(data);
            UpdatePartnerIndicator(data.IsDancing);
        }

        private void UpdateBody(ClothingType clothing)
        {
            if (bodyRenderer == null) return;

            bodyRenderer.color = clothing == ClothingType.Suit ? suitColor : dressColor;
        }

        private void UpdateMask(MaskIdentifier mask)
        {
            if (maskRenderer == null) return;

            Color maskColor;
            if (mask.IsAnimalMask)
            {
                maskColor = mask.AnimalMask switch
                {
                    AnimalMaskType.Fox => foxColor,
                    AnimalMaskType.Rabbit => rabbitColor,
                    AnimalMaskType.Shark => sharkColor,
                    AnimalMaskType.Fish => fishColor,
                    _ => Color.white
                };
            }
            else
            {
                maskColor = mask.NonAnimalMask switch
                {
                    NonAnimalMaskType.PlainEyes => plainEyesColor,
                    NonAnimalMaskType.PlainFullFace => plainFullFaceColor,
                    NonAnimalMaskType.Crowned => crownedColor,
                    NonAnimalMaskType.Jester => jesterColor,
                    _ => Color.white
                };
            }

            maskRenderer.color = maskColor;
        }

        private void UpdateAccessory(CharacterData data)
        {
            if (accessoryRenderer == null) return;

            if (data.HasBowtie)
            {
                accessoryRenderer.gameObject.SetActive(true);
                accessoryRenderer.color = bowtieColor;
            }
            else if (data.HasHairbow)
            {
                accessoryRenderer.gameObject.SetActive(true);
                accessoryRenderer.color = hairbowColor;
            }
            else
            {
                accessoryRenderer.gameObject.SetActive(false);
            }
        }

        private void UpdatePartnerIndicator(bool isDancing)
        {
            if (partnerLineRenderer != null)
            {
                partnerLineRenderer.gameObject.SetActive(isDancing);
            }
        }

        // Call this to highlight character (e.g., on hover)
        public void SetHighlight(bool highlighted)
        {
            if (bodyRenderer != null)
            {
                // Simple brightness increase for highlight
                float multiplier = highlighted ? 1.3f : 1f;
                // Could implement more sophisticated highlighting here
            }
        }
    }
}
