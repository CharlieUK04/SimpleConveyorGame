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
    private const float centringSpeedRelativeToSpeed = 1.35f;

    private static ConveyorManager _conveyorManager;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 endPosition = Vector3.zero;
    
    private float beltLength = 0f;
    private float currentProgress = 0f;
    private Vector3 itemPosition = Vector3.zero;
    private Vector3 beltDirHorizontal = Vector3.zero;
    private Vector3 idealItemPosition = Vector3.zero;
    
    private List<Item> allItems = new List<Item>();
    
    private void Awake()
    {
        _conveyorManager = FindAnyObjectByType<ConveyorManager>();
        
        //We use awake to calculate movement values, since ontriggerenter can fire before start
        startPosition = transform.Find("Start").position;
        endPosition = transform.Find("End").position;
        beltLength = Vector3.Distance(startPosition, endPosition);
        beltDirHorizontal = Vector3.ProjectOnPlane(endPosition - startPosition, Vector3.up).normalized;
        
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
            allItems.Add(outItem);
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

    private float GetProgressFromPosition(Vector3 position)
    {
            Vector3 AB = endPosition - startPosition;
            Vector3 AV = position - startPosition;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = allItems.Count - 1; i >= 0; i--)
        {
            if (i > allItems.Count - 1)
                continue;
            if (allItems[i].currentConveyorBelt.gameObject != this.gameObject)
            {
                allItems.Remove(allItems[i]);
                continue;
            }                     //We use 0.99f instead of 1f to account for floating point rounding errors
            else if (allItems[i].progressAcrossBelt > 0.99f && allItems[i].currentConveyorBelt == this)
            {
                allItems[i].currentConveyorBelt = null;
                allItems[i].progressAcrossBelt = 0f;
                allItems.Remove(allItems[i]);
                continue;
            }
            
            //Increments the items progress across the belt over time, starting from the position the item reaches the belt
            allItems[i].progressAcrossBelt += (_conveyorManager.getAvailableSpeed(speed)) * Time.deltaTime / beltLength;
            
            itemPosition = allItems[i].transform.position;
            //The item position on the belt based solely off of the progress between the start and end points
            //Includes the items Y offset
            idealItemPosition = Vector3.Lerp(startPosition, endPosition, allItems[i].progressAcrossBelt);
            idealItemPosition.y += allItems[i].itemYOffset;
            
            itemPosition = Vector3.MoveTowards(itemPosition, idealItemPosition, Time.deltaTime * (centringSpeedRelativeToSpeed * _conveyorManager.getAvailableSpeed(speed)));
            itemPosition.y = idealItemPosition.y;
            
            allItems[i].transform.position = itemPosition;
            
            
        }
    }
}
