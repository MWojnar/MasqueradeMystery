using UnityEngine;

namespace MasqueradeMystery
{
    public class CharacterAnimator : MonoBehaviour
    {
        private const int FrameCount = 5;
        private const float DefaultFrameTime = 0.3f; // 300ms

        [Header("Animation Settings")]
        [SerializeField] private float frameTime = DefaultFrameTime;

        [Header("Walking Behavior")]
        [Tooltip("Chance per second for a non-dancing character to start walking")]
        [SerializeField] private float walkChance = 0.1f;
        [Tooltip("Maximum distance a character will walk from their original position")]
        [SerializeField] private float walkRadius = 2f;
        [Tooltip("Movement speed while walking")]
        [SerializeField] private float walkSpeed = 2f;
        [Tooltip("Minimum distance to maintain from other characters")]
        [SerializeField] private float minDistanceFromOthers = 1.5f;
        [Tooltip("Max attempts to find a valid walk target before cancelling")]
        [SerializeField] private int maxWalkAttempts = 10;

        [Header("Dance Sway")]
        [Tooltip("How far left/right dancers sway during animation")]
        [SerializeField] private float swayAmount = 0.15f;

        private CharacterVisuals visuals;
        private Collider2D myCollider;
        private CharacterAnimationState state = CharacterAnimationState.Idle;
        private int currentFrame;
        private float frameTimer;
        private bool pingPongReverse; // Direction for ping-pong animation
        private bool isRightPartner;  // Determines starting frame for dance
        private bool hasPartner;

        // Walking state
        private Vector3 originalPosition;
        private Vector3 walkTarget;
        private bool isWalking;
        private float walkDecisionTimer;

        // Dance sway state
        private Vector3 danceBasePosition;
        private float currentSwayOffset;
        private float targetSwayOffset;

        private void Awake()
        {
            visuals = GetComponent<CharacterVisuals>();
            myCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            originalPosition = transform.position;
        }

        public void SetDancing(bool dancing, bool rightPartner = false)
        {
            hasPartner = dancing;
            isRightPartner = rightPartner;

            if (dancing)
            {
                state = CharacterAnimationState.Dancing;
                currentFrame = FrameCount - 1;
                pingPongReverse = true;

                // Store base position for sway
                danceBasePosition = transform.position;

                // Initialize sway position based on starting frame
                float frameProgress = (float)currentFrame / (FrameCount - 1);
                targetSwayOffset = Mathf.Lerp(-swayAmount, swayAmount, frameProgress);
                currentSwayOffset = targetSwayOffset;
            }
            else
            {
                state = CharacterAnimationState.Idle;
                currentFrame = 0;
            }

            UpdateVisuals();
        }

        public bool IsDancing => state == CharacterAnimationState.Dancing;

        private void Update()
        {
            switch (state)
            {
                case CharacterAnimationState.Idle:
                    UpdateIdleBehavior();
                    break;
                case CharacterAnimationState.Walking:
                    UpdateWalking();
                    UpdateFrameAnimation(loop: true);
                    break;
                case CharacterAnimationState.Dancing:
                    UpdateFrameAnimation(loop: false); // Ping-pong, not simple loop
                    UpdateDanceSway();
                    break;
            }
        }

        private void UpdateIdleBehavior()
        {
            // Only non-partnered characters can walk
            if (hasPartner) return;

            walkDecisionTimer += Time.deltaTime;

            // Check once per second if we should start walking
            if (walkDecisionTimer >= 1f)
            {
                walkDecisionTimer = 0f;

                if (Random.value < walkChance)
                {
                    StartWalking();
                }
            }
        }

        private void StartWalking()
        {
            // Try to find a valid walk target
            Vector3? validTarget = FindValidWalkTarget();

            if (!validTarget.HasValue)
            {
                // Couldn't find a valid spot, stay idle
                return;
            }

            walkTarget = validTarget.Value;

            isWalking = true;
            state = CharacterAnimationState.Walking;
            currentFrame = 0;
            frameTimer = 0f;

            // Face the direction we're walking
            bool walkingRight = walkTarget.x > transform.position.x;
            visuals?.SetFlipped(!walkingRight); // Flip when walking left

            UpdateVisuals();
        }

        private Vector3? FindValidWalkTarget()
        {
            for (int attempt = 0; attempt < maxWalkAttempts; attempt++)
            {
                // Pick a random point within walkRadius of original position
                Vector2 randomOffset = Random.insideUnitCircle * walkRadius;
                Vector3 targetPosition = originalPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

                // Clamp to scene bounds if available
                if (SceneBounds.Instance != null)
                {
                    var bounds = SceneBounds.Instance.Bounds;
                    targetPosition.x = Mathf.Clamp(targetPosition.x, bounds.xMin, bounds.xMax);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, bounds.yMin, bounds.yMax);
                }

                // Check if this position is clear of other characters
                if (IsPositionClear(targetPosition))
                {
                    return targetPosition;
                }
            }

            // Couldn't find a valid position after max attempts
            return null;
        }

        private bool IsPositionClear(Vector3 position)
        {
            // Find all colliders within minimum distance
            Collider2D[] nearby = Physics2D.OverlapCircleAll(position, minDistanceFromOthers);

            foreach (var collider in nearby)
            {
                // Skip self
                if (collider == myCollider)
                    continue;

                // Check if this is another character (has CharacterAnimator)
                var otherAnimator = collider.GetComponent<CharacterAnimator>();
                if (otherAnimator != null)
                {
                    // Found another character too close
                    return false;
                }
            }

            return true;
        }

        private void UpdateWalking()
        {
            if (!isWalking) return;

            Vector3 direction = (walkTarget - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, walkTarget);

            if (distance < 0.1f)
            {
                // Arrived at destination
                StopWalking();
                return;
            }

            // Move toward target
            transform.position += direction * walkSpeed * Time.deltaTime;

            // Update facing direction as we move
            bool walkingRight = walkTarget.x > transform.position.x;
            visuals?.SetFlipped(!walkingRight);
        }

        private void StopWalking()
        {
            isWalking = false;
            state = CharacterAnimationState.Idle;
            currentFrame = 0;

            // Random flip when idle (same behavior as initial spawn)
            visuals?.SetFlipped(Random.value > 0.5f);

            UpdateVisuals();
        }

        private void UpdateDanceSway()
        {
            // Calculate target sway based on current frame
            // Frame 0 = left side (-swayAmount), Frame max = right side (+swayAmount)
            float frameProgress = (float)currentFrame / (FrameCount - 1);
            targetSwayOffset = Mathf.Lerp(-swayAmount, swayAmount, frameProgress);

            // Smoothly interpolate toward target for gentle movement
            float swaySpeed = swayAmount * 2f / frameTime; // Move full range over one frame time
            currentSwayOffset = Mathf.MoveTowards(currentSwayOffset, targetSwayOffset, swaySpeed * Time.deltaTime);

            // Apply sway offset to position
            Vector3 swayedPosition = danceBasePosition;
            swayedPosition.x += currentSwayOffset;
            transform.position = swayedPosition;
        }

        private void UpdateFrameAnimation(bool loop)
        {
            frameTimer += Time.deltaTime;

            if (frameTimer >= frameTime)
            {
                frameTimer -= frameTime;
                AdvanceFrame(loop);
                UpdateVisuals();
            }
        }

        private void AdvanceFrame(bool loop)
        {
            if (loop)
            {
                // Simple loop: 0→1→2→3→0→1→2→3...
                currentFrame = (currentFrame + 1) % FrameCount;
            }
            else
            {
                // Ping-pong: 0→1→2→3→2→1→0→1→2→3...
                if (pingPongReverse)
                {
                    currentFrame--;
                    if (currentFrame < 0)
                    {
                        currentFrame = 1; // Bounce back
                        pingPongReverse = false;
                    }
                }
                else
                {
                    currentFrame++;
                    if (currentFrame >= FrameCount)
                    {
                        currentFrame = FrameCount - 2; // Bounce back
                        pingPongReverse = true;
                    }
                }
            }
        }

        private void UpdateVisuals()
        {
            visuals?.SetAnimationState(state, currentFrame);
        }

        // Called when the character's original position changes (e.g., after spawn)
        public void SetOriginalPosition(Vector3 position)
        {
            originalPosition = position;
        }
    }
}
