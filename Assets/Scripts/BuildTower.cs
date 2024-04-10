using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*public enum BuildState
{
    Wait,
    Prepare,
    Activation,
    Building
}

public class BuildTower : MonoBehaviour
{
    public GameObject towerParent;

    GameObject[] towerParts;

    public float waitTime = 4;
    public float prepareTime = 2;
    public float activationTime = 6;
    private float timer;
    private BuildState state;
    public Image wandIndicator;

    public Material brokenStoneMat;

    private int amountOfStonesPlaced = 0;

    public Transform stoneSpawnPoint;
    GameObject curMovingStone;
    Vector3 curMovingStoneStartPos;
    public float buildTime = 2f;

    public float stoneFlyingSpeed = 2f;
    public float yMin = -1.5f;
    public float yMax = 1.5f;
    bool placeBlock = false;
    Vector3 placingBlockStartPos;
    float flyingSpeed;

    private void Awake()
    {
        //Get Tower Parts
        towerParts = new GameObject[towerParent.transform.childCount];
        for (int i = 0; i < towerParts.Length; i++)
        {
            towerParts[i] = towerParent.transform.GetChild(i).gameObject;
            towerParts[i].SetActive(false);
        }
    }

    void SwitchState(BuildState state)
    {
        timer = 0;
        this.state = state;
        wandIndicator.color = GetStateColor();
    }

    Color GetStateColor()
    {
        switch (state)
        {
            case BuildState.Wait:
                return Color.red;
            case BuildState.Prepare:
                return Color.yellow;
            case BuildState.Activation:
                return Color.green;
        }
        return Color.white;
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        switch (state)
        {
            case BuildState.Wait:
                if (timer >= waitTime)
                {
                    SwitchState(BuildState.Prepare);
                }
                break;
            case BuildState.Prepare:

                break;
            case BuildState.Activation:
                if (Input.GetKey(KeyCode.A))
                {
                    SwitchState(BuildState.Building);
                    PlaceStone(true);
                }
                else if (timer >= activationTime)
                {
                    SwitchState(BuildState.Building);
                    PlaceStone(false);
                }
                break;
            case BuildState.Building:
                if (amountOfStonesPlaced <= towerParts.Length)
                {
                    
                    float t = timer / buildTime;
                    if (t < 0.8f)
                    {
                        curMovingStone.transform.position = Vector3.Lerp(stoneSpawnPoint.position, curMovingStoneStartPos, t);
                        Vector3 movingStoneLocalPos = curMovingStone.transform.localPosition;
                        curMovingStone.transform.localPosition = new Vector3(movingStoneLocalPos.x, movingStoneLocalPos.y + Mathf.Sin(Time.time * flyingSpeed) *3, movingStoneLocalPos.z);
                        //transform.position = Vector3.Lerp(start, end, Mathf.PingPong(Time.time, 1.0));
                    }
                    else
                    {
                        if (!placeBlock)
                        {
                            placeBlock = true;
                            placingBlockStartPos = curMovingStone.transform.position;
                        }
                        float newT = (t - 0.8f) * 5;
                        curMovingStone.transform.position = Vector3.Lerp(placingBlockStartPos, curMovingStoneStartPos, newT);
                    }
                    if (t >= 1)
                    {
                        placeBlock = false;
                        SwitchState(BuildState.Wait);
                    }
                }
                break;
        }
    }

    void PlaceStone(bool correctlyPlaced)
    {
        if (amountOfStonesPlaced >= towerParts.Length)
            return;

        curMovingStone = Instantiate(towerParts[amountOfStonesPlaced], stoneSpawnPoint);
        if (!correctlyPlaced)
            curMovingStone.GetComponent<MeshRenderer>().material = brokenStoneMat;

        flyingSpeed = Random.Range(stoneFlyingSpeed - 1f, stoneFlyingSpeed + 1f);
        curMovingStone.transform.localPosition = new Vector3(0, 0, 0);
        curMovingStone.SetActive(true);
        curMovingStoneStartPos = towerParts[amountOfStonesPlaced].transform.position;
        amountOfStonesPlaced++;
    }


}
*/