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

    bool coolingDown = false;
    Vector3[] points = new Vector3[3];

    public float height = 5f;
    float count = 0;

    bool shootProjectile = false;
    public Transform projectileStartTrans;

    public AudioSource spellHitSound;
    public AudioSource spellSpawnSound;
    public AudioSource spellReadySound;

    LoggingManager loggingManager;

    // Start is called before the first frame update
    void Start()
    {
        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();

        waveSpawner = FindObjectOfType<WaveSpawner>();
        bounds = waveSpawner.GetYRange();
        col = attackCursor.GetComponentInChildren<Collider2D>();
        sr = attackCursor.GetComponentInChildren<SpriteRenderer>();
        sr.sortingOrder = 10;
        attackCursor.SetActive(false);
        explosionPE.transform.parent = attackCursor.transform;
        explosionPE.transform.localPosition = Vector3.zero;
    }

    private void LogEvent(string eventLabel)
    {
        Dictionary<string, object> gameLog = new Dictionary<string, object>() {
            {"Event", eventLabel}
        };

        loggingManager.Log("Game", gameLog);
    }
    private void LogEnemiesKilledEvent(string eventLabel, float enemies)
    {
        Dictionary<string, object> gameLog = new Dictionary<string, object>() {
            {"Event", eventLabel},
            {"EnemiesKilled", enemies}
        };

        loggingManager.Log("Game", gameLog);
    }

    Vector3 GetScale(float yPos)
    {
        float minY = bounds.x; //-21
        float maxY = bounds.y; //-1

        float ratio = (yPos - minY) / (maxY - minY); //(-20 - -21) / (-1 - -21) = 1/20 

        float finalScale = maxScale - (maxScale - minScale) * ratio  ; //maxScale -  ratio; // 2 - 1/20

        return new Vector3(finalScale, finalScale, finalScale);
    }

    void PerformAttack()
    {
        spellReadySound.Stop();
        spellHitSound.Play();
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
        LogEnemiesKilledEvent("ProjectileDone", waveSpawner.GetEnemiesKilledOneShot());
            }

    IEnumerator RemoveParticleEffect()
    {
        spellSpawnSound.Play();
        yield return new WaitForSeconds(1f);
        explosionPE.transform.parent = attackCursor.transform;
        explosionPE.transform.localPosition = Vector3.zero;
        explosionPE.SetActive(false);
        sr.color = Color.white;
        coolingDown = false;
        if (enabled)
        {
            projectilePE.SetActive(true);
            spellReadySound.Play();
            LogEvent("ReadyToShoot");
        }
    }

    public void EnableMagicAttack()
    {
        bounds = waveSpawner.GetYRange();
        enabled = true;
        sr.enabled = true;

        explosionPE.SetActive(false);

        StartCoroutine(RemoveParticleEffect());

    }
    public void DisableMagicAttack()
    {
        enabled = false;
        sr.enabled = false;
        projectilePE.SetActive(false);
    }

    void SetPoints(Vector3 startPos, Vector3 endPos)
    {
        points[0] = startPos;
        points[2] = endPos;

        points[1] = points[0] + (points[2] - points[0]) / 2 + Vector3.up * height;
    }

    void ShootProjectile() {
        LogEvent("ShotProjectile");

        shootProjectile = true;
        projectilePE.SetActive(true);
        SetPoints(projectileStartTrans.position, attackCursor.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled) return;

        if (shootProjectile)
        {
            if (count < 1.05f)
            {
                count += 2 * Time.deltaTime;

                Vector3 m1 = Vector3.Lerp(points[0], points[1], count);
                Vector3 m2 = Vector3.Lerp(points[1], points[2], count);
                projectilePE.transform.position = Vector3.Lerp(m1, m2, count);
            }
            else
            {
                shootProjectile = false;
                count = 0;
                projectilePE.SetActive(false);
                projectilePE.transform.position = projectileStartTrans.position;

                PerformAttack();
            }
        }

        if (Input.GetMouseButtonDown(0) && !coolingDown)
        {
            followMouse = false;
            coolingDown = true;
            sr.sortingOrder = 0;
            sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            ShootProjectile();
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
