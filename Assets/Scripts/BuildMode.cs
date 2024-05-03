using Cinemachine;
using ExternalPropertyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static BuildMode;

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

    List<GameObject> posOptionBtns = new List<GameObject>();
    List<GameObject> posDecorationBtns = new List<GameObject>();
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

    public Transform blockEndTrans;

    [System.Serializable]
    public class OnBuildStart : UnityEvent{ }
    public OnBuildStart onBuildStart;

    [System.Serializable]
    public class OnBuildEnd : UnityEvent { }
    public OnBuildEnd onBuildEnd;

    [System.Serializable]
    public class OnBlockConjured : UnityEvent { }
    public OnBlockConjured onBlockConjured;    
    
    [System.Serializable]
    public class OnStartConjureBlock : UnityEvent { }
    public OnStartConjureBlock onStartConjureBlock;    

    [System.Serializable]
    public class OnBlockPlaced : UnityEvent { }
    public OnBlockPlaced onBlockPlaced;

    List<GameObject> blue = new List<GameObject> ();
    List<GameObject> green = new List<GameObject> ();
    PhaseManager phaseManager;

    float cellSize = 0;

    public GameObject backToDefendingBtn;

    bool noMoreDefending = false;

    public CinemachineVirtualCamera buildingCam;

    public float minOrthoSize = 6, maxOrthoSize = 12;
    public float minBuildCamYpos = -0.9f, maxBuildCamYpos = 3.9f;

    public int yHeightTotal = 17;

    int curHighestYPos;
    public int lowestYPos = -6;

    bool updatingCamera = false;

    public float timeToChangeCamera = 2;
    float cameraChangeTimer = 0;

    float orthoSizeBefore;
    Vector3 posBefore;

    float orthoSize;
    float yPos;
    GameManager gameManager;
    float timeBeforeInputWindow;
    LoggingManager loggingManager;

    public Transform decorationsTrans;
    GameObject[] greyOverlays;

    public Transform characterUITrans;
    public GameObject character;
    Transform characterStartTrans;
    public Vector3 characterInitialLocalPos;

    int blocksConjuredTotal = 0;

    public float pickBlockMaxTime = 10;
    public float pickBlockMinTime = 4;
    public float pickBlockShowTime = 5f;
    bool pickBlockReachedMinTime;
    float pickBlockTimer;
    public float placeBlockMaxTime = 10;
    public float placeBlockMinTime = 4;
    public float placeBlockShowTime = 5f;
    bool placeBlockReachedMinTime;
    float placeBlockTimer;

    public GameObject spriteBCIHand;


    public GameObject[] blockPrefabs;

    public TMP_Text timerText;

    bool blockPlaced = false;

    private void Start()
    {
        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();

        camera = Camera.main;
        switcher = camera.GetComponent<CinemachineSwitcher>();
        gridManager = FindObjectOfType<GridManager>();
        phaseManager = FindObjectOfType<PhaseManager>();
        gameManager = FindObjectOfType<GameManager>();
        timeBeforeInputWindow = gameManager.GetInterTrialSeconds();

        cellSize = FindFirstObjectByType<Grid>().cellSize.x;
        curHighestYPos = lowestYPos;

        UpdateBuildingCamera(curHighestYPos);

        List<GameObject> greyOverlayList = new List<GameObject>();
        foreach (Transform child in decorationsTrans.transform)
        {
            foreach(Transform grandChild in child)
            if (grandChild.name == "greyoverlay")
            {
                greyOverlayList.Add(grandChild.gameObject);
            }
        }
        greyOverlays = greyOverlayList.ToArray();
        //characterInitialLocalPos = character.transform.localPosition;
        //characterStartTrans = character.transform.parent;
        spriteBCIHand.SetActive(false);
    }

    void GreyOutDecorations(bool greyOut)
    {
        foreach (GameObject go in greyOverlays) { go.SetActive(greyOut); }

    }
    
    public void NoMoreDefending()
    {
        noMoreDefending = true;
    }

    public void StartBuildMode()
    {
        character.SetActive(true);

        //character.transform.parent = characterUITrans;
        //character.transform.localPosition = Vector3.zero;
        SwitchState(BuildState.PickBlock);
    }

    void PickBlockStart()
    {
        bounds = tilemap.cellBounds;

        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        posOptionBtns = new List<GameObject>();//new GameObject[bounds.size.x, bounds.size.y];
        posDecorationBtns = new List<GameObject>();//new GameObject[bounds.size.x, bounds.size.y];

        Vector3Int origin = bounds.position;

        bool noGreen = true;

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
                    print("newX " + newX + ", newY " + newY);

                    RectTransform rectTransform = block.GetComponent<RectTransform>();
                    Vector3 worldPos = origin +new Vector3(x* cellSize + offsetPos.x, y*cellSize + offsetPos.y, 0);

                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
                    rectTransform.anchoredPosition = new Vector2(1920 * viewportPos.x, 1080 * viewportPos.y);

                    float blockScale = minOrthoSize / orthoSize;
                    block.transform.localScale = new Vector3(blockScale, blockScale, blockScale);

                    //block.transform.anchoredPosition = worldPos;
                    //posOptionBtns[x, y] = block;
                    posOptionBtns.Add(block);
                    blue.Add(block);

                }else if (tile.name == "green")
                {
                    noGreen = false;
                    var block = Instantiate(posOptionBtnPrefab, placeBlockUI.transform);
                    Button button = block.GetComponent<Button>();
                    block.name = "Green";

                    int newX = x + origin.x;
                    int newY = y + origin.y;
                    button.onClick.AddListener(() => PositionChosen(new Vector2Int(newX, newY), PlacementType.Decoration));
                    print("newX " + newX + ", newY " + newY);

                    RectTransform rectTransform = block.GetComponent<RectTransform>();
                    Vector3 worldPos = origin + new Vector3(x  *cellSize + offsetPos.x, y * cellSize + offsetPos.y, 0);

                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
                    rectTransform.anchoredPosition = new Vector2(1920 * viewportPos.x, 1080 * viewportPos.y);

                    float blockScale = minOrthoSize / orthoSize;
                    block.transform.localScale = new Vector3(blockScale, blockScale, blockScale);

                    //block.transform.anchoredPosition = worldPos;
                    //posDecorationBtns[x, y] = block;
                    posDecorationBtns.Add(block);   
                    green.Add(block);   
                }
            }
        }
        GreyOutDecorations(noGreen);
    }
    private void LogEvent(string eventLabel)
    {
        Dictionary<string, object> gameLog = new Dictionary<string, object>() {
            {"Event", eventLabel},
            {"BlockType" , curPlacementType}
        };

        loggingManager.Log("Game", gameLog);
    }

    public void SwitchState(BuildState newState)
    {
        state = newState;

        pickBlockUI.SetActive(false);
        placeBlockUI.SetActive(false);
        backToDefendingBtn.SetActive(false);
        timerText.gameObject.SetActive(false);
        spriteBCIHand.SetActive(false);

        loggingManager.SetBuildState(newState);
        LogEvent("SwitchBuildState");

        switch (state)
        {
            case BuildState.Conjure:
                spriteBCIHand.SetActive(true);
                playerCharAnimator.SetBool("PickedBlock", true);
                break;
            case BuildState.BlockPlaceTransition:
                playerCharAnimator.SetBool("PickedPlacement", true);
                break;
            case BuildState.PickBlock:
                pickBlockTimer = pickBlockMaxTime - placingTime;

                PickBlockStart();
                playerCharAnimator.SetBool("PickedPlacement", false);
               

                pickBlockUI.SetActive(true);
                break;
            case BuildState.PlaceBlock:
                
                placeBlockTimer = placeBlockMaxTime;
                playerCharAnimator.SetBool("PickedBlock", false);
                Sprite sprite = conjuredBlock.GetComponent<SpriteRenderer>().sprite;
                switch (curPlacementType)
                {
                    case PlacementType.Block:
                        ShowPositions(posOptionBtns, sprite);
                        break;
                    case PlacementType.Decoration:
                        ShowPositions(posDecorationBtns, sprite);
                        break;
                }

                placeBlockUI.SetActive(true);
                break;
        }
    }

    public void ShowBlockPositions(Sprite sprite)
    {
        ShowPositions(posOptionBtns, sprite);
        print("show blocks");
    }
    public void ShowDecorationPositions(Sprite sprite)
    {
        ShowPositions(posDecorationBtns, sprite);

        print("show decorations");
    }

    public void HidePositons()
    {
        placeBlockUI.SetActive(false);

        foreach (var pos in posOptionBtns)
            pos.gameObject.SetActive(false);        
        
        foreach (var pos in posDecorationBtns)
            pos.gameObject.SetActive(false);

        /*for (int y = 0; y < bounds.size.y; y++)
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
        }*/
    }

    void ShowPositions(List<GameObject> posOptions, Sprite sprite)
    {
        HidePositons();
        placeBlockUI.SetActive(true);

        foreach(var pos in posOptions)
        {
            pos.gameObject.SetActive(true); 
            pos.GetComponent<Image>().sprite = sprite;
        }

        /*for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                if (posOptions[x, y] != null)
                {
                    posOptions[x, y].SetActive(true);
                    posOptions[x, y].GetComponent<Image>().sprite = sprite;
                }
            }
        }*/
    }

    public void PickedBlock(GameObject block)
    {
        curPlacementType = PlacementType.Block;

        StartCoroutine(PrepareConjuring(block));
    }

    IEnumerator PrepareConjuring(GameObject block)
    {
        magicBurstPE.SetActive(false);
        magicTrailPE.SetActive(false);
        pickBlockUI.SetActive(false);
        HidePositons();
        
        yield return new WaitUntil(() => pickBlockReachedMinTime);

        gameManager.ResumeTrial();

        pickBlockReachedMinTime = false;
        pickedBlock = block;
        SwitchState(BuildState.Conjure);
        StartCoroutine(ConjuringStarted(timeBeforeInputWindow));
    }

    IEnumerator ConjuringStarted(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        magicPE.SetActive(true);
        onStartConjureBlock.Invoke();
    }

    public void PickedDecoration(GameObject block)
    {
        curPlacementType = PlacementType.Decoration;
        StartCoroutine(PrepareConjuring(block));
    }

    public void ChoseBlockPlacement()
    {
        //Place Block somehow
        SwitchState(BuildState.PickBlock);
    }

    void UpdateBuildingCamera(int blocksYPos)
    {
        if (blocksYPos > curHighestYPos)
            curHighestYPos = blocksYPos;

        float yBlockAmount = curHighestYPos - lowestYPos;
        print("Y Block Amount: " + yBlockAmount + " = " + curHighestYPos +" - " + lowestYPos);

        float t = yBlockAmount / yHeightTotal;
        if (t > 1) { t = 1; }

        orthoSizeBefore = buildingCam.m_Lens.OrthographicSize;
        posBefore = buildingCam.transform.position;

        orthoSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, t);
        yPos = Mathf.Lerp(minBuildCamYpos, maxBuildCamYpos, t);

        updatingCamera = true;

    }

    string GetTimerText(float timer) {
        return Mathf.CeilToInt(timer).ToString();
    }

    void ChooseRandomPlacement()
    {
        if (curPlacementType == PlacementType.Block)
        {
            int rndIndex = Random.Range(0, posOptionBtns.Count);
            posOptionBtns[rndIndex].GetComponent<Button>().onClick.Invoke();
        }
        else if (curPlacementType == PlacementType.Decoration)
        {
            int rndIndex = Random.Range(0, posDecorationBtns.Count);
            posDecorationBtns[rndIndex].GetComponent<Button>().onClick.Invoke();
        }
        
    }

    private void FixedUpdate()
    {
        if (updatingCamera)
        {
            cameraChangeTimer += Time.fixedDeltaTime;

            float t = cameraChangeTimer / timeToChangeCamera;
            Vector3 cameraPos = buildingCam.transform.position;

            buildingCam.transform.position = Vector3.Lerp(posBefore, new Vector3(cameraPos.x, yPos, cameraPos.z), t);
            buildingCam.m_Lens.OrthographicSize = Mathf.Lerp(orthoSizeBefore, orthoSize, t);

            if (cameraChangeTimer > timeToChangeCamera)
            {
                updatingCamera = false;
                cameraChangeTimer = 0;
            }
        }

        if (state == BuildState.PlaceBlock || state == BuildState.BlockPlaceTransition)
        {
            placeBlockTimer -= Time.fixedDeltaTime;
            timerText.text = GetTimerText(placeBlockTimer);
            if (!placeBlockReachedMinTime && placeBlockMaxTime - placeBlockTimer > placeBlockMinTime)
            {
                placeBlockReachedMinTime = true;
            }

            if (placeBlockMaxTime - placeBlockTimer > placeBlockShowTime)
            {
                timerText.gameObject.SetActive(true);
            }

            if (placeBlockTimer <= 0)
            {
                ChooseRandomPlacement();
            }
        }

        switch (state)
        {
            case BuildState.PickBlock:
                pickBlockTimer -= Time.fixedDeltaTime;
                timerText.text = GetTimerText(pickBlockTimer);
                if (!pickBlockReachedMinTime && pickBlockMaxTime - pickBlockTimer > pickBlockMinTime)
                {
                    pickBlockReachedMinTime = true;
                }

                if (pickBlockMaxTime - pickBlockTimer > pickBlockShowTime)
                {
                    timerText.gameObject.SetActive(true);
                }

                if (pickBlockTimer <= 0)
                {
                    int rndIndex = Random.Range(0, blockPrefabs.Length);
                    PickedBlock(blockPrefabs[rndIndex]);
                }
                break;           
            case BuildState.PlaceBlock:
                break;
            case BuildState.BlockPlaceTransition:
                if (blockPlaced) return;
                placingTimer += Time.fixedDeltaTime;
                float t = placingTimer / placingTime;
                
                conjuredBlock.transform.position = Vector3.Lerp(placingBlockStartPos, placingBlockEndPos, t);
                if (t >= 1)
                {
                    StartCoroutine(WaitAfterPlacing());
                    blockPlaced = true;
                }
                break;
        }
    }


    public void HideUIHand(InputWindowState inputWindow)
    {
        if (inputWindow == InputWindowState.Open)
        {
            spriteBCIHand.SetActive(false);
        }
    }

    public void DoneBuilding()
    {
        character.SetActive(false);

        //character.transform.parent = characterStartTrans;
        //character.transform.localPosition = characterInitialLocalPos;
        SwitchState(BuildState.None);
        onBuildEnd.Invoke();
    }

    IEnumerator WaitAfterPlacing()
    {
        yield return new WaitUntil(() => placeBlockReachedMinTime);
        placeBlockReachedMinTime = false;
        blockPlaced = false;

        foreach (GameObject btn in posOptionBtns)
            if (btn != null)
                Destroy(btn);
        foreach (GameObject btn in posDecorationBtns)
            if (btn != null)
                Destroy(btn);

        posOptionBtns.Clear();
        posDecorationBtns.Clear();

        placingTimer = 0;
        conjuredBlock.transform.parent = blockEndTrans;
        onBlockPlaced.Invoke();

        if (phaseManager.BeenThroughEnoughExercises(blocksConjuredTotal))
        {
            character.SetActive(false);
            magicTrailPE.SetActive(false);
            SwitchState(BuildState.None);
            phaseManager.DoneWithVersion();
        }
        else if (phaseManager.HaveEnoughMana(1))
        {
            SwitchState(BuildState.PickBlock);
            if (!noMoreDefending)
                backToDefendingBtn.SetActive(true);
        }
        else
            DoneBuilding();
    }

    void StartPlacing(Vector2Int gridPos, PlacementType type)
    {
        placeBlockUI.SetActive(false);

        //yield return new WaitUntil(() => placeBlockReachedMinTime);
        print(gridPos);
        //Destroy(posOptionBtns[gridPos.x, gridPos.y]);

        UpdateBuildingCamera(gridPos.y);

        if (type == PlacementType.Block)
            gridManager.TakeBlockArea(new BoundsInt(new Vector3Int(gridPos.x, gridPos.y, 0), Vector3Int.one));
        else
            gridManager.TakeDecorationArea(new BoundsInt(new Vector3Int(gridPos.x, gridPos.y, 0), Vector3Int.one));

        Vector3 origin = buildingArea.position;
        Vector3 worldPos = origin + new Vector3(gridPos.x * cellSize + offsetBlock.x, gridPos.y * cellSize + offsetBlock.y, 0);
        placingBlockEndPos = worldPos;

        placingBlockStartPos = conjuredBlock.transform.position;
        SwitchState(BuildState.BlockPlaceTransition);
    }

    void PositionChosen(Vector2Int gridPos, PlacementType type)
    {
        StartPlacing(gridPos, type);
    }

    public void ConjureStone(GameDecisionData decisionData)//bool correctlyConjured)
    {
        conjureTimer = 0;
        blocksConjuredTotal++;
        conjuredBlock = Instantiate(pickedBlock, conjuringTransform);
        conjuredBlock.transform.parent = null;

        magicPE.SetActive(false);
        magicBurstPE.SetActive(true);
        magicTrailPE.SetActive(true);
        magicTrailPE.transform.parent = conjuredBlock.transform;
        magicTrailPE.transform.localPosition = Vector3.zero;

        SpriteRenderer spriteRend = conjuredBlock.GetComponent<SpriteRenderer>();
        spriteRend.sortingLayerName = "Wall";
        
        //LogBlockEvent("ConjureBlock", curPlacementType);

        SwitchState(BuildState.PlaceBlock);
        onBlockConjured.Invoke();

        Block block = conjuredBlock.GetComponent<Block>();
        block.ChangeSprite(decisionData.classification);
        //block.transform.GetChild(0).localPosition = Vector3.zero;
    }
}
