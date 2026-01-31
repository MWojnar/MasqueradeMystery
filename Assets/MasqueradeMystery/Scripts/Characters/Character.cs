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

            if (animator != null)
            {
                animator.SetDancing(data.IsDancing);
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
