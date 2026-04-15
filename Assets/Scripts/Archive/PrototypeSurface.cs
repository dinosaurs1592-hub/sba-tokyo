using UnityEngine;

namespace SBATokyo.Prototype.Gameplay
{
    public class PrototypeSurface : MonoBehaviour
    {
        [SerializeField] private PrototypeSurfaceType surfaceType = PrototypeSurfaceType.Ground;

        public PrototypeSurfaceType SurfaceType => surfaceType;
    }
}
