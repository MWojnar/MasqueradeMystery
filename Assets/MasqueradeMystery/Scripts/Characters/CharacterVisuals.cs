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

        [Header("Body Sprites")]
        [SerializeField] private Sprite suitSprite;      // Frame 0
        [SerializeField] private Sprite dressSprite;     // Frame 2

        [Header("Accessory Sprites")]
        [SerializeField] private Sprite bowtieSprite;    // Frame 1
        [SerializeField] private Sprite hairbowSprite;   // Frame 3

        [Header("Mask Sprites - Non-Animal")]
        [SerializeField] private Sprite plainEyesSprite;     // Frame 4
        [SerializeField] private Sprite plainFullFaceSprite; // Frame 5
        [SerializeField] private Sprite crownedSprite;       // Frame 6
        [SerializeField] private Sprite jesterSprite;        // Frame 7

        [Header("Mask Sprites - Animal")]
        [SerializeField] private Sprite foxSprite;       // Frame 8
        [SerializeField] private Sprite rabbitSprite;    // Frame 9
        [SerializeField] private Sprite sharkSprite;     // Frame 10
        [SerializeField] private Sprite fishSprite;      // Frame 11

        [Header("Highlight Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(1.2f, 1.2f, 1.2f, 1f);

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

            bodyRenderer.sprite = clothing == ClothingType.Suit ? suitSprite : dressSprite;
            bodyRenderer.color = normalColor;
        }

        private void UpdateMask(MaskIdentifier mask)
        {
            if (maskRenderer == null) return;

            Sprite maskSprite = null;

            if (mask.IsAnimalMask)
            {
                maskSprite = mask.AnimalMask switch
                {
                    AnimalMaskType.Fox => foxSprite,
                    AnimalMaskType.Rabbit => rabbitSprite,
                    AnimalMaskType.Shark => sharkSprite,
                    AnimalMaskType.Fish => fishSprite,
                    _ => null
                };
            }
            else
            {
                maskSprite = mask.NonAnimalMask switch
                {
                    NonAnimalMaskType.PlainEyes => plainEyesSprite,
                    NonAnimalMaskType.PlainFullFace => plainFullFaceSprite,
                    NonAnimalMaskType.Crowned => crownedSprite,
                    NonAnimalMaskType.Jester => jesterSprite,
                    _ => null
                };
            }

            maskRenderer.sprite = maskSprite;
            maskRenderer.color = normalColor;
            maskRenderer.gameObject.SetActive(maskSprite != null);
        }

        private void UpdateAccessory(CharacterData data)
        {
            if (accessoryRenderer == null) return;

            if (data.HasBowtie && bowtieSprite != null)
            {
                accessoryRenderer.sprite = bowtieSprite;
                accessoryRenderer.color = normalColor;
                accessoryRenderer.gameObject.SetActive(true);
            }
            else if (data.HasHairbow && hairbowSprite != null)
            {
                accessoryRenderer.sprite = hairbowSprite;
                accessoryRenderer.color = normalColor;
                accessoryRenderer.gameObject.SetActive(true);
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

        public void SetHighlight(bool highlighted)
        {
            Color color = highlighted ? highlightColor : normalColor;

            if (bodyRenderer != null)
                bodyRenderer.color = color;
            if (maskRenderer != null)
                maskRenderer.color = color;
            if (accessoryRenderer != null && accessoryRenderer.gameObject.activeSelf)
                accessoryRenderer.color = color;
        }

        public void SetFlipped(bool flipped)
        {
            if (bodyRenderer != null)
                bodyRenderer.flipX = flipped;
            if (maskRenderer != null)
                maskRenderer.flipX = flipped;
            if (accessoryRenderer != null)
                accessoryRenderer.flipX = flipped;
        }
    }
}
