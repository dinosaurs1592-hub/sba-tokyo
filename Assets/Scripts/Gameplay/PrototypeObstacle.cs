using UnityEngine;

namespace SBATokyo.Prototype.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class PrototypeObstacle : MonoBehaviour
    {
        [SerializeField] private PrototypeSurface surface;

        private void Reset()
        {
            EnsureSurface();
        }

        private void Awake()
        {
            EnsureSurface();
        }

        private void EnsureSurface()
        {
            if (surface == null)
            {
                surface = GetComponent<PrototypeSurface>();
            }

            if (surface == null)
            {
                surface = gameObject.AddComponent<PrototypeSurface>();
            }
        }
    }
}
