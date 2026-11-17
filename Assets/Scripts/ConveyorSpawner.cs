using UnityEngine;

public class ConveyorSpawner : MonoBehaviour
{
    private Camera mainCamera;
    private Transform spawnPlane;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        spawnPlane = GameObject.FindGameObjectWithTag("ConveyorSpawnPlane").transform;
        Debug.Log(spawnPlane.tag);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector2 MousePosInWorldSpace()
    {
        Ray _ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        return Vector2.zero;
    }
    
}
