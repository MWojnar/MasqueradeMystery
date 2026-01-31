using UnityEngine;

namespace MasqueradeMystery
{
    public class CharacterAnimator : MonoBehaviour
    {
        private const int FrameCount = 4;
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

        private CharacterVisuals visuals;
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

        private void Awake()
        {
            visuals = GetComponent<CharacterVisuals>();
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
                // Left partner starts at frame 0, right partner starts at frame 3
                currentFrame = rightPartner ? FrameCount - 1 : 0;
                pingPongReverse = rightPartner; // Right starts going backward (3→2→1→0)
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
            // Pick a random point within walkRadius of original position
            Vector2 randomOffset = Random.insideUnitCircle * walkRadius;
            walkTarget = originalPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

            isWalking = true;
            state = CharacterAnimationState.Walking;
            currentFrame = 0;
            frameTimer = 0f;

            // Face the direction we're walking
            bool walkingRight = walkTarget.x > transform.position.x;
            visuals?.SetFlipped(!walkingRight); // Flip when walking left

            UpdateVisuals();
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
