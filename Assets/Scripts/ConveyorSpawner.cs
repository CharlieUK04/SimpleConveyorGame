using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ConveyorSpawner : MonoBehaviour
{
    private Camera mainCamera;
    private Transform spawnPlaneTransform;
    private Plane spawnPlane;
    
    [SerializeField] private Conveyor[] allConveyorPrefabs;
    private int currentlySelected = 0;
    private ConveyorRotationEnum currentRotation = ConveyorRotationEnum.Left;

    private float cellSize;

    private GameObject placementPreview;
    private Vector2 previewScale = Vector2.zero;
    private Vector3 previewLocation = Vector3.zero;
    [SerializeField] private float previewThickness = .1f;
    [SerializeField] private float previewOffsetY = .5f;
    [SerializeField] private Material canPlaceMaterial;
    
    //HashSet to hold all the occupied cells, to prevent double placing
    //HashSets are more efficient than arrays or lists for this use case,
    //since we only need to check if the cell is in use or not.
    readonly HashSet<Vector2Int> occupiedCell = new HashSet<Vector2Int>();

    [Serializable]
    [Tooltip("The box collider on the first prefab element determines the cell size, based on its X size")]
    private class Conveyor
    {
        [SerializeField] private GameObject  conveyorPrefab;
        [SerializeField] private Vector2Int conveyorSizeWxL = new Vector2Int(1, 3);
        public GameObject getConveyorPrefab() {return conveyorPrefab;}
        public Vector2Int getConveyorSize() {return conveyorSizeWxL;}
    }
    
    public enum ConveyorRotationEnum
    {
        Left = 0,
        Up = 90,
        Right = 180,
        Down = 270
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        spawnPlaneTransform = GameObject.FindGameObjectWithTag("ConveyorSpawnPlane").transform;
        spawnPlane = new Plane(spawnPlaneTransform.up, spawnPlaneTransform.position);

        cellSize = allConveyorPrefabs[0].getConveyorPrefab().GetComponent<BoxCollider>().size.x;

        //Creates the preview square for placing conveyors
        placementPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placementPreview.name = "ConveyorPlacementPreview";
        placementPreview.transform.localScale = new Vector3(cellSize, .1f, cellSize);
        placementPreview.GetComponent<Renderer>().material = canPlaceMaterial;
        Destroy(placementPreview.GetComponent<Collider>());
        //placementPreview.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        UpdatePreview();
    }

    //Updates the position and scale of the placement preview based on the currently selected conveyor
    private void UpdatePreview()
    {
        //Switches the x and y if the rotation is up or down
        previewScale = new Vector2(cellSize * GetRequiredCells(currentlySelected).x,
            cellSize * GetRequiredCells(currentlySelected).y);
        previewLocation = GridPosInWorldSpace(WorldToCellPos(MousePosInWorldSpace()));
        previewLocation.y = spawnPlaneTransform.position.y +  previewOffsetY;

        placementPreview.transform.localScale = new Vector3(previewScale.x, previewThickness, previewScale.y);
        placementPreview.transform.position = previewLocation;

    }

    public void SpawnConveyor()
    {
        Vector2Int placementPositionGrid = WorldToCellPos(MousePosInWorldSpace());
        
        //Debug.Log(placementPositionGrid);
        if (!CanPlace(placementPositionGrid, currentlySelected))
        {
            Debug.Log("Cell Occupied");
            return;
        }

        MarkOccupied(placementPositionGrid, currentlySelected);
        Vector3 placementPositionWorld = GridPosInWorldSpace(placementPositionGrid);
        
        Quaternion currentConveyorRotation = Quaternion.Euler(0f, (float)(int)currentRotation, 0f);
        GameObject currentConveyor = Instantiate(allConveyorPrefabs[currentlySelected].getConveyorPrefab(), placementPositionWorld, currentConveyorRotation);
        Debug.Log("Conveyor placed");

    }

    public void ChangeSelection(bool increment)
    {
        currentlySelected += increment ? 1 : -1;
        currentlySelected = Mathf.Clamp(currentlySelected, 0, allConveyorPrefabs.Length - 1); //Subtract 1 due to length starting at 1 and array starting at 0
        //Debug.Log(currentlySelected + " " + allConveyorPrefabs.Length);
    }

    public void ChangeRotation(ConveyorRotationEnum rotationValue)
    {
        currentRotation = rotationValue;
    }
    
    void MarkOccupied(Vector2Int origin, int conveyorID)
    {
        for (int x = 0; x < GetRequiredCells(conveyorID).x; x++)
        {
            for (int y = 0; y < GetRequiredCells(conveyorID).y; y++)
            {
                occupiedCell.Add(origin + new Vector2Int(x, y));
            }
        }
    }

    private Vector3 GridPosInWorldSpace(Vector2Int  gridPos)
    {
        return new Vector3(
            (gridPos.x * cellSize) + (GetRequiredCells(currentlySelected).x * cellSize / 2f),
            spawnPlaneTransform.position.y,
            (gridPos.y * cellSize) + (GetRequiredCells(currentlySelected).y * cellSize / 2f)
        );
    }
    
    Vector3 MousePosInWorldSpace()
    {
        Ray _ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (spawnPlane.Raycast(_ray, out float distance))
        {
            return _ray.GetPoint(distance);
        }
        Debug.Log("Click did not register on the floor plane!");
        
        return Vector2.zero;
    }

    //Snaps a world position to the nearest cell position
    public Vector2Int WorldToCellPos(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt((worldPos.x / cellSize) - (GetRequiredCells(currentlySelected).x / 2f)),
            Mathf.RoundToInt((worldPos.z / cellSize) - (GetRequiredCells(currentlySelected).y / 2f)));
    }

    public Vector2Int GetRequiredCells(int conveyorID)
    {
        if (currentRotation == ConveyorRotationEnum.Left || currentRotation == ConveyorRotationEnum.Right)
        {
            return new Vector2Int(
                allConveyorPrefabs[conveyorID].getConveyorSize().x, 
                allConveyorPrefabs[conveyorID].getConveyorSize().y);
        }
        return new Vector2Int(
            allConveyorPrefabs[conveyorID].getConveyorSize().y, 
            allConveyorPrefabs[conveyorID].getConveyorSize().x);
    }

    //Denotes whether a conveyor can be placed in a certain place, based off the conveyor size from its ID, and if the cells it needs are occupied
    bool CanPlace(Vector2Int origin, int conveyorID)
    {
        //Debug.Log(origin + " " + conveyorID);
        
        //Checks each cell the conveyor needs
        for (int x = 0; x < GetRequiredCells(conveyorID).x; x++)
        {
            for (int y = 0; y < GetRequiredCells(conveyorID).y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (occupiedCell.Contains(cell)) return false;
            }
        }
        return true;
    }
    
}
