using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public Transform startTrans;
    public Transform endTrans;

    List<GameObject> enemies = new List<GameObject>();

    private void OnMouseDown()
    {
        DestroyAllEnemies();
    }

    void DestroyAllEnemies()
    {
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
        enemy.SetWayPoints(startTrans, endTrans);
    }


}
