using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicAttack : MonoBehaviour
{
    public GameObject attackCursor;
    WaveSpawner waveSpawner;

    Vector2 bounds;

    public float minScale = 1, maxScale = 2;

    bool followMouse = true;
    Collider2D col;
    SpriteRenderer sr;

    bool enabled = false;

    public GameObject projectilePE;
    public GameObject explosionPE;

    // Start is called before the first frame update
    void Start()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();
        bounds = waveSpawner.GetYRange();
        col = attackCursor.GetComponentInChildren<Collider2D>();
        sr = attackCursor.GetComponentInChildren<SpriteRenderer>();
        sr.sortingOrder = 10;
        attackCursor.SetActive(false);
        explosionPE.transform.parent = attackCursor.transform;
        explosionPE.transform.localPosition = Vector3.zero;
        projectilePE.SetActive(true);
    }

    Vector3 GetScale(float yPos)
    {
        float minY = bounds.x; //-21
        float maxY = bounds.y; //-1

        float ratio = (yPos - minY) / (maxY - minY); //(-20 - -21) / (-1 - -21) = 1/20 

        float finalScale = maxScale - (maxScale - minScale) * ratio  ; //maxScale -  ratio; // 2 - 1/20

        return new Vector3(finalScale, finalScale, finalScale);
    }

    IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(0.5f);
        explosionPE.SetActive(true);
        sr.color = Color.red;
        col.enabled = true;
        StartCoroutine(RemoveAttack());
    }

    IEnumerator RemoveAttack()
    {
        yield return new WaitForSeconds(0.5f);
        col.enabled = false;
        sr.color = Color.grey;
        sr.sortingOrder = 10;
        followMouse = true;
        explosionPE.transform.parent = null;
        StartCoroutine(RemoveParticleEffect()); 
    }

    IEnumerator RemoveParticleEffect()
    {
        yield return new WaitForSeconds(1f);
        explosionPE.transform.parent = attackCursor.transform;
        explosionPE.transform.localPosition = Vector3.zero;
        explosionPE.SetActive(false);

    }

    public void EnableMagicAttack()
    {
        bounds = waveSpawner.GetYRange();
        enabled = true;
    }
    public void DisableMagicAttack()
    {
        enabled = false;
        sr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled) return;

        if (Input.GetMouseButtonDown(0) && followMouse)
        {
            followMouse = false;
            sr.sortingOrder = 0;
            sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            StartCoroutine(PerformAttack());
        }

        if (followMouse)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0;
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePos);

            if (worldMousePosition.y < bounds.y && worldMousePosition.y > bounds.x)
            {
                if (!attackCursor.activeSelf)
                    attackCursor.SetActive(true);

                attackCursor.transform.localScale = GetScale(worldMousePosition.y);

                attackCursor.transform.position = worldMousePosition;
            }
            else
            {
                if (attackCursor.activeSelf)
                    attackCursor.SetActive(false);
            }
        }
    }
}
