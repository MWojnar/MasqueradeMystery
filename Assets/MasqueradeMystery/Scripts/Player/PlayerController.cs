using UnityEngine;

namespace MasqueradeMystery
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;

        private CharacterAnimator animator;
        private CharacterVisuals visuals;

        private void Awake()
        {
            animator = GetComponent<CharacterAnimator>();
            visuals = GetComponent<CharacterVisuals>();
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

            Vector2 input = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            bool isMoving = input.sqrMagnitude > 0.01f;
            animator?.SetExternalWalking(isMoving);

            if (input.x != 0)
            {
                visuals?.SetFlipped(input.x < 0);
            }

            if (isMoving)
            {
                Vector3 movement = new Vector3(input.x, input.y, 0).normalized;
                Vector3 newPos = transform.position + movement * moveSpeed * Time.deltaTime;
                newPos = ClampToSceneBounds(newPos);
                transform.position = newPos;
            }
        }

        private Vector3 ClampToSceneBounds(Vector3 position)
        {
            if (SceneBounds.Instance != null)
            {
                var bounds = SceneBounds.Instance.Bounds;
                position.x = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax);
                position.y = Mathf.Clamp(position.y, bounds.yMin, bounds.yMax);
            }
            return position;
        }
    }
}
