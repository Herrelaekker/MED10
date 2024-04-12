using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Transform goalTrans;
    private Transform startTrans;

    public float timeBeforeReachingGoal = 5f;
    private float timer = 0;

    public void SetWayPoints(Transform startTrans, Transform goalTrans)
    {
        this.goalTrans = goalTrans;
        this.startTrans = startTrans;
    }

    private void FixedUpdate()
    {
        if (goalTrans)
        {
            timer += Time.fixedDeltaTime;
            float t = timer / timeBeforeReachingGoal;
            transform.position = Vector3.Lerp(startTrans.position, goalTrans.position, t);
        }
    }
}
