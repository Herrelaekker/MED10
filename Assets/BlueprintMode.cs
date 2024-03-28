using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlueprintMode : MonoBehaviour
{
    GridManager gridManager;

    public GameObject blueprintUI;

    Building building;
    BuildMode buildMode;

    BoundsInt buildingArea;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        buildMode = FindObjectOfType<BuildMode>();
    }

    public void SpawnBuilding(GameObject buildingObj)
    {
        var newBuilding = Instantiate(buildingObj);
        building = newBuilding.GetComponent<Building>();
        gridManager.SetTemp(building);
        blueprintUI.SetActive(false);
    }

    public Building GetCurrentBuilding()
    {
        return building;
    }

    public BoundsInt GetCurrentBuildingBounds()
    {
        return buildingArea;
    }

    public void BuildingPlaced()
    {
        buildingArea = gridManager.GetBuildingArea();
        buildMode.SwitchState(BuildState.PickBlock);
        buildMode.StartBuildMode();
    }

    public TileBase[] GetCurrentBuildingTiles()
    {
        return gridManager.GetRecentlyClaimedTiles();
    }
}
