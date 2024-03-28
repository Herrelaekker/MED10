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
    PlaceBlock
}

public class BuildMode : MonoBehaviour
{
    private BuildState state = BuildState.None;
    public GameObject pickBlockUI;
    public GameObject placeBlockUI;

    private GameObject pickedBlock;
    private GameObject conjuredBlock;
    float timer = 0;
    public float conjureTime = 5f;

    public Transform conjuringTransform;
    BlueprintMode blueprintMode;

    public GameObject posOptionBtnPrefab;

    GameObject[,] posOptionBtns;

    private void Start()
    {
        blueprintMode = FindObjectOfType<BlueprintMode>();
    }

    public void StartBuildMode()
    {
        BoundsInt area = blueprintMode.GetCurrentBuildingBounds();
        print(area);
        //TileBase[] blueprintTiles = blueprintMode.GetCurrentBuildingTiles();

        Vector3 origin = area.position;
        posOptionBtns = new GameObject[area.size.x , area.size.y];

        for (int x = 0; x < area.size.x; x++)
        {
            for (int y = 0; y < area.size.y; y++)
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
                timer += Time.fixedDeltaTime;

                if (Input.GetKey(KeyCode.A))
                {
                    ConjureStone(true);
                }
                else if (timer >= conjureTime)
                {
                    ConjureStone(false);
                }
                break;
        }
    }

    void PositionChosen(Vector2Int gridPos)
    {
        print(gridPos);
        posOptionBtns[gridPos.x, gridPos.y].SetActive(false);
        SwitchState(BuildState.PickBlock);
    }

    void ConjureStone(bool correctlyConjured)
    {
        conjuredBlock = Instantiate(pickedBlock, conjuringTransform);
        SwitchState(BuildState.PlaceBlock);
    }
}
