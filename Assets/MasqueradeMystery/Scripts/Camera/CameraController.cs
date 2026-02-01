using System.Collections;
using UnityEngine;

namespace MasqueradeMystery
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float edgePanThreshold = 50f; // pixels from screen edge
        [SerializeField] private float edgePanSpeedMultiplier = 0.6f;

        [Header("Bounds")]
        [Tooltip("If true, uses SceneBounds singleton. Otherwise uses manual bounds below.")]
        [SerializeField] private bool useSceneBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(10f, 5f);

        [Header("Edge Pan Settings")]
        [SerializeField] private bool enableEdgePan = true;

        [Header("Pan Settings")]
        [SerializeField] private float panDuration = 1f;

        [Header("Follow Settings")]
        [SerializeField] private bool followPlayer = true;

        private Camera cam;
        private Transform followTarget;
        private bool inputEnabled = true;
        private bool isPanning;
        private Vector3 defaultPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }

            // Store the initial position as the default/center position
            defaultPosition = transform.position;
        }

        private void Start()
        {
            GameEvents.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= OnGameStateChanged;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Title)
            {
                // Disable input and reset to center on title screen
                DisableInput();
                ResetToCenter();
            }
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        private void Update()
        {
            // Don't process input during pan or when disabled
            if (!inputEnabled || isPanning) return;

            // Follow player if enabled and target exists
            if (followPlayer && followTarget != null)
            {
                Vector3 targetPos = new Vector3(followTarget.position.x, followTarget.position.y, transform.position.z);
                targetPos = ClampToBounds(targetPos);
                transform.position = targetPos;
            }
            else
            {
                // Fallback to WASD/edge pan when no follow target
                Vector2 input = Vector2.zero;

                // WASD / Arrow key input (Legacy Input System)
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");

                // Edge panning (only if no keyboard input and enabled)
                if (enableEdgePan && input == Vector2.zero)
                {
                    input = GetEdgePanInput() * edgePanSpeedMultiplier;
                }

                // Normalize diagonal movement
                if (input.magnitude > 1f)
                {
                    input.Normalize();
                }

                // Apply movement
                Vector3 move = new Vector3(input.x, input.y, 0) * moveSpeed * Time.deltaTime;
                Vector3 newPos = transform.position + move;

                // Clamp to bounds (accounting for camera viewport)
                newPos = ClampToBounds(newPos);

                transform.position = newPos;
            }
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            Vector2 min, max;

            if (useSceneBounds && SceneBounds.Instance != null)
            {
                min = SceneBounds.Instance.Min;
                max = SceneBounds.Instance.Max;
            }
            else
            {
                min = minBounds;
                max = maxBounds;
            }

            // Calculate camera half-extents
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            // Adjust bounds to account for camera size
            // Camera center must stay far enough from edges that viewport doesn't exceed bounds
            float clampedMinX = min.x + halfWidth;
            float clampedMaxX = max.x - halfWidth;
            float clampedMinY = min.y + halfHeight;
            float clampedMaxY = max.y - halfHeight;

            // Handle case where bounds are smaller than camera viewport
            if (clampedMinX > clampedMaxX)
            {
                // Center horizontally if bounds are too narrow
                position.x = (min.x + max.x) / 2f;
            }
            else
            {
                position.x = Mathf.Clamp(position.x, clampedMinX, clampedMaxX);
            }

            if (clampedMinY > clampedMaxY)
            {
                // Center vertically if bounds are too short
                position.y = (min.y + max.y) / 2f;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, clampedMinY, clampedMaxY);
            }

            return position;
        }

        private Vector2 GetEdgePanInput()
        {
            Vector2 input = Vector2.zero;
            Vector3 mousePos = Input.mousePosition;

            // Check if mouse is within screen bounds
            if (mousePos.x < 0 || mousePos.x > Screen.width ||
                mousePos.y < 0 || mousePos.y > Screen.height)
            {
                return input;
            }

            // Horizontal edge detection
            if (mousePos.x < edgePanThreshold)
            {
                input.x = -1f;
            }
            else if (mousePos.x > Screen.width - edgePanThreshold)
            {
                input.x = 1f;
            }

            // Vertical edge detection
            if (mousePos.y < edgePanThreshold)
            {
                input.y = -1f;
            }
            else if (mousePos.y > Screen.height - edgePanThreshold)
            {
                input.y = 1f;
            }

            return input;
        }

        // Call this to set bounds dynamically (e.g., based on spawn area)
        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        // Pan camera smoothly to target position
        public Coroutine PanToTarget(Vector3 targetPosition, float duration = -1f)
        {
            if (duration < 0) duration = panDuration;
            return StartCoroutine(PanToTargetCoroutine(targetPosition, duration));
        }

        private IEnumerator PanToTargetCoroutine(Vector3 targetPosition, float duration)
        {
            isPanning = true;

            Vector3 start = transform.position;
            Vector3 end = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            end = ClampToBounds(end);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end;
            isPanning = false;
        }

        public void DisableInput()
        {
            inputEnabled = false;
        }

        public void EnableInput()
        {
            inputEnabled = true;
        }

        public void ResetToCenter()
        {
            transform.position = defaultPosition;
        }

        public bool IsInputEnabled => inputEnabled;
        public bool IsPanning => isPanning;

        // Visualize bounds in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Vector2 min, max;
            if (useSceneBounds && SceneBounds.Instance != null)
            {
                min = SceneBounds.Instance.Min;
                max = SceneBounds.Instance.Max;
            }
            else
            {
                min = minBounds;
                max = maxBounds;
            }

            Vector3 center = new Vector3(
                (min.x + max.x) / 2f,
                (min.y + max.y) / 2f,
                0
            );
            Vector3 size = new Vector3(
                max.x - min.x,
                max.y - min.y,
                0.1f
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}
