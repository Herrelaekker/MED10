using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Vector3 goalPoint;
    private Vector3 startPoint;

    public float timeBeforeReachingGoal = 5f;
    private float timer = 0;

    Vector2 bounds;

    public float minScale = 1, maxScale = 2;

    WaveSpawner waveSpawner;

    public float health = 4;

    public void SetWayPoints(Vector3 startPoint, Vector3 goalPoint)
    {
        this.goalPoint = goalPoint;
        this.startPoint = startPoint;
    }

    public void SetBounds(Vector2 bounds)
    {
        this.bounds = bounds;
    }

    public void SetWaveSpawner(WaveSpawner waveSpawner)
    {
        this.waveSpawner = waveSpawner;
    }

    Vector3 GetScale(float yPos)
    {
        float minY = bounds.x; //-21
        float maxY = bounds.y; //-1

        float ratio = (yPos - minY) / (maxY - minY); //(-20 - -21) / (-1 - -21) = 1/20 

        float finalScale = maxScale - (maxScale - minScale) * ratio; //maxScale -  ratio; // 2 - 1/20

        return new Vector3(finalScale, finalScale, finalScale);
    }

    public void HitByArrow()
    {
        health -= 1;
        if (health <= 0)
        {
            waveSpawner.EnemiesKilled(1);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MagicAttack"))
        {
            waveSpawner.EnemiesKilled(1);
            Destroy(gameObject);
        }

        if (collision.CompareTag("ArrowRange"))
        {
            collision.GetComponent<ArrowShooter>().EnemyInRange(gameObject);
        }
    }

    private void FixedUpdate()
    {
       // if (goalPoint)
        {
            timer += Time.fixedDeltaTime;
            float t = timer / timeBeforeReachingGoal;
            transform.position = Vector3.Lerp(startPoint, goalPoint, t);
            
            transform.localScale = GetScale(transform.position.y);
        }
    }
}
