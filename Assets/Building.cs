using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public bool Placed { get; private set; }
    public BoundsInt area;
    private GridManager gridmanager;

    public bool CanBePlaced()
    {
        Vector3Int positionInt = gridmanager.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if (gridmanager.CanTakeArea(areaTemp))
        {
            return true;
        }
        return false;
    }

    public void Place()
    {
        Vector3Int positionInt = gridmanager.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        Placed = true;
        gridmanager.TakeArea(areaTemp);
    }

    // Start is called before the first frame update
    void Awake()
    {
        gridmanager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
