using UnityEngine;

namespace MasqueradeMystery
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float edgePanThreshold = 50f; // pixels from screen edge
        [SerializeField] private float edgePanSpeedMultiplier = 0.6f;

        [Header("Bounds")]
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(10f, 5f);

        [Header("Edge Pan Settings")]
        [SerializeField] private bool enableEdgePan = true;

        private void Update()
        {
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

            // Clamp to bounds
            newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
            newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);

            transform.position = newPos;
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

        // Visualize bounds in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) / 2f,
                (minBounds.y + maxBounds.y) / 2f,
                0
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0.1f
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}
