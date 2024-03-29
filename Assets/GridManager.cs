using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Empty,
    White,
    Green,
    Red,
    Blue
}

//https://www.youtube.com/watch?v=gFpmJtO0NT4&t=536s

public class GridManager : MonoBehaviour
{
    public GridLayout gridLayout;
    public Tilemap tempTilemap;
    public Tilemap mainTilemap;
    Vector3Int prevPos;
    private BoundsInt prevArea;

    public Building temp;

    private Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    BlueprintMode blueprintMode;

    BoundsInt recentlyClaimedArea;

    // Start is called before the first frame update
    void Start()
    {
        string tilePath = @"Tiles/";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "white"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "red"));
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "green"));
        tileBases.Add(TileType.Blue, Resources.Load<TileBase>(tilePath + "blue"));
        blueprintMode = FindObjectOfType<BlueprintMode>();
    }

    private void ClearArea()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, TileType.Empty);
        tempTilemap.SetTilesBlock(prevArea, toClear);
    }

    void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i <arr.Length; i++)
        {   
            arr[i] = tileBases[type];
        }
    }

    void FollowBuilding()
    {
        ClearArea();
        temp.area.position = gridLayout.WorldToCell(temp.gameObject.transform.position);
        BoundsInt buildingArea = temp.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, mainTilemap);
        TileBase[] bottomArray = GetBottomTiles(buildingArea, mainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.White] || baseArray[i] == tileBases[TileType.Blue])
            {
                tileArray[i] = tileBases[TileType.Green];
            }
            else
            {
                FillTiles(tileArray, TileType.Red);
                break;
            }
        }
        for (int i= 0; i < bottomArray.Length; i++)
        {
            if (bottomArray[i] != tileBases[TileType.Blue])
            {
                FillTiles(tileArray, TileType.Red);
                break;
            }
        }

        tempTilemap.SetTilesBlock(buildingArea, tileArray);
        prevArea = buildingArea;
    }

    TileBase[] GetTilesAbove(BoundsInt area, Tilemap tilemap)
    {
        int counter = 0;
        int highestY = 100000;

        TileBase[] aboveTop = new TileBase[area.size.x];

        foreach (var v in area.allPositionsWithin)
        {
            if (v.y > highestY)
            {
                highestY = v.y;
            }
        }

        foreach (var v in area.allPositionsWithin)
        {
            if (v.y == highestY)
            {
                Vector3Int pos = new Vector3Int(v.x+1, v.y+1, 0);
                print(pos);
                aboveTop[counter] = tilemap.GetTile(pos);
                counter++;
            }
        }

        return aboveTop;
    }

    TileBase[] GetBottomTiles(BoundsInt area, Tilemap tilemap)
    {
        int counter = 0;
        int lowestY = 100000;

        TileBase[] bottom = new TileBase[area.size.x];

        foreach (var v in area.allPositionsWithin)
        {
            if (v.y < lowestY)
            {
                lowestY = v.y;
            }
        }
        
        foreach (var v in area.allPositionsWithin)
        {
            if (v.y == lowestY)
            {
                Vector3Int pos = new Vector3Int(v.x, v.y, 0);
                bottom[counter] = tilemap.GetTile(pos);
                counter++;
            }
        }

        return bottom;
    }

    TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach(var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, mainTilemap);
        TileBase[] bottomArray = GetBottomTiles(area, mainTilemap);
        foreach(var b in baseArray)
        {
            if (b != tileBases[TileType.White] && b != tileBases[TileType.Blue])
            {
                return false;
            }
        }
        foreach(var b in bottomArray)
        {
            if (b != tileBases[TileType.Blue])
                return false;
        }
        return true;
    }

    public TileBase[] GetRecentlyClaimedTiles()
    {
        return GetTilesBlock(recentlyClaimedArea, mainTilemap);
    }
    
    public BoundsInt GetBuildingArea()
    {
        return recentlyClaimedArea;
    }

    public void TakeArea(BoundsInt area)
    {
        recentlyClaimedArea = area;
        BoundsInt areaAboveBuilding = new BoundsInt(new Vector3Int(area.xMin, area.yMax,0), new Vector3Int(area.xMax-area.xMin,1,1));
        SetTilesBlock(area, TileType.Empty, tempTilemap);
        SetTilesBlock(area, TileType.Green, mainTilemap);
        SetTilesBlock(areaAboveBuilding, TileType.Blue, mainTilemap);
    }

    public void SetTemp(Building temp)
    {
        this.temp = temp;
    }

    // Update is called once per frame
    void Update()
    {
        if (!temp)
            return;
        Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

        if (prevPos != cellPos)
        {
            temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos);// + new Vector3(0.5f, 0.5f, 0));
            prevPos = cellPos;
            FollowBuilding();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (temp.CanBePlaced())
            {
                temp.Place();
                temp = null;
                blueprintMode.BuildingPlaced();
            }
        }
    }
}
