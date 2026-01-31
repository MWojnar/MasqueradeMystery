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
            }

            if (animator != null)
            {
                animator.SetDancing(data.IsDancing);
            }
        }

        public void SetDancePartner(Character partner)
        {
            DancePartner = partner;
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
