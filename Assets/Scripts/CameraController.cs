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

    private Vector3 _focusPoint;   // where the camera wants to be
    private Vector3 _velocity;

    /// <summary>
    /// Initializes the CameraController component by locating and caching the ConveyorSpawner reference.
    /// </summary>
    /// <remarks>
    /// During the Awake lifecycle, this method searches the scene hierarchy for a GameObject of type ConveyorSpawner and assigns its instance to the internal _conveyorSpawner field. This establishes the necessary dependency to access conveyor spawning functionality throughout the controller's operation.
    /// </remarks>
    private void Awake()
    {
        _conveyorSpawner = GameObject.FindAnyObjectByType<ConveyorSpawner>();
    }

    void Start()
    {
        _focusPoint = transform.position;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        Vector2 mouse = Mouse.current.position.ReadValue();

        // Which way the mouse is pushing: x = left/right, y = up/down on screen
        float HorizontalPush = 0f;
        float VerticalPush = 0f;
        if (mouse.x <= edgeSize) HorizontalPush = -1f;
        else if (mouse.x >= Screen.width - edgeSize) HorizontalPush = 1f;
        if (mouse.y <= edgeSize) VerticalPush = -1f;
        else if (mouse.y >= Screen.height - edgeSize) VerticalPush = 1f;

        // Turn the push into a direction that matches the camera's view, flattened onto the floor
        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;

        _focusPoint += (right * HorizontalPush + forward * VerticalPush) * panSpeed * Time.deltaTime;

        // Keep the focus point inside the grid walls
        _focusPoint.x = Mathf.Clamp(_focusPoint.x, _conveyorSpawner.gridMinWorld.x, _conveyorSpawner.gridMaxWorld.x);
        _focusPoint.z = Mathf.Clamp(_focusPoint.z, _conveyorSpawner.gridMinWorld.z, _conveyorSpawner.gridMaxWorld.z);

        // Height follows the current floor
        _focusPoint.y = _conveyorSpawner.GetCurrentFloorY();

        // Smoothly move the camera rig toward the focus point
        transform.position = Vector3.SmoothDamp(transform.position, _focusPoint, ref _velocity, smoothTime);
    }
}
