using UnityEngine;

namespace SBATokyo.Prototype.Gameplay
{
    public class ChunkController : MonoBehaviour
    {
        [SerializeField] private float chunkLength = 36f;

        public float ChunkLength => chunkLength;

        public void Activate(float startZ)
        {
            transform.position = new Vector3(0f, 0f, startZ);
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            transform.position = new Vector3(0f, -200f, 0f);
        }

        public float EndZ => transform.position.z + chunkLength;
    }
}
