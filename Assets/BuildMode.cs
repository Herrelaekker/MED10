using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    BlueprintMode blueprintMode;

    public GameObject posOptionBtnPrefab;

    GameObject[,] posOptionBtns;
    BoundsInt buildingArea;

    private void Start()
    {
        blueprintMode = FindObjectOfType<BlueprintMode>();
    }

    public void StartBuildMode()
    {
        buildingArea = blueprintMode.GetCurrentBuildingBounds();
        print(buildingArea);
        //TileBase[] blueprintTiles = blueprintMode.GetCurrentBuildingTiles();

        Vector3 origin = buildingArea.position;
        posOptionBtns = new GameObject[buildingArea.size.x , buildingArea.size.y];

        for (int x = 0; x < buildingArea.size.x; x++)
        {
            for (int y = 0; y < buildingArea.size.y; y++)
            {
                print(x + "," + y);
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
            case BuildState.PickBlock:
                pickBlockUI.SetActive(true);
                break;
            case BuildState.PlaceBlock:
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
        SwitchState(BuildState.Conjure);
    }

    public void ChoseBlockPlacement()
    {
        //Place Block somehow
        SwitchState(BuildState.PickBlock);
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case BuildState.Conjure:
                conjureTimer += Time.fixedDeltaTime;

                if (Input.GetKey(KeyCode.A))
                {
                    ConjureStone(true);
                }
                else if (conjureTimer >= conjureTime)
                {
                    ConjureStone(false);
                }
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
                        state = BuildState.None;
                        blueprintMode.StartBlueprintMode();
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

    void ConjureStone(bool correctlyConjured)
    {
        conjureTimer = 0;
        conjuredBlock = Instantiate(pickedBlock, conjuringTransform);
        SwitchState(BuildState.PlaceBlock);
    }
}
