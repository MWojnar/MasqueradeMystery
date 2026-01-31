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

        [Header("Body Sprites - Idle")]
        [SerializeField] private Sprite suitIdle;
        [SerializeField] private Sprite dressIdle;

        [Header("Body Sprites - Walk (4 frames)")]
        [SerializeField] private Sprite[] suitWalk;
        [SerializeField] private Sprite[] dressWalk;

        [Header("Body Sprites - Dance (4 frames)")]
        [SerializeField] private Sprite[] suitDance;
        [SerializeField] private Sprite[] dressDance;

        [Header("Accessory Sprites - Idle")]
        [SerializeField] private Sprite bowtieIdle;
        [SerializeField] private Sprite hairbowIdle;

        [Header("Accessory Sprites - Walk (4 frames)")]
        [SerializeField] private Sprite[] bowtieWalk;
        [SerializeField] private Sprite[] hairbowWalk;

        [Header("Accessory Sprites - Dance (4 frames)")]
        [SerializeField] private Sprite[] bowtieDance;
        [SerializeField] private Sprite[] hairbowDance;

        [Header("Non-Animal Mask Sprites - Idle")]
        [SerializeField] private Sprite plainEyesIdle;
        [SerializeField] private Sprite plainFullFaceIdle;
        [SerializeField] private Sprite crownedIdle;
        [SerializeField] private Sprite jesterIdle;

        [Header("Non-Animal Mask Sprites - Walk (4 frames)")]
        [SerializeField] private Sprite[] plainEyesWalk;
        [SerializeField] private Sprite[] plainFullFaceWalk;
        [SerializeField] private Sprite[] crownedWalk;
        [SerializeField] private Sprite[] jesterWalk;

        [Header("Non-Animal Mask Sprites - Dance (4 frames)")]
        [SerializeField] private Sprite[] plainEyesDance;
        [SerializeField] private Sprite[] plainFullFaceDance;
        [SerializeField] private Sprite[] crownedDance;
        [SerializeField] private Sprite[] jesterDance;

        [Header("Animal Mask Sprites - Idle")]
        [SerializeField] private Sprite foxIdle;
        [SerializeField] private Sprite rabbitIdle;
        [SerializeField] private Sprite sharkIdle;
        [SerializeField] private Sprite fishIdle;

        [Header("Animal Mask Sprites - Walk (4 frames)")]
        [SerializeField] private Sprite[] foxWalk;
        [SerializeField] private Sprite[] rabbitWalk;
        [SerializeField] private Sprite[] sharkWalk;
        [SerializeField] private Sprite[] fishWalk;

        [Header("Animal Mask Sprites - Dance (4 frames)")]
        [SerializeField] private Sprite[] foxDance;
        [SerializeField] private Sprite[] rabbitDance;
        [SerializeField] private Sprite[] sharkDance;
        [SerializeField] private Sprite[] fishDance;

        [Header("Highlight Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(1.2f, 1.2f, 1.2f, 1f);

        private CharacterData currentData;
        private CharacterAnimationState currentState = CharacterAnimationState.Idle;
        private int currentFrame;

        public void UpdateVisuals(CharacterData data)
        {
            currentData = data;
            currentState = CharacterAnimationState.Idle;
            currentFrame = 0;

            ApplySprites();
            UpdatePartnerIndicator(data.IsDancing);
        }

        public void SetAnimationState(CharacterAnimationState state, int frame)
        {
            if (currentData == null) return;

            currentState = state;
            currentFrame = frame;
            ApplySprites();
        }

        private void ApplySprites()
        {
            if (currentData == null) return;

            ApplyBodySprite();
            ApplyMaskSprite();
            ApplyAccessorySprite();
        }

        private void ApplyBodySprite()
        {
            if (bodyRenderer == null) return;

            bool isSuit = currentData.Clothing == ClothingType.Suit;

            Sprite sprite = currentState switch
            {
                CharacterAnimationState.Idle => isSuit ? suitIdle : dressIdle,
                CharacterAnimationState.Walking => GetFrameSprite(isSuit ? suitWalk : dressWalk, isSuit ? suitIdle : dressIdle),
                CharacterAnimationState.Dancing => GetFrameSprite(isSuit ? suitDance : dressDance, isSuit ? suitIdle : dressIdle),
                _ => isSuit ? suitIdle : dressIdle
            };

            bodyRenderer.sprite = sprite;
            bodyRenderer.color = normalColor;
        }

        private void ApplyMaskSprite()
        {
            if (maskRenderer == null) return;

            Sprite sprite = GetMaskSprite();
            maskRenderer.sprite = sprite;
            maskRenderer.color = normalColor;
            maskRenderer.gameObject.SetActive(sprite != null);
        }

        private Sprite GetMaskSprite()
        {
            var mask = currentData.Mask;

            if (mask.IsAnimalMask)
            {
                return mask.AnimalMask switch
                {
                    AnimalMaskType.Fox => GetMaskByState(foxIdle, foxWalk, foxDance),
                    AnimalMaskType.Rabbit => GetMaskByState(rabbitIdle, rabbitWalk, rabbitDance),
                    AnimalMaskType.Shark => GetMaskByState(sharkIdle, sharkWalk, sharkDance),
                    AnimalMaskType.Fish => GetMaskByState(fishIdle, fishWalk, fishDance),
                    _ => null
                };
            }
            else
            {
                return mask.NonAnimalMask switch
                {
                    NonAnimalMaskType.PlainEyes => GetMaskByState(plainEyesIdle, plainEyesWalk, plainEyesDance),
                    NonAnimalMaskType.PlainFullFace => GetMaskByState(plainFullFaceIdle, plainFullFaceWalk, plainFullFaceDance),
                    NonAnimalMaskType.Crowned => GetMaskByState(crownedIdle, crownedWalk, crownedDance),
                    NonAnimalMaskType.Jester => GetMaskByState(jesterIdle, jesterWalk, jesterDance),
                    _ => null
                };
            }
        }

        private Sprite GetMaskByState(Sprite idle, Sprite[] walk, Sprite[] dance)
        {
            return currentState switch
            {
                CharacterAnimationState.Idle => idle,
                CharacterAnimationState.Walking => GetFrameSprite(walk, idle),
                CharacterAnimationState.Dancing => GetFrameSprite(dance, idle),
                _ => idle
            };
        }

        private void ApplyAccessorySprite()
        {
            if (accessoryRenderer == null) return;

            Sprite sprite = null;

            if (currentData.HasBowtie)
            {
                sprite = currentState switch
                {
                    CharacterAnimationState.Idle => bowtieIdle,
                    CharacterAnimationState.Walking => GetFrameSprite(bowtieWalk, bowtieIdle),
                    CharacterAnimationState.Dancing => GetFrameSprite(bowtieDance, bowtieIdle),
                    _ => bowtieIdle
                };
            }
            else if (currentData.HasHairbow)
            {
                sprite = currentState switch
                {
                    CharacterAnimationState.Idle => hairbowIdle,
                    CharacterAnimationState.Walking => GetFrameSprite(hairbowWalk, hairbowIdle),
                    CharacterAnimationState.Dancing => GetFrameSprite(hairbowDance, hairbowIdle),
                    _ => hairbowIdle
                };
            }

            if (sprite != null)
            {
                accessoryRenderer.sprite = sprite;
                accessoryRenderer.color = normalColor;
                accessoryRenderer.gameObject.SetActive(true);
            }
            else
            {
                accessoryRenderer.gameObject.SetActive(false);
            }
        }

        private Sprite GetFrameSprite(Sprite[] frames, Sprite fallback)
        {
            if (frames == null || frames.Length == 0)
                return fallback;

            int index = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
            return frames[index] != null ? frames[index] : fallback;
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
