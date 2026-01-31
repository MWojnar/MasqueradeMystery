using UnityEngine;

namespace MasqueradeMystery
{
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private bool isDancing;

        // Placeholder for future animation system
        // Currently disabled - characters are static

        public void SetDancing(bool dancing)
        {
            isDancing = dancing;
            // Future: trigger dance animation vs idle animation
        }

        public bool IsDancing => isDancing;
    }
}
