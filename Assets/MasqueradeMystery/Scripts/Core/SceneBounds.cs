using UnityEngine;

namespace MasqueradeMystery
{
    public class SceneBounds : MonoBehaviour
    {
        public static SceneBounds Instance { get; private set; }

        [Header("Bounds Source")]
        [Tooltip("If set, bounds are derived from this sprite's size. Otherwise uses manual bounds.")]
        [SerializeField] private SpriteRenderer backgroundSprite;

        [Header("Manual Bounds (used if no background sprite)")]
        [SerializeField] private Rect manualBounds = new Rect(-10, -5, 20, 10);

        [Header("Character Margins (inset from bounds)")]
        [SerializeField] private float marginTop = 5f;
        [SerializeField] private float marginBottom = 1f;
        [SerializeField] private float marginLeft = 1f;
        [SerializeField] private float marginRight = 1f;

        public Rect Bounds
        {
            get
            {
                if (backgroundSprite != null && backgroundSprite.sprite != null)
                {
                    return GetBoundsFromSprite();
                }
                return manualBounds;
            }
        }

        public Vector2 Min => new Vector2(Bounds.xMin, Bounds.yMin);
        public Vector2 Max => new Vector2(Bounds.xMax, Bounds.yMax);

        /// <summary>
        /// Bounds with character margins applied (for player/NPC movement).
        /// </summary>
        public Rect CharacterBounds
        {
            get
            {
                var b = Bounds;
                return new Rect(
                    b.xMin + marginLeft,
                    b.yMin + marginBottom,
                    b.width - marginLeft - marginRight,
                    b.height - marginBottom - marginTop
                );
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple SceneBounds instances found. Using the first one.");
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private Rect GetBoundsFromSprite()
        {
            var bounds = backgroundSprite.bounds;
            return new Rect(
                bounds.min.x,
                bounds.min.y,
                bounds.size.x,
                bounds.size.y
            );
        }

        public Vector2 ClampPosition(Vector2 position)
        {
            var bounds = Bounds;
            return new Vector2(
                Mathf.Clamp(position.x, bounds.xMin, bounds.xMax),
                Mathf.Clamp(position.y, bounds.yMin, bounds.yMax)
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            var bounds = Bounds;
            Vector3 center = new Vector3(bounds.center.x, bounds.center.y, 0);
            Vector3 size = new Vector3(bounds.width, bounds.height, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
