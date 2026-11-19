using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("Value is based on the ConveyorHandlers list of available speeds, not the speed itself")]
    public int speed = 0;
    [SerializeField] private Renderer beltRenderer;
    [SerializeField] private GameObject[] conveyorLegs;

    //The speed the item centres on the belt, relative to the speed of the belt
    private const float CentringSpeedRelativeToSpeed = 1.35f;

    private static ConveyorManager _conveyorManager;
    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPosition = Vector3.zero;
    
    private float _beltLength = 0f;
    private float _currentProgress = 0f;
    private Vector3 _itemPosition = Vector3.zero;
    private Vector3 _beltDirHorizontal = Vector3.zero;
    private Vector3 _idealItemPosition = Vector3.zero;
    
    private List<Item> _allItems = new List<Item>();
    
    private void Awake()
    {
        _conveyorManager = FindAnyObjectByType<ConveyorManager>();
        
        //We use awake to calculate movement values, since ontriggerenter can fire before start
        _startPosition = transform.Find("Start").position;
        _endPosition = transform.Find("End").position;
        _beltLength = Vector3.Distance(_startPosition, _endPosition);
        _beltDirHorizontal = Vector3.ProjectOnPlane(_endPosition - _startPosition, Vector3.up).normalized;
        
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            if (transform.position.y > .25f)
            {
                foreach(GameObject conveyorLeg in conveyorLegs)
                    conveyorLeg.SetActive(false);
            }
    }

    /// <summary>
    /// Sets the speed value of the conveyor
    /// </summary>
    public void SetSpeed(int setSpeed)
    {
        speed = setSpeed;
        beltRenderer.sharedMaterial = _conveyorManager.GetConveyorMaterial(speed);
        Debug.Log(speed);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Item>(out var outItem))
        {
            Debug.Log($"Switched to conveyor {gameObject.name} with speed {speed}");
            _allItems.Add(outItem);
            outItem.currentConveyorBelt = this;
            outItem.progressAcrossBelt = GetProgressFromPosition(outItem.transform.position);
            //outItem.transform.position = new Vector3(outItem.transform.position.x, startPosition.y + outItem.itemYOffset, outItem.transform.position.z);
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.TryGetComponent<Item>(out var outItem))
    //    {
    //        allItems.Remove(outItem);
    //    }
    //}
    
    /// <summary>
    /// Gets the progress between the start and end position of the conveyor, based on the given Vector3
    /// </summary>
    /// <returns>The progress between the start and end position as a float</returns>
    private float GetProgressFromPosition(Vector3 position)
    {
            Vector3 ab = _endPosition - _startPosition;
            Vector3 av = position - _startPosition;
            return Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = _allItems.Count - 1; i >= 0; i--)
        {
            if (i > _allItems.Count - 1)
                continue;
            if (_allItems[i].currentConveyorBelt.gameObject != this.gameObject)
            {
                _allItems.Remove(_allItems[i]);
                continue;
            }                     
            else if (_allItems[i].progressAcrossBelt >= 1f && _allItems[i].currentConveyorBelt == this)
            {
                _allItems[i].currentConveyorBelt = null;
                _allItems[i].progressAcrossBelt = 0f;
                _allItems.Remove(_allItems[i]);
                continue;
            }
            
            //Increments the items progress across the belt over time, starting from the position the item reaches the belt
            _allItems[i].progressAcrossBelt += (_conveyorManager.GetAvailableSpeed(speed)) * Time.deltaTime / _beltLength;
            
            _itemPosition = _allItems[i].transform.position;
            //The item position on the belt based solely off of the progress between the start and end points
            //Includes the items Y offset
            _idealItemPosition = Vector3.Lerp(_startPosition, _endPosition, _allItems[i].progressAcrossBelt);
            _idealItemPosition.y += _allItems[i].itemYOffset;
            
            _itemPosition = Vector3.MoveTowards(_itemPosition, _idealItemPosition, Time.deltaTime * (CentringSpeedRelativeToSpeed * _conveyorManager.GetAvailableSpeed(speed)));
            _itemPosition.y = _idealItemPosition.y;
            
            _allItems[i].transform.position = _itemPosition;
            
            
        }
    }
}
