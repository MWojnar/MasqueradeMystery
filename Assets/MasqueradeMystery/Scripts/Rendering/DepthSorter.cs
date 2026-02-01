using UnityEngine;

namespace MasqueradeMystery
{
    public class DepthSorter : MonoBehaviour
    {
        [SerializeField] private float yMultiplier = 100f;
        [SerializeField] private int baseOffset = 1000;

        private SpriteRenderer[] renderers;
        private int[] relativeOrders;

        private void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            relativeOrders = new int[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                relativeOrders[i] = renderers[i].sortingOrder;
            }
        }

        private void LateUpdate()
        {
            int baseOrder = baseOffset + (int)(-transform.position.y * yMultiplier);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingOrder = baseOrder + relativeOrders[i];
            }
        }
    }
}
