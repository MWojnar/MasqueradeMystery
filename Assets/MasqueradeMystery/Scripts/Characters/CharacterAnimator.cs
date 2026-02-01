using System;
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

        [Header("Dance Movement")]
        [Tooltip("Chance per second for a dancing character to move to a new spot")]
        [SerializeField] private float danceMoveChance = 0.05f;
        [Tooltip("Movement speed while dancing (slower than walking)")]
        [SerializeField] private float danceMoveSpeed = 0.5f;
        [Tooltip("Maximum distance a dancer will move from their original position")]
        [SerializeField] private float danceMoveRadius = 3f;

        private CharacterVisuals visuals;
        private Character character;
        private Collider2D myCollider;
        private CharacterAnimationState state = CharacterAnimationState.Idle;
        public int CurrentFrame => currentFrame;
        private int currentFrame;
        private float frameTimer;
        public bool PingPongReverse => pingPongReverse;
        private bool pingPongReverse; // Direction for ping-pong animation
        private bool isRightPartner;  // Determines starting frame for dance
        private bool hasPartner;

        // Walking state
        private Vector3 originalPosition;
        private Vector3 walkTarget;
        private bool isWalking;
        private float walkDecisionTimer;

        // Dance sway state
        public Vector3 DanceBasePosition => danceBasePosition;
        private Vector3 danceBasePosition;
        private float currentSwayOffset;
        private float targetSwayOffset;

        // Dance movement state
        private Vector3 danceOriginalPosition;
        private Vector3 danceMoveTarget;
        private bool isDanceMoving;
        private float danceMoveDecisionTimer;
        private Vector3 partnerOffset; // Offset from partner (for follower to maintain)

        // External control (for player)
        private bool externallyControlled;

        // One-shot animation state
        private bool isOneShotAnimation;
        private bool animationComplete;
        private Action onAnimationComplete;
        private int oneShotFrameCount;

        // Freeze state - prevents all behavior updates
        private bool isFrozen;

        private void Awake()
        {
            visuals = GetComponent<CharacterVisuals>();
            character = GetComponent<Character>();
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

                // Check if we should sync with partner (follower syncs to leader)
                bool syncedToPartner = false;
                if (isRightPartner && character != null && character.DancePartner != null)
                {
                    var leaderAnimator = character.DancePartner.GetComponent<CharacterAnimator>();
                    if (leaderAnimator != null)
                    {
                        // Copy leader's animation phase to stay in sync
                        currentFrame = leaderAnimator.CurrentFrame;
                        pingPongReverse = leaderAnimator.PingPongReverse;
                        syncedToPartner = true;
                    }
                }

                if (!syncedToPartner)
                {
                    // Leader (or solo) randomly picks starting direction
                    bool startReversed = UnityEngine.Random.value > 0.5f;
                    if (startReversed)
                    {
                        currentFrame = FrameCount - 1;
                        pingPongReverse = true;
                    }
                    else
                    {
                        currentFrame = 0;
                        pingPongReverse = false;
                    }
                }

                // Store base position for sway and original position for movement
                danceBasePosition = transform.position;
                danceOriginalPosition = transform.position;
                isDanceMoving = false;
                danceMoveDecisionTimer = 0f;

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

        public void SetExternalWalking(bool walking)
        {
            externallyControlled = true;
            if (walking && state != CharacterAnimationState.Walking)
            {
                state = CharacterAnimationState.Walking;
                currentFrame = 0;
                frameTimer = 0f;
            }
            else if (!walking && state == CharacterAnimationState.Walking)
            {
                state = CharacterAnimationState.Idle;
                currentFrame = 0;
            }
            UpdateVisuals();
        }

        private void Update()
        {
            // Skip all behavior updates when frozen
            if (isFrozen) return;

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
                    UpdateDanceMovement();
                    UpdateDanceSway();
                    break;
                case CharacterAnimationState.Accusing:
                case CharacterAnimationState.Accused:
                    UpdateOneShotAnimation();
                    break;
            }
        }

        private void UpdateIdleBehavior()
        {
            // Skip if externally controlled (player)
            if (externallyControlled) return;

            // Only non-partnered characters can walk
            if (hasPartner) return;

            walkDecisionTimer += Time.deltaTime;

            // Check once per second if we should start walking
            if (walkDecisionTimer >= 1f)
            {
                walkDecisionTimer = 0f;

                if (UnityEngine.Random.value < walkChance)
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
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * walkRadius;
                Vector3 targetPosition = originalPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

                // Clamp to character bounds (scene bounds with margins)
                if (SceneBounds.Instance != null)
                {
                    var bounds = SceneBounds.Instance.CharacterBounds;
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

            // Get dance partner's collider to exclude from check
            Collider2D partnerCollider = null;
            if (character != null && character.DancePartner != null)
            {
                partnerCollider = character.DancePartner.GetComponent<Collider2D>();
            }

            foreach (var collider in nearby)
            {
                // Skip self
                if (collider == myCollider)
                    continue;

                // Skip dance partner (they're allowed to be close)
                if (collider == partnerCollider)
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
            visuals?.SetFlipped(UnityEngine.Random.value > 0.5f);

            UpdateVisuals();
        }

        private void UpdateDanceMovement()
        {
            // Right partner (follower) tracks left partner's position
            if (isRightPartner)
            {
                UpdateFollowerMovement();
                return;
            }

            // Left partner (leader) makes movement decisions
            if (isDanceMoving)
            {
                // Move danceBasePosition toward target
                Vector3 direction = (danceMoveTarget - danceBasePosition).normalized;
                float distance = Vector3.Distance(danceBasePosition, danceMoveTarget);

                if (distance < 0.1f)
                {
                    // Arrived at destination
                    danceBasePosition = danceMoveTarget;
                    isDanceMoving = false;
                }
                else
                {
                    // Slowly move base position (sway will be applied on top)
                    danceBasePosition += direction * danceMoveSpeed * Time.deltaTime;
                }
            }
            else
            {
                // Check if we should start moving
                danceMoveDecisionTimer += Time.deltaTime;

                if (danceMoveDecisionTimer >= 1f)
                {
                    danceMoveDecisionTimer = 0f;

                    if (UnityEngine.Random.value < danceMoveChance)
                    {
                        TryStartDanceMove();
                    }
                }
            }
        }

        private void UpdateFollowerMovement()
        {
            // Get the leader (dance partner)
            if (character == null || character.DancePartner == null) return;

            var leaderAnimator = character.DancePartner.GetComponent<CharacterAnimator>();
            if (leaderAnimator == null) return;

            // Calculate where we should be relative to leader
            // partnerOffset is calculated once when we first start following
            if (partnerOffset == Vector3.zero)
            {
                partnerOffset = danceBasePosition - leaderAnimator.DanceBasePosition;
            }

            // Our target is always leader's position + our offset
            Vector3 targetPosition = leaderAnimator.DanceBasePosition + partnerOffset;

            // Move toward target position
            float distance = Vector3.Distance(danceBasePosition, targetPosition);
            if (distance > 0.01f)
            {
                Vector3 direction = (targetPosition - danceBasePosition).normalized;
                danceBasePosition += direction * danceMoveSpeed * Time.deltaTime;

                // Snap if very close
                if (Vector3.Distance(danceBasePosition, targetPosition) < 0.05f)
                {
                    danceBasePosition = targetPosition;
                }
            }
        }

        private void TryStartDanceMove()
        {
            // Only the leader (left partner) initiates movement
            if (isRightPartner) return;

            // Get partner info for checking both positions
            CharacterAnimator partnerAnimator = null;
            Vector3 currentPartnerOffset = Vector3.zero;

            if (character != null && character.DancePartner != null)
            {
                partnerAnimator = character.DancePartner.GetComponent<CharacterAnimator>();
                if (partnerAnimator != null)
                {
                    currentPartnerOffset = partnerAnimator.DanceBasePosition - danceBasePosition;
                }
            }

            // Try to find a valid position for both partners
            for (int attempt = 0; attempt < maxWalkAttempts; attempt++)
            {
                // Pick a random point within danceMoveRadius of original dance position
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * danceMoveRadius;
                Vector3 leaderTarget = danceOriginalPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
                Vector3 followerTarget = leaderTarget + currentPartnerOffset;

                // Clamp to character bounds (scene bounds with margins)
                if (SceneBounds.Instance != null)
                {
                    var bounds = SceneBounds.Instance.CharacterBounds;
                    leaderTarget.x = Mathf.Clamp(leaderTarget.x, bounds.xMin, bounds.xMax);
                    leaderTarget.y = Mathf.Clamp(leaderTarget.y, bounds.yMin, bounds.yMax);
                    followerTarget.x = Mathf.Clamp(followerTarget.x, bounds.xMin, bounds.xMax);
                    followerTarget.y = Mathf.Clamp(followerTarget.y, bounds.yMin, bounds.yMax);
                }

                // Check if both positions are clear of other characters
                if (IsPositionClear(leaderTarget) && IsPositionClear(followerTarget))
                {
                    danceMoveTarget = leaderTarget;
                    isDanceMoving = true;
                    return;
                }
            }

            // Couldn't find a valid position, stay put
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

        /// <summary>
        /// Plays an animation once and freezes on the last frame.
        /// </summary>
        public void PlayOneShotAnimation(CharacterAnimationState animState, Action onComplete = null)
        {
            PlayOneShotAnimation(animState, frameTime, FrameCount, onComplete);
        }

        /// <summary>
        /// Plays an animation once with custom frame time and frame count, freezes on the last frame.
        /// </summary>
        public void PlayOneShotAnimation(CharacterAnimationState animState, float customFrameTime, int frameCount, Action onComplete = null)
        {
            state = animState;
            currentFrame = 0;
            frameTimer = 0f;
            frameTime = customFrameTime;
            oneShotFrameCount = frameCount;
            isOneShotAnimation = true;
            animationComplete = false;
            onAnimationComplete = onComplete;
            UpdateVisuals();
        }

        /// <summary>
        /// Freezes the animation on the current frame.
        /// </summary>
        public void FreezeAnimation()
        {
            isOneShotAnimation = false;
            animationComplete = true;
        }

        /// <summary>
        /// Stops all movement and dancing, sets character to idle and frozen.
        /// Frozen characters will not start any new behaviors.
        /// </summary>
        public void StopAllActivity()
        {
            isFrozen = true;
            isWalking = false;
            isDanceMoving = false;
            hasPartner = false;
            isOneShotAnimation = false;
            animationComplete = false;
            onAnimationComplete = null;
            state = CharacterAnimationState.Idle;
            currentFrame = 0;
            UpdateVisuals();
        }

        /// <summary>
        /// Unfreezes the character, allowing behaviors to resume.
        /// </summary>
        public void Unfreeze()
        {
            isFrozen = false;
        }

        /// <summary>
        /// Returns whether the current one-shot animation has completed.
        /// </summary>
        public bool IsAnimationComplete => animationComplete;

        private void UpdateOneShotAnimation()
        {
            if (!isOneShotAnimation || animationComplete) return;

            frameTimer += Time.deltaTime;

            if (frameTimer >= frameTime)
            {
                frameTimer -= frameTime;
                currentFrame++;

                if (currentFrame >= oneShotFrameCount)
                {
                    // Freeze on last frame
                    currentFrame = oneShotFrameCount - 1;
                    animationComplete = true;
                    isOneShotAnimation = false;
                    onAnimationComplete?.Invoke();
                    onAnimationComplete = null;
                }

                UpdateVisuals();
            }
        }
    }
}
