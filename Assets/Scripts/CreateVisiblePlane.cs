using UnityEngine;

public class CreateVisiblePlane : MonoBehaviour
{
    private ConveyorSpawner _conveyorSpawner;
    [SerializeField] private float yOffset = 0f;

    void Start()
    {
        _conveyorSpawner = GameObject.FindAnyObjectByType<ConveyorSpawner>();
        
        Vector3 min = _conveyorSpawner.GridMinWorld;
        Vector3 max = _conveyorSpawner.GridMaxWorld;

        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min;

        transform.position = new Vector3(center.x, yOffset, center.z);

        // Unity's built-in Plane is 10 x 10 units, so divide the size by 10
        transform.localScale = new Vector3(size.x / 10f, 1f, size.z / 10f);
    }
}
