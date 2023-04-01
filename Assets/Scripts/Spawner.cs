using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float areaHeight = 100;
    [SerializeField] private float areaWidth = 100;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject ballTarget;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private int obstacleCount;
    [SerializeField] private float minObstacleDistance;
    [SerializeField] private float maxObstacleDistance;

    void Start()
    {
        float gridSize = Mathf.Sqrt(obstacleCount);
        float cellSize = (areaHeight + areaWidth) / 2 / gridSize;
        float boundExpandValue = 5f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                float randomDistance = Random.Range(minObstacleDistance, maxObstacleDistance);
                Vector3 spawnPosition = new Vector3((x + randomDistance) * cellSize - areaWidth / 2, 0, (z + randomDistance) * cellSize - areaHeight / 2);

                Quaternion randomrotation = Quaternion.Euler(Random.Range(-5, 5), Random.Range(0, 360), Random.Range(-5, 5));
                GameObject obstacle = Instantiate(obstaclePrefab, transform.position + spawnPosition, randomrotation, obstacleParent);

                Bounds ballTargetBounds = ballTarget.GetComponent<BoxCollider>().bounds;
                ballTargetBounds.Expand(boundExpandValue);

                if (ballTargetBounds.Contains(obstacle.transform.position))
                {
                    Destroy(obstacle);
                }
            }
        }
    }
}

