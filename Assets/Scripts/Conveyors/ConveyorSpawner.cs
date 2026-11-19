using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Mathematics;
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
    private Vector2Int _gridOffset;
    
    private Camera _mainCamera;
    private Transform _spawnPlaneTransform;
    private Plane _spawnPlane;
    private ConveyorManager _conveyorManager;
    
    private int _currentlySelectedConveyor = 0;
    private int _currentlySelectedSpeed = 0;
    private int _currentlySelectedFloor = 0;
    public float GetCurrentFloorY() {return _spawnPlaneTransform.position.y + (floorYOffset * _currentlySelectedFloor);}
    private ConveyorRotationEnum _currentlySelectedRotation = ConveyorRotationEnum.Left;

    private float _cellSize;

    private GameObject _placementPreview;
    private Vector2 _previewScale = Vector2.zero;
    private Vector3 _previewLocation = Vector3.zero;
    
    //3D Array holding all the conveyor information for each cell and floors
    private PlacedConveyorInfo[,,] _grid;
    
    //Returns the bottom left and bottom right of the grid, used for cinemachine camera and CreateVisiblePlain
    public Vector3 gridMinWorld => new Vector3(-_gridOffset.x * _cellSize, 0f, -_gridOffset.y * _cellSize);
    public Vector3 gridMaxWorld => new Vector3((gridSize.x - _gridOffset.x) * _cellSize, 0f, (gridSize.y - _gridOffset.y) * _cellSize);
    

    [Serializable]
    [Tooltip("The box collider on the first prefab element determines the cell size, based on its X size")]
    private class Conveyor
    {
        [SerializeField] public GameObject conveyorPrefab;
        [SerializeField] private Vector2Int conveyorSizeWxL = new Vector2Int(1, 3);
        
        /// <summary>
        /// Gets the conveyor prefab reference.
        /// </summary>
        /// <returns>The conveyor prefab GameObject</returns>
        public GameObject GetConveyorPrefab() {return conveyorPrefab;}
        
        /// <summary>
        /// Gets the size of the conveyor in grid dimensions (x,y)
        /// </summary>
        /// <returns>A Vector2Int showing the X and Y size of the conveyor on the grid</returns>
        public Vector2Int GetConveyorSize() {return conveyorSizeWxL;}
        [SerializeField] private Sprite conveyorIcon;
        
        /// <summary>
        /// Gets the sprite for the icon of the conveyor
        /// </summary>
        /// <returns>A sprite reference to use for the icon of the conveyor</returns>
        public Sprite GetConveyorIcon() {return conveyorIcon;}
    }

    private class PlacedConveyorInfo
    {
        private GameObject _occupiedObject;
        private Vector2Int[] _occupiedCells;
        
        /// <summary>
        /// Gets the GameObject of the PlacedConveyor
        /// </summary>
        /// <returns>A GameObject of the Placed Conveyor</returns>
        public GameObject GetOccupiedObject() {return _occupiedObject;}
        
        /// <summary>
        /// Gets the cells Occupied by the conveyor
        /// </summary>
        /// <returns>A Vector2Int array with the X and Y grid co-ords of all the cells this conveyor occupies</returns>
        public Vector2Int[] GetOccupiedCells() {return _occupiedCells;}
        
        /// <summary>
        /// Sets the GameObject of the conveyor to the input, if the occupiedObject is currently null
        /// </summary>
        public void SetOccupiedObject(GameObject value)
        { if(_occupiedObject == null) {_occupiedObject = value;} }
        
        /// <summary>
        /// Sets the cells occupied by the conveyor, if occupiedCells is currently null
        /// </summary>
        public void SetOccupiedCells(Vector2Int[] value)
        { if(_occupiedCells == null) {_occupiedCells = value; } }
    }
    
    /// <summary>
    /// Main 4 rotations for the conveyor, with int references for the degree values of each rotation
    /// </summary>
    public enum ConveyorRotationEnum
    {
        Left = 0,
        Up = 90,
        Right = 180,
        Down = 270
    }

    private void Awake()
    {
        _spawnPlaneTransform = GameObject.FindGameObjectWithTag("ConveyorSpawnPlane").transform;
        _grid = new PlacedConveyorInfo[gridSize.x, gridSize.y, totalFloorCount];
        _gridOffset =  new Vector2Int(Mathf.CeilToInt(gridSize.x / 2), Mathf.CeilToInt(gridSize.y / 2));
        _cellSize = allConveyorPrefabs[0].GetConveyorPrefab().GetComponent<BoxCollider>().size.x;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = Camera.main;
        
        _conveyorManager = GameObject.FindAnyObjectByType<ConveyorManager>();
        ChangeFloor(false);

        //Creates the preview square for placing conveyors
        _placementPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _placementPreview.name = "ConveyorPlacementPreview";
        _placementPreview.transform.localScale = new Vector3(_cellSize, .1f, _cellSize);
        _placementPreview.GetComponent<Renderer>().material = canPlaceMaterial;
        Destroy(_placementPreview.GetComponent<Collider>());
        //placementPreview.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        UpdatePreview();
    }
    
    /// <summary>
    /// Updates the position, scale, and rotation of the placement preview based on the currently selected conveyor
    /// </summary>
    private void UpdatePreview()
    {
        _previewScale = new Vector2(_cellSize * allConveyorPrefabs[_currentlySelectedConveyor].GetConveyorSize().x,
            _cellSize * allConveyorPrefabs[_currentlySelectedConveyor].GetConveyorSize().y);
        _previewLocation = GridPosInWorldSpace(GetPlacementOriginCentreOffset(MousePosInWorldSpace(), GetConveyorDimensions(_currentlySelectedConveyor)));
        _previewLocation.y = GetCurrentFloorY() + previewOffsetY;

        _placementPreview.transform.localScale = new Vector3(_previewScale.x, previewThickness, _previewScale.y);
        _placementPreview.transform.position = _previewLocation;
        _placementPreview.transform.rotation = Quaternion.Euler(new Vector3(0, (float)(int)_currentlySelectedRotation, 0));

    }

    /// <summary>
    /// Spawns a conveyor based on currentlySelectedConveyor, currentlySelectedSpeed, currentFloor, and the current mouse position
    /// </summary>
    public void SpawnConveyor()
    {
        Vector2Int placementPositionGrid = GetPlacementOriginCentreOffset(MousePosInWorldSpace(), GetConveyorDimensions(_currentlySelectedConveyor));
        
        //Debug.Log(placementPositionGrid);
        if (IsOccupied(placementPositionGrid, GetConveyorDimensions(_currentlySelectedConveyor)))
        {
            Debug.Log("Cell Occupied");
            return;
        }

        Vector3 placementPositionWorld = GridPosInWorldSpace(placementPositionGrid);
        placementPositionWorld.y = GetCurrentFloorY();
        
        Quaternion currentConveyorRotation = Quaternion.Euler(0f, (float)(int)_currentlySelectedRotation, 0f);
        GameObject currentConveyor = Instantiate(allConveyorPrefabs[_currentlySelectedConveyor].GetConveyorPrefab(), placementPositionWorld, currentConveyorRotation);
        MarkOccupied(currentConveyor, GetRequiredCells(_currentlySelectedConveyor, placementPositionGrid));
        currentConveyor.GetComponent<ConveyorBelt>().SetSpeed(_currentlySelectedSpeed);
        Debug.Log("Conveyor placed");

    }

    /// <summary>
    /// Deletes the conveyor currently at the mouse position
    /// </summary>
    public void DeleteConveyor()
    {
        Vector2Int deletionPositionGrid = WorldToCellPos(MousePosInWorldSpace());
        PlacedConveyorInfo info = _grid[deletionPositionGrid.x, deletionPositionGrid.y, _currentlySelectedFloor];
        if (!IsOccupied(deletionPositionGrid, new Vector2Int(1, 1)) || info == null)
        {
            return;
        }
        Vector2Int[] cells = info.GetOccupiedCells();
        for (int i = 0; i < cells.Length; i++)
        {
            _grid[cells[i].x, cells[i].y, _currentlySelectedFloor] = null;
        }
        Destroy(info.GetOccupiedObject());
        
    }

    /// <summary>
    /// Changes currentlySelectedConveyor by 1 or -1, clamped between all valid conveyor options
    /// </summary>
    public void ChangeSelection(bool increment)
    {
        _currentlySelectedConveyor += increment ? 1 : -1;
        _currentlySelectedConveyor = Mathf.Clamp(_currentlySelectedConveyor, 0, allConveyorPrefabs.Length - 1); //Subtract 1 due to length starting at 1 and array starting at 0
        conveyorSelectionUI.sprite = allConveyorPrefabs[_currentlySelectedConveyor].GetConveyorIcon();
        //Debug.Log(currentlySelected + " " + allConveyorPrefabs.Length);
    }

    /// <summary>
    /// Changes currentlySelectedRotation
    /// </summary>
    public void ChangeRotation(ConveyorRotationEnum rotationValue)
    {
        _currentlySelectedRotation = rotationValue;
    }

    /// <summary>
    /// Changes the current selected floor, clamped between all valid floors
    /// </summary>
    public void ChangeFloor(bool increment)
    {
        _currentlySelectedFloor = math.clamp(increment ? _currentlySelectedFloor + 1 : _currentlySelectedFloor - 1, 0, totalFloorCount - 1);
        Vector3 newPlanePosition = new Vector3(_spawnPlaneTransform.position.x, GetCurrentFloorY(), _spawnPlaneTransform.position.z);
        _spawnPlane = new Plane(_spawnPlaneTransform.up, newPlanePosition);
        selectedFloorTextUI.text = (_currentlySelectedFloor + 1).ToString();
    }

    /// <summary>
    /// Changes the currently selected speed, clamped between all valid speeds
    /// </summary>
    public void ChangeSpeed(bool increment)
    {
        _currentlySelectedSpeed = math.clamp(increment ? _currentlySelectedSpeed + 1 : _currentlySelectedSpeed - 1, 0,
            _conveyorManager.GetTotalSpeeds() - 1);
        conveyorSpeedUI.sprite = _conveyorManager.GetSpeedImage(_currentlySelectedSpeed);
    }
    
    /// <summary>
    /// Marks cells as occupied by a given GameObject
    /// </summary>
    void MarkOccupied(GameObject conveyorObject, Vector2Int[] occupiedCells)
    {
        PlacedConveyorInfo info = new PlacedConveyorInfo();
        info.SetOccupiedObject(conveyorObject);
        info.SetOccupiedCells(occupiedCells);
        
        for (int i = 0; i < occupiedCells.Length; i++)
        {
             _grid[occupiedCells[i].x, occupiedCells[i].y, _currentlySelectedFloor] = info;
        }
        Debug.Log($"Marked {occupiedCells.Length} cells, first cell: {occupiedCells[0]}, object: {conveyorObject}");
    }

    /// <summary>
    /// Converts grid position to world position
    /// </summary>
    /// <returns>A Vector3 of the world position from the given grid position</returns>
    private Vector3 GridPosInWorldSpace(Vector2Int gridPos)
    {
        return new Vector3(
            ((gridPos.x - _gridOffset.x) * _cellSize) + (GetConveyorDimensions(_currentlySelectedConveyor).x * _cellSize / 2f),
            GetCurrentFloorY(),
            ((gridPos.y - _gridOffset.y) * _cellSize) + (GetConveyorDimensions(_currentlySelectedConveyor).y * _cellSize / 2f)
        );
    }
    
    /// <summary>
    /// Gets the current mouse position in world space to the conveyor spawn plane
    /// </summary>
    /// <returns>A Vector3 of the current mouse position on the spawn plane</returns>
    Vector3 MousePosInWorldSpace()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (_spawnPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        Debug.Log("Click did not register on the floor plane!");
        
        return Vector2.zero;
    }
    /// <summary>
    /// Snaps a world position to the nearest cell position
    /// </summary>
    /// <returns>A Vector2Int of the cell position from the given world position</returns>
    public Vector2Int WorldToCellPos(Vector3 worldPos)
    {
        return new Vector2Int(
            math.clamp(Mathf.FloorToInt(worldPos.x / _cellSize) + _gridOffset.x, 0, gridSize.x - 1),
            math.clamp(Mathf.FloorToInt(worldPos.z / _cellSize) + _gridOffset.y, 0, gridSize.y - 1));
    }
    
    /// <summary>
    /// Gets the origin to place a conveyor with an offset to centre it
    /// </summary>
    /// <returns>A Vector2Int of the placement origin of a conveyor with an offset to centre it based on the given size</returns>
    public Vector2Int GetPlacementOriginCentreOffset(Vector3 worldPos, Vector2Int conveyorSize)
    {
        return new Vector2Int(
            math.clamp(Mathf.RoundToInt((worldPos.x / _cellSize) - (conveyorSize.x / 2f)) + _gridOffset.x, 0, gridSize.x - 1),
            math.clamp(Mathf.RoundToInt((worldPos.z / _cellSize) - (conveyorSize.y / 2f)) + _gridOffset.y, 0, gridSize.y - 1));
    }

    /// <summary>
    /// Gets the current dimensions of a conveyor given its ID, accounting for its rotation
    /// </summary>
    /// <returns>A Vector2Int of the conveyor dimensions</returns>
    public Vector2Int GetConveyorDimensions(int conveyorID)
    {
        if (_currentlySelectedRotation == ConveyorRotationEnum.Left || _currentlySelectedRotation == ConveyorRotationEnum.Right)
        {
            return new Vector2Int(
                allConveyorPrefabs[conveyorID].GetConveyorSize().x, 
                allConveyorPrefabs[conveyorID].GetConveyorSize().y);
        }
        return new Vector2Int(
            allConveyorPrefabs[conveyorID].GetConveyorSize().y, 
            allConveyorPrefabs[conveyorID].GetConveyorSize().x);
    }

    /// <summary>
    /// Gets the cell footprint required for conveyor placement at this grid position.
    /// </summary>
    /// <returns>Array of Vector2Int representing required cells</returns>
    public Vector2Int[] GetRequiredCells(int conveyorID, Vector2Int origin)
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

    /// <summary>
    ///Denotes whether cells are occupied from an origin and size
    /// </summary>
    /// <returns>True if any cells in the selection are occupied, false if they are all unoccupied</returns>
    bool IsOccupied(Vector2Int origin, Vector2Int sizeToCheck)
    {
        Debug.Log($"Origin: {origin}, sizeToCheck: {sizeToCheck} sizeofarray: {_grid}");
        
        //Checks each cell the conveyor needs
        for (int x = 0; x < sizeToCheck.x; x++)
        {
            for (int y = 0; y < sizeToCheck.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (cell.x >= gridSize.x || cell.y >=gridSize.y || cell.x < 0 || cell.y < 0 || _grid[cell.x, cell.y, _currentlySelectedFloor] != null) return true;
            }
        }
        return false;
    }
    
}
