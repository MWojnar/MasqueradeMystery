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

        [Header("Body Sprites - Accusing (5 frames)")]
        [SerializeField] private Sprite[] suitAccusing;
        [SerializeField] private Sprite[] dressAccusing;

        [Header("Body Sprites - Accused (5 frames)")]
        [SerializeField] private Sprite[] suitAccused;
        [SerializeField] private Sprite[] dressAccused;

        [Header("Accessory Sprites - Idle")]
        [SerializeField] private Sprite bowtieIdle;
        [SerializeField] private Sprite hairbowIdle;

        [Header("Accessory Sprites - Walk (4 frames)")]
        [SerializeField] private Sprite[] bowtieWalk;
        [SerializeField] private Sprite[] hairbowWalk;

        [Header("Accessory Sprites - Dance (4 frames)")]
        [SerializeField] private Sprite[] bowtieDance;
        [SerializeField] private Sprite[] hairbowDance;

        [Header("Accessory Sprites - Accusing (5 frames)")]
        [SerializeField] private Sprite[] bowtieAccusing;
        [SerializeField] private Sprite[] hairbowAccusing;

        [Header("Accessory Sprites - Accused (5 frames)")]
        [SerializeField] private Sprite[] bowtieAccused;
        [SerializeField] private Sprite[] hairbowAccused;

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

        [Header("Non-Animal Mask Sprites - Accusing (5 frames)")]
        [SerializeField] private Sprite[] plainEyesAccusing;
        [SerializeField] private Sprite[] plainFullFaceAccusing;
        [SerializeField] private Sprite[] crownedAccusing;
        [SerializeField] private Sprite[] jesterAccusing;

        [Header("Non-Animal Mask Sprites - Accused (5 frames)")]
        [SerializeField] private Sprite[] plainEyesAccused;
        [SerializeField] private Sprite[] plainFullFaceAccused;
        [SerializeField] private Sprite[] crownedAccused;
        [SerializeField] private Sprite[] jesterAccused;

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

        [Header("Animal Mask Sprites - Accusing (5 frames)")]
        [SerializeField] private Sprite[] foxAccusing;
        [SerializeField] private Sprite[] rabbitAccusing;
        [SerializeField] private Sprite[] sharkAccusing;
        [SerializeField] private Sprite[] fishAccusing;

        [Header("Animal Mask Sprites - Accused (5 frames)")]
        [SerializeField] private Sprite[] foxAccused;
        [SerializeField] private Sprite[] rabbitAccused;
        [SerializeField] private Sprite[] sharkAccused;
        [SerializeField] private Sprite[] fishAccused;

        [Header("Outline Settings")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Color outlineColor = Color.red;

        [Header("Highlight Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(1.2f, 1.2f, 1.2f, 1f);

        // Material instance for outline control (body only)
        private Material bodyMaterialInstance;

        private static readonly int OutlineEnabledProperty = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");

        private CharacterData currentData;
        private CharacterAnimationState currentState = CharacterAnimationState.Idle;
        private int currentFrame;

        private void Awake()
        {
            InitializeMaterials();
        }

        private void InitializeMaterials()
        {
            if (outlineMaterial == null) return;

            // Only apply outline material to body renderer
            // Mask and accessory sit on top of body, so body outline represents the silhouette
            // Applying to all would cause internal edge outlines where sprites overlap
            if (bodyRenderer != null)
            {
                bodyMaterialInstance = new Material(outlineMaterial);
                bodyMaterialInstance.SetColor(OutlineColorProperty, outlineColor);
                bodyRenderer.material = bodyMaterialInstance;
            }
        }

        private void OnDestroy()
        {
            // Clean up material instance
            if (bodyMaterialInstance != null) Destroy(bodyMaterialInstance);
        }

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
                CharacterAnimationState.Accusing => GetFrameSprite(isSuit ? suitAccusing : dressAccusing, isSuit ? suitIdle : dressIdle),
                CharacterAnimationState.Accused => GetFrameSprite(isSuit ? suitAccused : dressAccused, isSuit ? suitIdle : dressIdle),
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
                    AnimalMaskType.Fox => GetMaskByState(foxIdle, foxWalk, foxDance, foxAccusing, foxAccused),
                    AnimalMaskType.Rabbit => GetMaskByState(rabbitIdle, rabbitWalk, rabbitDance, rabbitAccusing, rabbitAccused),
                    AnimalMaskType.Shark => GetMaskByState(sharkIdle, sharkWalk, sharkDance, sharkAccusing, sharkAccused),
                    AnimalMaskType.Fish => GetMaskByState(fishIdle, fishWalk, fishDance, fishAccusing, fishAccused),
                    _ => null
                };
            }
            else
            {
                return mask.NonAnimalMask switch
                {
                    NonAnimalMaskType.PlainEyes => GetMaskByState(plainEyesIdle, plainEyesWalk, plainEyesDance, plainEyesAccusing, plainEyesAccused),
                    NonAnimalMaskType.PlainFullFace => GetMaskByState(plainFullFaceIdle, plainFullFaceWalk, plainFullFaceDance, plainFullFaceAccusing, plainFullFaceAccused),
                    NonAnimalMaskType.Crowned => GetMaskByState(crownedIdle, crownedWalk, crownedDance, crownedAccusing, crownedAccused),
                    NonAnimalMaskType.Jester => GetMaskByState(jesterIdle, jesterWalk, jesterDance, jesterAccusing, jesterAccused),
                    _ => null
                };
            }
        }

        private Sprite GetMaskByState(Sprite idle, Sprite[] walk, Sprite[] dance, Sprite[] accusing = null, Sprite[] accused = null)
        {
            return currentState switch
            {
                CharacterAnimationState.Idle => idle,
                CharacterAnimationState.Walking => GetFrameSprite(walk, idle),
                CharacterAnimationState.Dancing => GetFrameSprite(dance, idle),
                CharacterAnimationState.Accusing => GetFrameSprite(accusing, idle),
                CharacterAnimationState.Accused => GetFrameSprite(accused, idle),
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
                    CharacterAnimationState.Accusing => GetFrameSprite(bowtieAccusing, bowtieIdle),
                    CharacterAnimationState.Accused => GetFrameSprite(bowtieAccused, bowtieIdle),
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
                    CharacterAnimationState.Accusing => GetFrameSprite(hairbowAccusing, hairbowIdle),
                    CharacterAnimationState.Accused => GetFrameSprite(hairbowAccused, hairbowIdle),
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

        public void SetOutline(bool enabled)
        {
            if (bodyMaterialInstance != null)
                bodyMaterialInstance.SetFloat(OutlineEnabledProperty, enabled ? 1f : 0f);
        }

        public void SetOutlineColor(Color color)
        {
            if (bodyMaterialInstance != null)
                bodyMaterialInstance.SetColor(OutlineColorProperty, color);
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
