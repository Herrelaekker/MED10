using ExternalPropertyAttributes;
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

public enum PlacementType
{
    Block,
    Decoration
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
    GameObject[,] posDecorationBtns;
    BoundsInt buildingArea;
    BoundsInt bounds;

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

    public Tilemap tilemap;
    GridManager gridManager;
    public Vector2 offsetPos;
    public Vector2 offsetBlock;
    PlacementType curPlacementType;

    [System.Serializable]
    public class OnBuildStart : UnityEvent{ }
    public OnBuildStart onBuildStart;

    [System.Serializable]
    public class OnBuildEnd : UnityEvent { }
    public OnBuildEnd onBuildEnd;

    [System.Serializable]
    public class OnBlockConjured : UnityEvent { }
    public OnBlockConjured onBlockConjured;

    List<GameObject> blue = new List<GameObject> ();
    List<GameObject> green = new List<GameObject> ();

    private void Start()
    {
        camera = Camera.main;
        switcher = camera.GetComponent<CinemachineSwitcher>();
        gridManager = FindObjectOfType<GridManager>();
    }

    public void StartBuildMode()
    {
        SwitchState(BuildState.PickBlock);
    }

    void PickBlockStart()
    {
        bounds = tilemap.cellBounds;

        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        posOptionBtns = new GameObject[bounds.size.x, bounds.size.y];
        posDecorationBtns = new GameObject[bounds.size.x, bounds.size.y];

        Vector3Int origin = bounds.position;

        for (int x = 0;x < bounds.size.x; x++)
        {
            for (int y = 0;y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];

                if (tile == null) continue;
                if (tile.name == "blue")
                {
                    var block = Instantiate(posOptionBtnPrefab, placeBlockUI.transform);
                    Button button = block.GetComponent<Button>();
                    block.name = "Blue";

                    int newX = x + origin.x;
                    int newY = y + origin.y;
                    button.onClick.AddListener(() => PositionChosen(new Vector2Int(newX, newY), PlacementType.Block));

                    RectTransform rectTransform = block.GetComponent<RectTransform>();
                    Vector3 worldPos = origin +new Vector3(x + offsetPos.x, y + offsetPos.y, 0);

                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
                    rectTransform.anchoredPosition = new Vector2(1920 * viewportPos.x, 1080 * viewportPos.y);

                    //block.transform.anchoredPosition = worldPos;
                    posOptionBtns[x, y] = block;
                    blue.Add(block);

                }else if (tile.name == "green")
                {
                    var block = Instantiate(posOptionBtnPrefab, placeBlockUI.transform);
                    Button button = block.GetComponent<Button>();
                    block.name = "Green";

                    int newX = x + origin.x;
                    int newY = y + origin.y;
                    button.onClick.AddListener(() => PositionChosen(new Vector2Int(newX, newY), PlacementType.Decoration));

                    RectTransform rectTransform = block.GetComponent<RectTransform>();
                    Vector3 worldPos = origin + new Vector3(x + offsetPos.x, y + offsetPos.y, 0);

                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
                    rectTransform.anchoredPosition = new Vector2(1920 * viewportPos.x, 1080 * viewportPos.y);

                    //block.transform.anchoredPosition = worldPos;
                    posDecorationBtns[x, y] = block;
                    green.Add(block);   
                }
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
                PickBlockStart();
                playerCharAnimator.SetBool("PickedPlacement", false);

                pickBlockUI.SetActive(true);
                break;
            case BuildState.PlaceBlock:
                playerCharAnimator.SetBool("PickedBlock", false);

                switch (curPlacementType)
                {
                    case PlacementType.Block:
                        ShowPositions(posOptionBtns);
                        break;
                    case PlacementType.Decoration:
                        ShowPositions(posDecorationBtns);
                        break;
                }

                placeBlockUI.SetActive(true);
                break;
        }
    }

    public void ShowBlockPositions()
    {
        ShowPositions(posOptionBtns);
        print("show blocks");
    }
    public void ShowDecorationPositions()
    {
        ShowPositions(posDecorationBtns);

        print("show decorations");
    }

    public void HidePositons()
    {
        placeBlockUI.SetActive(false);

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                if (posDecorationBtns[x, y] != null)
                {
                    posDecorationBtns[x, y].SetActive(false);
                }
                if (posOptionBtns[x, y] != null)
                {
                    posOptionBtns[x, y].SetActive(false);
                }
            }
        }
    }

    void ShowPositions(GameObject[,] posOptions)
    {
        HidePositons();
        placeBlockUI.SetActive(true );
        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                if (posOptions[x, y] != null)
                {
                    posOptions[x, y].SetActive(true);
                }
            }
        }
    }

    public void PickedBlock(GameObject block)
    {
        curPlacementType = PlacementType.Block;
        StartConjuring(block);
    }

    void StartConjuring(GameObject block)
    {
        pickedBlock = block;
        magicPE.SetActive(true);
        magicBurstPE.SetActive(false);
        magicTrailPE.SetActive(false);

        SwitchState(BuildState.Conjure);
    }

    public void PickedDecoration(GameObject block)
    {
        curPlacementType = PlacementType.Decoration;
        StartConjuring(block);

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
            case BuildState.BlockPlaceTransition:
                placingTimer += Time.fixedDeltaTime;
                float t = placingTimer / placingTime;
                
                conjuredBlock.transform.position = Vector3.Lerp(placingBlockStartPos, placingBlockEndPos, t);
                if (t >= 1)
                {
                    placingTimer = 0;

                    /*if (IsArrayEmpty(posOptionBtns, buildingArea.size.x, buildingArea.size.y))

                    {
                        //GOTO Next phase
                        DoneBuilding();
                    }
                    else*/
                        SwitchState(BuildState.PickBlock);
                }
                break;
        }
    }

    void PositionChosen(Vector2Int gridPos, PlacementType type)
    {
        print(gridPos);
        //Destroy(posOptionBtns[gridPos.x, gridPos.y]);

        foreach (GameObject btn in posOptionBtns)
            if (btn != null)
                Destroy(btn);
        foreach (GameObject btn in posDecorationBtns)
            if (btn != null)
                Destroy(btn);

        if (type == PlacementType.Block)
            gridManager.TakeBlockArea(new BoundsInt(new Vector3Int(gridPos.x, gridPos.y, 0), Vector3Int.one));
        else
            gridManager.TakeDecorationArea(new BoundsInt(new Vector3Int(gridPos.x, gridPos.y, 0), Vector3Int.one));

        Vector3 origin = buildingArea.position;
        Vector3 worldPos = origin + new Vector3(gridPos.x + offsetBlock.x, gridPos.y + offsetBlock.y, 0);
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

        if (curPlacementType == PlacementType.Block)
        {
            SpriteRenderer spriteRend = conjuredBlock.GetComponent<SpriteRenderer>();
            if (decisionData.decision != TrialType.AccInput && decisionData.decision != TrialType.FabInput)
                spriteRend.sprite = crackedWall;
            else if (decisionData.classification == MotorImageryEvent.GoldenMotorImagery)
                spriteRend.sprite = goldenWall;
        }

        SwitchState(BuildState.PlaceBlock);
        onBlockConjured.Invoke();
    }
}
