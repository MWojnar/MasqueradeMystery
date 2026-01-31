using UnityEngine;

namespace MasqueradeMystery
{
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float swayAmplitude = 0.15f;
        [SerializeField] private float swaySpeed = 2f;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobSpeed = 1.5f;
        [SerializeField] private float rotationAmplitude = 3f;

        [Header("State")]
        [SerializeField] private bool isDancing;

        private Vector3 basePosition;
        private Quaternion baseRotation;
        private float phaseOffset;

        private void Start()
        {
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;

            // Random phase offset so characters don't all move in sync
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float time = Time.time + phaseOffset;

            if (isDancing)
            {
                // Dancing: side-to-side sway with slight rotation
                float swayX = Mathf.Sin(time * swaySpeed) * swayAmplitude;
                float swayY = Mathf.Sin(time * swaySpeed * 2f) * bobAmplitude * 0.5f;
                float rotation = Mathf.Sin(time * swaySpeed) * rotationAmplitude;

                transform.localPosition = basePosition + new Vector3(swayX, swayY, 0);
                transform.localRotation = baseRotation * Quaternion.Euler(0, 0, rotation);
            }
            else
            {
                // Not dancing: subtle vertical bob
                float bobY = Mathf.Sin(time * bobSpeed) * bobAmplitude;

                transform.localPosition = basePosition + new Vector3(0, bobY, 0);
                transform.localRotation = baseRotation;
            }
        }

        public void SetDancing(bool dancing)
        {
            isDancing = dancing;
        }

        // Reset to base position (useful when disabling animation)
        public void ResetPosition()
        {
            transform.localPosition = basePosition;
            transform.localRotation = baseRotation;
        }

        // Update base position if character moves
        public void SetBasePosition(Vector3 newBase)
        {
            basePosition = newBase;
        }
    }
}
