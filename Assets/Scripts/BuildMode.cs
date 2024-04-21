using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum BuildState
{
    None,
    PickBlock,
    Conjure,
    PlaceBlock,
    BlockPlaceTransition
}

public class BuildMode : MonoBehaviour
{
    private BuildState state = BuildState.None;
    public GameObject pickBlockUI;
    public GameObject placeBlockUI;

    private GameObject pickedBlock;
    private GameObject conjuredBlock;
    float conjureTimer = 0;
    public float conjureTime = 5f;

    float placingTimer = 0;
    public float placingTime = 2f;
    Vector3 placingBlockStartPos;
    Vector3 placingBlockEndPos;

    public Transform conjuringTransform;

    public GameObject posOptionBtnPrefab;

    GameObject[,] posOptionBtns;
    BoundsInt buildingArea;

    public Sprite crackedWall;
    public Sprite goldenWall;
    Camera camera;
    CinemachineSwitcher switcher;
    public GameObject defaultBlock;

    public Sprite[] merlons;
    public GameObject merlonPrefab;

    public Animator playerCharAnimator;
    public GameObject magicPE;
    public GameObject magicBurstPE;
    public GameObject magicTrailPE;

    [System.Serializable]
    public class OnBuildStart : UnityEvent{ }
    public OnBuildStart onBuildStart;

    [System.Serializable]
    public class OnBuildEnd : UnityEvent { }
    public OnBuildEnd onBuildEnd;

    [System.Serializable]
    public class OnBlockConjured : UnityEvent { }
    public OnBlockConjured onBlockConjured;

    private void Start()
    {
        camera = Camera.main;
        switcher = camera.GetComponent<CinemachineSwitcher>();
    }

    public void StartBuildMode()
    {
        switcher.SwitchState("Blueprint");
        StartCoroutine(BuildModeStart(1));
    }

    /*IEnumerator WaitOnBlend()
    {
        yield return new WaitUntil(switcher.);
        float waitDuration = switcher.GetBlendDuration();
        StartCoroutine(BuildModeStart(waitDuration));
    }*/

    IEnumerator BuildModeStart(float waitDuration)
    {
        yield return new WaitUntil(() => switcher.DoneBlending());
        onBuildStart.Invoke();
        print("START BUILD MODE");
        buildingArea = blueprintMode.GetCurrentBuildingBounds();
        print(buildingArea);
        //TileBase[] blueprintTiles = blueprintMode.GetCurrentBuildingTiles();

        Vector3 origin = buildingArea.position;
        posOptionBtns = new GameObject[buildingArea.size.x, buildingArea.size.y];

        for (int x = 0; x < buildingArea.size.x; x++)
        {
            for (int y = 0; y < buildingArea.size.y; y++)
            {
                var block = Instantiate(posOptionBtnPrefab, placeBlockUI.transform);
                Button button = block.GetComponent<Button>();

                int newX = x;
                int newY = y;
                button.onClick.AddListener(() => PositionChosen(new Vector2Int(newX, newY)));

                RectTransform rectTransform = block.GetComponent<RectTransform>();
                Vector3 worldPos = origin + new Vector3(x + 0.5f, y + 0.5f, 0);

                Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
                rectTransform.anchoredPosition = new Vector2(1920 * viewportPos.x, 1080 * viewportPos.y);

                //block.transform.anchoredPosition = worldPos;
                posOptionBtns[x, y] = block;
            }
        }
    }

    private bool IsArrayEmpty(GameObject[,] essenceArray, int row, int col)
    {
        if (essenceArray == null || essenceArray.Length == 0) return true;
        for (int i = 0; i < row; i++)
        for (int j = 0; j < col; j++)
        {
            if (essenceArray[i,j] != null)
            {
                return false;
            }
        }
        return true;
    }

    public void SwitchState(BuildState newState)
    {
        state = newState;

        pickBlockUI.SetActive(false);
        placeBlockUI.SetActive(false);

        switch (state)
        {
            case BuildState.Conjure:
                playerCharAnimator.SetBool("PickedBlock", true);
                break;
            case BuildState.BlockPlaceTransition:
                playerCharAnimator.SetBool("PickedPlacement", true);
                break;
            case BuildState.PickBlock:
                playerCharAnimator.SetBool("PickedPlacement", false);

                pickBlockUI.SetActive(true);
                break;
            case BuildState.PlaceBlock:
                playerCharAnimator.SetBool("PickedBlock", false);

                placeBlockUI.SetActive(true);
                int rowWithBtns = 0;
                bool rowChosen = false;

                for (int y = 0; y < buildingArea.size.y; y++)
                {
                    for (int x = 0; x < buildingArea.size.x; x++)
                    {
                        if (!posOptionBtns[x, y])
                        {
                            continue;
                        }

                        if (!rowChosen)
                        {
                            rowWithBtns = y;
                            rowChosen = true;
                        }
                        if (rowChosen && y == rowWithBtns)
                        {
                            posOptionBtns[x, y].SetActive(true);
                        }
                        else
                        {
                            posOptionBtns[x, y].SetActive(false); 
                        }
                    }
                }
                break;
        }
    }

    public void PickedBlock(GameObject block)
    {
        pickedBlock = block;
        magicPE.SetActive(true);
        magicBurstPE.SetActive(false);
        magicTrailPE.SetActive(false);

        SwitchState(BuildState.Conjure);
    }

    public void ChoseBlockPlacement()
    {
        //Place Block somehow
        SwitchState(BuildState.PickBlock);
    }

    void AddMerlons()
    {
        Vector3 origin = buildingArea.position;
        int iterations = buildingArea.size.x;
        for (int x = 0; x < iterations; x++)
        {
            var merlon = Instantiate(merlonPrefab);

            Sprite merlonSprite;
            Vector3 worldPos;
            if (x == 0)
            {
                merlonSprite = merlons[0];
                worldPos = origin + new Vector3(x + 0.435f, buildingArea.size.y + 0.15f, 0);
            }
            else if (x == iterations - 1)
            {
                merlonSprite = merlons[2];
                worldPos = origin + new Vector3(x + 0.565f, buildingArea.size.y + 0.15f, 0);
            }
            else
            {
                merlonSprite = merlons[1];
                worldPos = origin + new Vector3(x + 0.5f, buildingArea.size.y + 0.15f, 0);
            }


            SpriteRenderer merlonSR = merlon.GetComponent<SpriteRenderer>();
            merlonSR.sprite = merlonSprite;
            merlonSR.sortingOrder = 4;
            merlon.transform.position = worldPos;
        }
    }

    void DoneBuilding()
    {
        AddMerlons();

        SwitchState(BuildState.None);
        onBuildEnd.Invoke();
        //blueprintMode.StartBlueprintMode();
        //switcher.SwitchState("Blueprint");
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case BuildState.PickBlock:
                if (Input.GetKey(KeyCode.L))
                {
                    for (int x = 0; x < buildingArea.size.x; x++)
                    {
                        for (int y = 0; y < buildingArea.size.y; y++)
                        {
                            print(x + "," + y);

                            if (!posOptionBtns[x, y])
                                continue;

                            conjuredBlock = Instantiate(defaultBlock, conjuringTransform);
                            conjuredBlock.transform.parent = null;

                            Destroy(posOptionBtns[x,y]);

                            Vector3 origin = buildingArea.position;
                            Vector3 worldPos = origin + new Vector3(x + 0.5f, y + 0.5f, 0);
                            placingBlockEndPos = worldPos;
                            
                            conjuredBlock.transform.position = placingBlockEndPos;
                            onBlockConjured.Invoke();
                        }
                    }

                    DoneBuilding();

                }
                break;
            case BuildState.Conjure:
                /*conjureTimer += Time.fixedDeltaTime;

                if (Input.GetKey(KeyCode.A))
                {
                    ConjureStone(true);
                }
                else if (conjureTimer >= conjureTime)
                {
                    ConjureStone(false);
                }*/
                break;
            case BuildState.BlockPlaceTransition:
                placingTimer += Time.fixedDeltaTime;
                float t = placingTimer / placingTime;
                
                conjuredBlock.transform.position = Vector3.Lerp(placingBlockStartPos, placingBlockEndPos, t);
                if (t >= 1)
                {
                    placingTimer = 0;

                    if (IsArrayEmpty(posOptionBtns, buildingArea.size.x, buildingArea.size.y))
                    {
                        //GOTO Next phase
                        DoneBuilding();
                    }
                    else
                        SwitchState(BuildState.PickBlock);
                }
                break;
        }
    }

    void PositionChosen(Vector2Int gridPos)
    {
        print(gridPos);
        Destroy(posOptionBtns[gridPos.x, gridPos.y]);

        Vector3 origin = buildingArea.position;
        Vector3 worldPos = origin + new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0);
        placingBlockEndPos = worldPos;

        placingBlockStartPos = conjuredBlock.transform.position;
        SwitchState(BuildState.BlockPlaceTransition);
    }

    public void ConjureStone(GameDecisionData decisionData)//bool correctlyConjured)
    {
        conjureTimer = 0;
        conjuredBlock = Instantiate(pickedBlock, conjuringTransform);
        conjuredBlock.transform.parent = null;

        magicPE.SetActive(false);
        magicBurstPE.SetActive(true);
        magicTrailPE.SetActive(true);
        magicTrailPE.transform.parent = conjuredBlock.transform;
        magicTrailPE.transform.localPosition = Vector3.zero;

        SpriteRenderer spriteRend = conjuredBlock.GetComponent<SpriteRenderer>();
        if (decisionData.decision != TrialType.AccInput && decisionData.decision != TrialType.FabInput)
            spriteRend.sprite = crackedWall;
        else if (decisionData.classification == MotorImageryEvent.GoldenMotorImagery)
            spriteRend.sprite = goldenWall;
        SwitchState(BuildState.PlaceBlock);
        onBlockConjured.Invoke();
    }
}
