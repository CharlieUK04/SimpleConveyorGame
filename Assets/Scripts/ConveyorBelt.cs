using System;
using UnityEditor;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private static ConveyorManager _conveyorHandler;
    
    [Tooltip("Value is based on the ConveyorHandlers list of available speeds, not the speed itself")]
    [SerializeField] private int speed = 0;
    [SerializeField] private Renderer beltRenderer;
    [SerializeField] private bool isFlipped = false;

    private void Awake()
    {
        _conveyorHandler = FindAnyObjectByType<ConveyorManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            beltRenderer.sharedMaterial = _conveyorHandler.GetConveyorMaterial(speed, isFlipped);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
