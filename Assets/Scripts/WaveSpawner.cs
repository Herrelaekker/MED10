using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    public float timeBeforeSpawning = 4f;
    public float timeBeforeBlowingAway = 1f;
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

    bool blowAway = false;

    List<Enemy> enemies = new List<Enemy>();

    public GameObject backToBuildingBtn;

    int deathTotal = 0;
    int deathTotalMax = 0;

    PhaseManager phaseManager;

    public GameObject blowEffect;
    public float blowEffectDuration = 1f;
    float blowEffectTimer = 0f;
    Vector3 blowEffectStartPos;
    public Transform blowEffectEndPos;


    private void Awake()
    {
        battleMode = FindObjectOfType<BattleMode>();
        phaseManager = FindObjectOfType<PhaseManager>();
        totalSpawnRangeX = Mathf.Abs(spawnRangeTrans1.position.x) + Mathf.Abs(spawnRangeTrans2.position.x);
        totalTowerRangeX = Mathf.Abs(towerRangeTrans1.position.x) + Mathf.Abs(towerRangeTrans2.position.x);
        spawnToTowerRatio = totalTowerRangeX / totalSpawnRangeX;

        maxTowerY = towerRangeTrans1.position.y;
        minSpawnY = spawnRangeTrans1.position.y;

        blowEffectStartPos = blowEffect.transform.position;
    }

    public void SetMaxTotalDeaths(int amount)
    {
        deathTotalMax = amount;
    }

    public void SetEnemyAmount(int amount)
    {
        maxDeathAmount = amount;
    }

    public Vector2 GetYRange()
    {
        maxTowerY = towerRangeTrans1.position.y;
        minSpawnY = spawnRangeTrans1.position.y;
        return new Vector2(minSpawnY, maxTowerY);
    }

    private void FixedUpdate()
    {
        if (!blowAway && isActive)
        {
            //if (enemySpawnCounter < maxDeathAmount)
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
        else if (blowAway) 
        {
            blowEffectTimer += Time.fixedDeltaTime;

            if (blowEffectTimer >= blowEffectDuration)
            {
                blowEffectTimer = 0;
                blowAway = false;
                foreach (Enemy enemy in enemies)
                {
                    if (enemy != null)
                    {
                        Destroy(enemy.gameObject);
                    }
                }
                blowEffect.SetActive(false);

                enemies.Clear();
            }

            float t = blowEffectTimer / blowEffectDuration;

            blowEffect.transform.position = Vector3.Lerp(blowEffectStartPos, blowEffectEndPos.position, t);  
        }
    }

    public int GetEnemiesKilledOneShot()
    {
        int amount = enemiesKilledOneShot;
        enemiesKilledOneShot = 0;

        return amount;
    }

    int enemiesKilledOneShot = 0;
    public void EnemiesKilled(int amount)
    {
        enemiesKilledOneShot += amount;
        if (deathAmount + amount <= maxDeathAmount)
        {
            deathTotal += amount;
        }
        deathAmount += amount;

        if(!blowAway)
        backToBuildingBtn.SetActive(true);
        onEnemiesKilled.Invoke(amount);

        if (deathTotal >= deathTotalMax)
        {
            deathAmount = maxDeathAmount;
            phaseManager.NoMoreDefending();
            StopBattle();

        }else if (deathAmount >= maxDeathAmount)
        {
            deathAmount = maxDeathAmount;
            StopBattle();
        }
    }

    public void StopBattle()
    {
        backToBuildingBtn.SetActive(false);
        deathAmount = 0;
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.BlowAway(timeBeforeBlowingAway);
            }
        }

        blowAway = true;
        blowEffect.SetActive(true);

        StartCoroutine(EndBattleAfterBlow());
        StopSpawning();
    }

    IEnumerator EndBattleAfterBlow()
    {

        yield return new WaitForSeconds(timeBeforeBlowingAway);
        battleMode.EndBattle();
    }

    public void StartSpawning()
    {
        isActive = true;
        timer = 0;
        enemySpawnCounter = 0;
        deathAmount = 0;
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
        enemies.Add(enemy);

        Vector3 endPos = new Vector3(rndXPos * spawnToTowerRatio, towerRangeTrans1.position.y, towerRangeTrans1.position.z);
        enemy.SetWayPoints(startPos, endPos);
        enemy.SetBounds(GetYRange());
        enemy.SetWaveSpawner(this);

        if (enemySpawnCounter % 2 == 0)
        {
            enemy.PlaySpawnSound();
        }
    }
}
