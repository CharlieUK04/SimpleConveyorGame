using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private ConveyorSpawner _conveyorSpawner;
    [SerializeField] private float panSpeed = 10f;
    [SerializeField] private float edgeSize = 20f;          // how close to the screen edge counts, in pixels
    [SerializeField] private float heightPerFloor = 1.5f;
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 focusPoint;   // where the camera wants to be
    private Vector3 velocity;

    private void Awake()
    {
        _conveyorSpawner = GameObject.FindAnyObjectByType<ConveyorSpawner>();
    }

    void Start()
    {
        focusPoint = transform.position;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        Vector2 mouse = Mouse.current.position.ReadValue();

        // Which way the mouse is pushing: x = left/right, y = up/down on screen
        float pushX = 0f;
        float pushY = 0f;
        if (mouse.x <= edgeSize) pushX = -1f;
        else if (mouse.x >= Screen.width - edgeSize) pushX = 1f;
        if (mouse.y <= edgeSize) pushY = -1f;
        else if (mouse.y >= Screen.height - edgeSize) pushY = 1f;

        // Turn the push into a direction that matches the camera's view, flattened onto the floor
        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;

        focusPoint += (right * pushX + forward * pushY) * panSpeed * Time.deltaTime;

        // Keep the focus point inside the grid walls
        focusPoint.x = Mathf.Clamp(focusPoint.x, _conveyorSpawner.GridMinWorld.x, _conveyorSpawner.GridMaxWorld.x);
        focusPoint.z = Mathf.Clamp(focusPoint.z, _conveyorSpawner.GridMinWorld.z, _conveyorSpawner.GridMaxWorld.z);

        // Height follows the current floor
        focusPoint.y = _conveyorSpawner.getCurrentFloorY();

        // Smoothly move the camera rig toward the focus point
        transform.position = Vector3.SmoothDamp(transform.position, focusPoint, ref velocity, smoothTime);
    }
}
