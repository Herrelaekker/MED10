using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public Transform startTrans;
    public Transform endTrans;

    List<GameObject> enemies = new List<GameObject>();

    WaveSpawner waveSpawner;
    private void Start()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();
    }

    private void OnMouseDown()
    {
        DestroyAllEnemies();
    }

    void DestroyAllEnemies()
    {
        waveSpawner.EnemiesKilled(enemies.Count);

        foreach(GameObject go in enemies)
        {
            Destroy(go);
        }
        enemies.Clear();
    }

    public void SpawnEnemy(GameObject enemyObj)
    {
        var enemyObjInstance = Instantiate(enemyObj, startTrans);
        enemies.Add(enemyObjInstance);
        Enemy enemy = enemyObjInstance.GetComponent<Enemy>();
        //enemy.SetWayPoints(startTrans, endTrans);
    }


}
