using UnityEngine;

namespace SBATokyo.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private RoadSurface surface;

        private void Reset() => EnsureSurface();
        private void Awake() => EnsureSurface();

        private void EnsureSurface()
        {
            if (surface == null) surface = GetComponent<RoadSurface>();
            if (surface == null) surface = gameObject.AddComponent<RoadSurface>();
        }
    }
}
