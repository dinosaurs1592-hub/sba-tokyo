using SBATokyo.Prototype.Core;
using UnityEngine;

namespace SBATokyo.Prototype.Gameplay
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private PrototypeGameManager gameManager;
        [SerializeField] private Transform runner;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private float spawnDistanceAhead = 40f;
        [SerializeField] private float spacing = 12f;
        [SerializeField] private float recycleDistanceBehind = 15f;
        [SerializeField] private int poolSize = 12;

        private GameObject[] pool;
        private float nextSpawnZ;
        private int poolIndex;

        private void Start()
        {
            if (obstaclePrefab == null)
            {
                return;
            }

            pool = new GameObject[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                pool[i] = Instantiate(obstaclePrefab, new Vector3(0f, -100f, 0f), Quaternion.identity);
                pool[i].SetActive(false);
            }

            nextSpawnZ = spawnDistanceAhead;
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.RunReset += HandleRunReset;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.RunReset -= HandleRunReset;
            }
        }

        private void Update()
        {
            if (gameManager == null || runner == null || obstaclePrefab == null || gameManager.IsGameOver)
            {
                return;
            }

            RecyclePassedObstacles();

            while (nextSpawnZ < runner.position.z + spawnDistanceAhead)
            {
                SpawnObstacle(nextSpawnZ);
                nextSpawnZ += spacing;
            }
        }

        private void SpawnObstacle(float spawnZ)
        {
            GameObject instance = pool[poolIndex];
            poolIndex = (poolIndex + 1) % pool.Length;

            int lane = Random.Range(-1, 2);
            instance.transform.position = new Vector3(lane * laneWidth, 0.5f, spawnZ);
            if (instance.GetComponent<PrototypeObstacle>() == null)
            {
                instance.AddComponent<PrototypeObstacle>();
            }
            instance.SetActive(true);
        }

        private void RecyclePassedObstacles()
        {
            if (pool == null)
            {
                return;
            }

            for (int i = 0; i < pool.Length; i++)
            {
                if (!pool[i].activeSelf)
                {
                    continue;
                }

                if (pool[i].transform.position.z < runner.position.z - recycleDistanceBehind)
                {
                    pool[i].SetActive(false);
                }
            }
        }

        private void HandleRunReset()
        {
            if (pool == null)
            {
                nextSpawnZ = spawnDistanceAhead;
                return;
            }

            for (int i = 0; i < pool.Length; i++)
            {
                pool[i].SetActive(false);
                pool[i].transform.position = new Vector3(0f, -100f, 0f);
            }

            nextSpawnZ = spawnDistanceAhead;
        }
    }
}
