using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowShooter : MonoBehaviour
{
    public Transform arrowSpawnTrans;
    public GameObject arrowPrefab;
    List<GameObject> enemiesInRange = new List<GameObject>();

    public float timeBeforeShooting = 2f;
    public float startTime = 1;
    float timer = 0;

    private void Start()
    {
        timer = startTime;
    }

    private void Update()
    {
        if (enemiesInRange.Count > 0)
        {
            timer += Time.deltaTime;

            if (timer >= timeBeforeShooting)
            {
                timer = 0;
                float rndNum = Random.Range(-.25f, .25f);
                timer += rndNum;
                enemiesInRange.RemoveAll(x => !x); // Removes all that are null

                foreach (GameObject enemy in enemiesInRange)
                {
                    if (enemy)
                    {
                        Shoot(enemy);
                        return;
                    }
                }
            }
        }
    }

    void Shoot(GameObject enemy)
    {
        var arrowObj = Instantiate(arrowPrefab, arrowSpawnTrans);
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        arrow.SetPoints(arrowSpawnTrans.position, enemy.transform.position, enemy.GetComponent<Enemy>());
    }

    public void RemoveEnemyFromList(GameObject enemy)
    {
        enemiesInRange.Remove(enemy);
    }

    public void EnemyInRange(GameObject enemy)
    {
        enemiesInRange.Add(enemy);
        //Destroy(collision.gameObject);
    }
}
