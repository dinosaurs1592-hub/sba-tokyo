using UnityEngine;

namespace SBATokyo.Gameplay
{
    public class RoadSurface : MonoBehaviour
    {
        [SerializeField] private SurfaceType surfaceType = SurfaceType.Ground;

        public SurfaceType SurfaceType => surfaceType;
    }
}
