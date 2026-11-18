using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class ConveyorSpawner : MonoBehaviour
{
    [SerializeField] private Conveyor[] allConveyorPrefabs;
    [SerializeField] private float previewThickness = .1f;
    [SerializeField] private float previewOffsetY = .5f;
    [SerializeField] private Material canPlaceMaterial;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(18, 25);
    [SerializeField] private int totalFloorCount = 4;
    [SerializeField] private float floorYOffset = 0.75f;
    [SerializeField] private Image conveyorSelectionUI;
    [SerializeField] private Image conveyorSpeedUI;
    [SerializeField] private TextMeshProUGUI selectedFloorTextUI;
    private Vector2Int gridOffset;
    
    private Camera mainCamera;
    private Transform spawnPlaneTransform;
    private Plane spawnPlane;
    private ConveyorManager _conveyorManager;
    
    private int currentlySelectedConveyor = 0;
    private int currentlySelectedSpeed = 0;
    private int currentFloor = 0;
    public float getCurrentFloorY() {return spawnPlaneTransform.position.y + (floorYOffset * currentFloor);}
    private ConveyorRotationEnum currentRotation = ConveyorRotationEnum.Left;

    private float cellSize;

    private GameObject placementPreview;
    private Vector2 previewScale = Vector2.zero;
    private Vector3 previewLocation = Vector3.zero;
    
    //3D Array holding all the conveyor information for each cell and floors
    private PlacedConveyorInfo[,,] grid;
    
    //Returns the bottom left and bottom right of the grid, used for cinemachine camera
    public Vector3 GridMinWorld => new Vector3(-gridOffset.x * cellSize, 0f, -gridOffset.y * cellSize);
    public Vector3 GridMaxWorld => new Vector3((gridSize.x - gridOffset.x) * cellSize, 0f, (gridSize.y - gridOffset.y) * cellSize);
    

    [Serializable]
    [Tooltip("The box collider on the first prefab element determines the cell size, based on its X size")]
    private class Conveyor
    {
        [SerializeField] private GameObject  conveyorPrefab;
        [SerializeField] private Vector2Int conveyorSizeWxL = new Vector2Int(1, 3);
        public GameObject getConveyorPrefab() {return conveyorPrefab;}
        public Vector2Int getConveyorSize() {return conveyorSizeWxL;}
        [SerializeField] private Sprite conveyorIcon;
        public Sprite getConveyorIcon() {return conveyorIcon;}
    }

    private class PlacedConveyorInfo
    {
        private GameObject occupiedObject;
        private Vector2Int[] occupiedCells;
        public GameObject getOccupiedObject() {return occupiedObject;}
        public Vector2Int[] getOccupiedCells() {return occupiedCells;}
        public void setOccupiedObject(GameObject value)
        { if(occupiedObject == null) {occupiedObject = value;} }
        public void setOccupiedCells(Vector2Int[] value)
        { if(occupiedCells == null) {occupiedCells = value; } }
    }
    
    public enum ConveyorRotationEnum
    {
        Left = 0,
        Up = 90,
        Right = 180,
        Down = 270
    }

    private void Awake()
    {
        spawnPlaneTransform = GameObject.FindGameObjectWithTag("ConveyorSpawnPlane").transform;
        grid = new PlacedConveyorInfo[gridSize.x, gridSize.y, totalFloorCount];
        gridOffset =  new Vector2Int(Mathf.CeilToInt(gridSize.x / 2), Mathf.CeilToInt(gridSize.y / 2));
        cellSize = allConveyorPrefabs[0].getConveyorPrefab().GetComponent<BoxCollider>().size.x;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        
        _conveyorManager = GameObject.FindAnyObjectByType<ConveyorManager>();
        ChangeFloor(false);

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
        previewScale = new Vector2(cellSize * allConveyorPrefabs[currentlySelectedConveyor].getConveyorSize().x,
            cellSize * allConveyorPrefabs[currentlySelectedConveyor].getConveyorSize().y);
        previewLocation = GridPosInWorldSpace(GetPlacementOrigin(MousePosInWorldSpace(), GetConveyorDimensions(currentlySelectedConveyor)));
        previewLocation.y = getCurrentFloorY() + previewOffsetY;

        placementPreview.transform.localScale = new Vector3(previewScale.x, previewThickness, previewScale.y);
        placementPreview.transform.position = previewLocation;
        placementPreview.transform.rotation = Quaternion.Euler(new Vector3(0, (float)(int)currentRotation, 0));

    }

    public void SpawnConveyor()
    {
        Vector2Int placementPositionGrid = GetPlacementOrigin(MousePosInWorldSpace(), GetConveyorDimensions(currentlySelectedConveyor));
        
        //Debug.Log(placementPositionGrid);
        if (IsOccupied(placementPositionGrid, GetConveyorDimensions(currentlySelectedConveyor)))
        {
            Debug.Log("Cell Occupied");
            return;
        }

        Vector3 placementPositionWorld = GridPosInWorldSpace(placementPositionGrid);
        placementPositionWorld.y = getCurrentFloorY();
        
        Quaternion currentConveyorRotation = Quaternion.Euler(0f, (float)(int)currentRotation, 0f);
        GameObject currentConveyor = Instantiate(allConveyorPrefabs[currentlySelectedConveyor].getConveyorPrefab(), placementPositionWorld, currentConveyorRotation);
        MarkOccupied(currentConveyor, GetRequiredSCells(currentlySelectedConveyor, placementPositionGrid));
        currentConveyor.GetComponent<ConveyorBelt>().SetSpeed(currentlySelectedSpeed);
        Debug.Log("Conveyor placed");

    }

    public void DeleteConveyor()
    {
        Vector2Int deletionPositionGrid = WorldToCellPos(MousePosInWorldSpace());
        PlacedConveyorInfo info = grid[deletionPositionGrid.x, deletionPositionGrid.y, currentFloor];
        if (!IsOccupied(deletionPositionGrid, new Vector2Int(1, 1)) || info == null)
        {
            return;
        }
        Vector2Int[] cells = info.getOccupiedCells();
        for (int i = 0; i < cells.Length; i++)
        {
            grid[cells[i].x, cells[i].y, currentFloor] = null;
        }
        Destroy(info.getOccupiedObject());
        
    }

    public void ChangeSelection(bool increment)
    {
        currentlySelectedConveyor += increment ? 1 : -1;
        currentlySelectedConveyor = Mathf.Clamp(currentlySelectedConveyor, 0, allConveyorPrefabs.Length - 1); //Subtract 1 due to length starting at 1 and array starting at 0
        conveyorSelectionUI.sprite = allConveyorPrefabs[currentlySelectedConveyor].getConveyorIcon();
        //Debug.Log(currentlySelected + " " + allConveyorPrefabs.Length);
    }

    public void ChangeRotation(ConveyorRotationEnum rotationValue)
    {
        currentRotation = rotationValue;
    }

    public void ChangeFloor(bool increment)
    {
        currentFloor = math.clamp(increment ? currentFloor + 1 : currentFloor - 1, 0, totalFloorCount - 1);
        Vector3 newPlanePosition = new Vector3(spawnPlaneTransform.position.x, getCurrentFloorY(), spawnPlaneTransform.position.z);
        spawnPlane = new Plane(spawnPlaneTransform.up, newPlanePosition);
        selectedFloorTextUI.text = (currentFloor + 1).ToString();
    }

    public void ChangeSpeed(bool increment)
    {
        currentlySelectedSpeed = math.clamp(increment ? currentlySelectedSpeed + 1 : currentlySelectedSpeed - 1, 0,
            _conveyorManager.getTotalSpeeds() - 1);
        conveyorSpeedUI.sprite = _conveyorManager.getSpeedImage(currentlySelectedSpeed);
    }
    
    void MarkOccupied(GameObject conveyorObject, Vector2Int[] occupiedCells)
    {
        PlacedConveyorInfo info = new PlacedConveyorInfo();
        info.setOccupiedObject(conveyorObject);
        info.setOccupiedCells(occupiedCells);
        
        for (int i = 0; i < occupiedCells.Length; i++)
        {
             grid[occupiedCells[i].x, occupiedCells[i].y, currentFloor] = info;
        }
        Debug.Log($"Marked {occupiedCells.Length} cells, first cell: {occupiedCells[0]}, object: {conveyorObject}");
    }

    private Vector3 GridPosInWorldSpace(Vector2Int  gridPos)
    {
        return new Vector3(
            ((gridPos.x - gridOffset.x) * cellSize) + (GetConveyorDimensions(currentlySelectedConveyor).x * cellSize / 2f),
            getCurrentFloorY(),
            ((gridPos.y - gridOffset.y) * cellSize) + (GetConveyorDimensions(currentlySelectedConveyor).y * cellSize / 2f)
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
            math.clamp(Mathf.FloorToInt(worldPos.x / cellSize) + gridOffset.x, 0, gridSize.x - 1),
            math.clamp(Mathf.FloorToInt(worldPos.z / cellSize) + gridOffset.y, 0, gridSize.y - 1));
    }
    
    public Vector2Int GetPlacementOrigin(Vector3 worldPos, Vector2Int conveyorSize)
    {
        return new Vector2Int(
            math.clamp(Mathf.RoundToInt((worldPos.x / cellSize) - (conveyorSize.x / 2f)) + gridOffset.x, 0, gridSize.x - 1),
            math.clamp(Mathf.RoundToInt((worldPos.z / cellSize) - (conveyorSize.y / 2f)) + gridOffset.y, 0, gridSize.y - 1));
    }

    public Vector2Int GetConveyorDimensions(int conveyorID)
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

    public Vector2Int[] GetRequiredSCells(int conveyorID, Vector2Int origin)
    {
        Vector2Int[] output = new Vector2Int[GetConveyorDimensions(conveyorID).x * GetConveyorDimensions(conveyorID).y];
        for (int x = 0; x < GetConveyorDimensions(conveyorID).x; x++)
        {
            for (int y = 0; y < GetConveyorDimensions(conveyorID).y; y++)
            {
                output[x * GetConveyorDimensions(conveyorID).y + y] = origin + new Vector2Int(x, y);
            }
        }
        return output;
    }

    //Denotes whether cells are occupied from an origin and size
    bool IsOccupied(Vector2Int origin, Vector2Int sizeToCheck)
    {
        Debug.Log($"Origin: {origin}, sizeToCheck: {sizeToCheck} sizeofarray: {grid}");
        
        //Checks each cell the conveyor needs
        for (int x = 0; x < sizeToCheck.x; x++)
        {
            for (int y = 0; y < sizeToCheck.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (cell.x >= gridSize.x || cell.y >=gridSize.y || cell.x < 0 || cell.y < 0 || grid[cell.x, cell.y, currentFloor] != null) return true;
            }
        }
        return false;
    }
    
}
