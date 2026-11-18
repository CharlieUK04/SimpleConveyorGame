using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("Value is based on the ConveyorHandlers list of available speeds, not the speed itself")]
    [SerializeField] private int speed = 0;

    private const float centringSpeedRelativeToSpeed = 2f;
    [SerializeField] private Renderer beltRenderer;
    [SerializeField] private bool isFlipped = false;

    private static ConveyorManager _conveyorHandler;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 endPosition = Vector3.zero;
    private float beltLength = 0f;
    private float currentProgress = 0f;

    private List<Item> allItems = new List<Item>();
    
    private void Awake()
    {
        _conveyorHandler = FindAnyObjectByType<ConveyorManager>();
        startPosition = transform.Find("Start").position;
        endPosition = transform.Find("End").position;
        beltLength = Vector3.Distance(startPosition, endPosition);
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            beltRenderer.sharedMaterial = _conveyorHandler.GetConveyorMaterial(speed, isFlipped);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Switched to conveyor " + gameObject.name);
        if (other.TryGetComponent<Item>(out var outItem))
        {
            allItems.Add(outItem);
            outItem.progressAcrossBelt = GetProgressFromPosition(outItem.transform.position);
            outItem.currentConveyorBelt = this;
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
            
            allItems[i].progressAcrossBelt += (_conveyorHandler.getAvailableSpeed(speed)) * Time.deltaTime / beltLength;
            Debug.Log(allItems[i].progressAcrossBelt);
            Vector3 itemPosition = Vector3.Lerp(startPosition, endPosition, allItems[i].progressAcrossBelt);
            itemPosition.y = itemPosition.y + allItems[i].itemYOffset;
            allItems[i].transform.position = itemPosition;
            
            if (allItems[i].currentConveyorBelt.gameObject != this.gameObject)
            {
                allItems.Remove(allItems[i]);
            }
        }
    }
}
