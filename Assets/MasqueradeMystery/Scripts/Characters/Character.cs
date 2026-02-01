using UnityEngine;

namespace MasqueradeMystery
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterVisuals visuals;
        [SerializeField] private CharacterAnimator animator;

        public CharacterData Data { get; private set; }
        public Character DancePartner { get; private set; }

        public void Initialize(CharacterData data)
        {
            Data = data;
            transform.position = new Vector3(data.Position.x, data.Position.y, 0);
            gameObject.name = $"Character_{data.CharacterId}";

            if (visuals != null)
            {
                visuals.UpdateVisuals(data);

                // Random flip for non-dancing characters
                if (!data.IsDancing)
                {
                    visuals.SetFlipped(Random.value > 0.5f);
                }
            }

            // Note: SetDancing with rightPartner is called later in SetDancePartner
            // For now, just mark as dancing without partner info
            if (animator != null && !data.IsDancing)
            {
                animator.SetDancing(false);
            }
        }

        public void SetDancePartner(Character partner)
        {
            DancePartner = partner;

            // Flip the character on the right side to face their partner
            if (partner != null && visuals != null)
            {
                bool isOnRight = transform.position.x > partner.transform.position.x;
                visuals.SetFlipped(isOnRight);

                // Configure dancing animation - right partner starts with reversed animation
                if (animator != null)
                {
                    animator.SetDancing(true, rightPartner: isOnRight);
                }
            }
        }

        /// <summary>
        /// Updates the character's data and refreshes visuals.
        /// Used when hints require modifying existing characters.
        /// </summary>
        public void UpdateData(CharacterData newData)
        {
            Data = newData;
            gameObject.name = $"Character_{newData.CharacterId}";

            if (visuals != null)
            {
                visuals.UpdateVisuals(newData);
            }

            // Update animation state
            if (animator != null)
            {
                animator.SetDancing(newData.IsDancing);
            }
        }

        // Debug display in inspector
        private void OnValidate()
        {
            if (Data != null)
            {
                gameObject.name = $"Character_{Data.CharacterId}";
            }
        }
    }
}
