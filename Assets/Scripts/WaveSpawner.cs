using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public float timeBeforeSpawning = 4f;
    private float timer = 0;

    public Lane[] lanes;
    public GameObject enemyPrefab;

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timeBeforeSpawning <= timer)
        {
            timer = 0;
            SpawnEnemyInLane();
        }
    }

    void SpawnEnemyInLane()
    {
        int randIndex = Random.Range(0, lanes.Length);
        lanes[randIndex].SpawnEnemy(enemyPrefab);
    }
}
