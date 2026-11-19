using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsManager : MonoBehaviour
{
    private ConveyorSpawner _conveyorSpawner;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _conveyorSpawner = FindAnyObjectByType<ConveyorSpawner>();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _conveyorSpawner.SpawnConveyor();
        }
    }
    
    public void OnChangeSelection(InputAction.CallbackContext context)
    {
        //Makes sure the values are drastic enough before changing
        //We use .5 instead of checking for exactly 1, to allow for joysticks to work in the future,
        //Since joysticks often don't return exact values (e.g. neutral may be 0.1, full left may be -0.9)
        if (context.performed && (context.ReadValue<float>() < -0.5f || context.ReadValue<float>() > 0.5f))
        {
            //We only need to check one value, since we ensured it was between those two values already (above .5 or below -.5)
            _conveyorSpawner.ChangeSelection(context.ReadValue<float>() > 0.5f);
        }
    }
    
    public void OnSetRotation(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (context.ReadValue<Vector2>().x > 0.8f)       // Right
            {
                _conveyorSpawner.ChangeRotation(ConveyorSpawner.ConveyorRotationEnum.Right);
            }
            else if (context.ReadValue<Vector2>().x < -0.8f) // Left
            {
                _conveyorSpawner.ChangeRotation(ConveyorSpawner.ConveyorRotationEnum.Left);
            }
            else if (context.ReadValue<Vector2>().y > 0.8f)  // Up
            {
                _conveyorSpawner.ChangeRotation(ConveyorSpawner.ConveyorRotationEnum.Up);
            }
            else if (context.ReadValue<Vector2>().y < -0.8f) // Down
            {
                _conveyorSpawner.ChangeRotation(ConveyorSpawner.ConveyorRotationEnum.Down);
            }
        }
    }

    public void OnDelete(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _conveyorSpawner.DeleteConveyor();
        }
    }

    public void OnChangeFloor(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _conveyorSpawner.ChangeFloor(context.ReadValue<float>() > 0.5f);
        }
    }
    
    public void OnChangeSpeed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _conveyorSpawner.ChangeSpeed(context.ReadValue<float>() > 0.5f);
        }
    }
    
}
