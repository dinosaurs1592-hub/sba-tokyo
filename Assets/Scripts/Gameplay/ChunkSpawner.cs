using System.Collections.Generic;
using SBATokyo.Prototype.Core;
using UnityEngine;

namespace SBATokyo.Prototype.Gameplay
{
    public class ChunkSpawner : MonoBehaviour
    {
        [SerializeField] private PrototypeGameManager gameManager;
        [SerializeField] private Transform runner;
        [SerializeField] private GameObject[] chunkPrefabs;
        [SerializeField] private int poolSizePerChunk = 3;
        [SerializeField] private float spawnDistanceAhead = 80f;
        [SerializeField] private float recycleDistanceBehind = 20f;

        private readonly List<ChunkController> activeChunks = new();
        private readonly List<ChunkController> pool = new();
        private float nextSpawnZ;

        private void Start()
        {
            if (chunkPrefabs == null || chunkPrefabs.Length == 0)
            {
                return;
            }

            foreach (var prefab in chunkPrefabs)
            {
                for (int i = 0; i < poolSizePerChunk; i++)
                {
                    var instance = Instantiate(prefab, new Vector3(0f, -200f, 0f), Quaternion.identity);
                    var controller = instance.GetComponent<ChunkController>();
                    if (controller == null)
                    {
                        controller = instance.AddComponent<ChunkController>();
                    }

                    instance.SetActive(false);
                    pool.Add(controller);
                }
            }

            nextSpawnZ = 0f;
            SpawnUntilAhead();
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
            if (gameManager == null || runner == null || gameManager.IsGameOver)
            {
                return;
            }

            RecyclePassedChunks();
            SpawnUntilAhead();
        }

        private void SpawnUntilAhead()
        {
            if (runner == null)
            {
                return;
            }

            while (nextSpawnZ < runner.position.z + spawnDistanceAhead)
            {
                var chunk = GetChunkFromPool();
                if (chunk == null)
                {
                    break;
                }

                chunk.Activate(nextSpawnZ);
                activeChunks.Add(chunk);
                nextSpawnZ += chunk.ChunkLength;
            }
        }

        private void RecyclePassedChunks()
        {
            for (int i = activeChunks.Count - 1; i >= 0; i--)
            {
                if (activeChunks[i].EndZ < runner.position.z - recycleDistanceBehind)
                {
                    activeChunks[i].Deactivate();
                    pool.Add(activeChunks[i]);
                    activeChunks.RemoveAt(i);
                }
            }
        }

        private ChunkController GetChunkFromPool()
        {
            if (pool.Count == 0)
            {
                return null;
            }

            int index = Random.Range(0, pool.Count);
            var chunk = pool[index];
            pool.RemoveAt(index);
            return chunk;
        }

        private void HandleRunReset()
        {
            foreach (var chunk in activeChunks)
            {
                chunk.Deactivate();
                pool.Add(chunk);
            }

            activeChunks.Clear();
            nextSpawnZ = 0f;
            SpawnUntilAhead();
        }
    }
}
