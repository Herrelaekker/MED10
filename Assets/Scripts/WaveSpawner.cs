using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    public float timeBeforeSpawning = 4f;
    private float timer = 0;

   // public Lane[] lanes;
    public GameObject enemyPrefab;

    bool isActive;

    public int maxDeathAmount = 15;

    BattleMode battleMode;

    int deathAmount = 0;

    int enemySpawnCounter = 0;

    public Transform spawnTrans;
    public Transform spawnRangeTrans1;
    public Transform spawnRangeTrans2;
    public Transform towerRangeTrans1;
    public Transform towerRangeTrans2;

    float totalSpawnRangeX;
    float totalTowerRangeX;
    float spawnToTowerRatio;

    float maxTowerY;
    float minSpawnY;

    [System.Serializable]
    public class OnEnemiesKilled : UnityEvent<int> { }
    public OnEnemiesKilled onEnemiesKilled;

    private void Awake()
    {
        battleMode = FindObjectOfType<BattleMode>();
        totalSpawnRangeX = Mathf.Abs(spawnRangeTrans1.position.x) + Mathf.Abs(spawnRangeTrans2.position.x);
        totalTowerRangeX = Mathf.Abs(towerRangeTrans1.position.x) + Mathf.Abs(towerRangeTrans2.position.x);
        spawnToTowerRatio = totalTowerRangeX / totalSpawnRangeX;

        maxTowerY = towerRangeTrans1.position.y;
        minSpawnY = spawnRangeTrans1.position.y;
    }

    public Vector2 GetYRange()
    {
        maxTowerY = towerRangeTrans1.position.y;
        minSpawnY = spawnRangeTrans1.position.y;
        return new Vector2(minSpawnY, maxTowerY);
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            if (enemySpawnCounter < maxDeathAmount)
            {
                timer += Time.fixedDeltaTime;
                if (timeBeforeSpawning <= timer)
                {
                    timer = 0;
                    SpawnEnemyInLane();
                    enemySpawnCounter++;
                }
            }
        }
    }

    public void EnemiesKilled(int amount)
    {
        deathAmount += amount;
        onEnemiesKilled.Invoke(amount);
        if (deathAmount >= maxDeathAmount)
        {
            deathAmount = 0;
            battleMode.EndBattle();
        }
    }

    public void StartSpawning()
    {
        isActive = true;
        timer = 0;
        enemySpawnCounter = 0;
    }

    public void StopSpawning()
    {
        isActive = false;
    }

    void SpawnEnemyInLane()
    {
        /*int randIndex = Random.Range(0, lanes.Length);
        lanes[randIndex].SpawnEnemy(enemyPrefab);*/

        var enemyObjInstance = Instantiate(enemyPrefab, spawnTrans);
        float rndXPos = Random.Range(spawnRangeTrans1.position.x, spawnRangeTrans2.position.x);
        Vector3 startPos = new Vector3(rndXPos, enemyObjInstance.transform.position.y, enemyObjInstance.transform.position.z);
        enemyObjInstance.transform.position = startPos;

        Enemy enemy = enemyObjInstance.GetComponent<Enemy>();

        Vector3 endPos = new Vector3(rndXPos * spawnToTowerRatio, towerRangeTrans1.position.y, towerRangeTrans1.position.z);
        enemy.SetWayPoints(startPos, endPos);
        enemy.SetBounds(GetYRange());
        enemy.SetWaveSpawner(this);
    }
}
