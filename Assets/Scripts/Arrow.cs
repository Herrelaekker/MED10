using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    Rigidbody2D rb;

    Vector3[] points = new Vector3[3];

    public float height = 5f;

    float count = 0.0f;

    Quaternion endRotation;
    Enemy enemy;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
       
    }

    public void SetPoints(Vector3 startPos, Vector3 endPos, Enemy enemy)
    {
        points[0] = startPos;
        points[2] = endPos;

        points[1] = points[0] + (points[2] - points[0]) / 2 + Vector3.up * height;

        if (endPos.x - startPos.x > 0)
        {
            endRotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            endRotation = Quaternion.Euler(0, 0, -180);
        }
        this.enemy = enemy;
    }

    // Update is called once per frame
    void Update()
    {
        if (count < 1.0f)
        {
            count += 1.0f * Time.deltaTime;

            Vector3 m1 = Vector3.Lerp(points[0], points[1], count);
            Vector3 m2 = Vector3.Lerp(points[1], points[2], count);
            transform.position = Vector3.Lerp(m1, m2, count);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, 0), endRotation, count);
        }
        else
        {
            if(enemy)
            enemy.HitByArrow();
            Destroy(gameObject);    
        }
    }
}
