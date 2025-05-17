using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class GameInputs : MonoBehaviour
{
    public event EventHandler OnInteractAction; // Event to notify when the interact action is performed
    private PlayerInputActions playerInputActions;
    
    void Awake()
    {
        playerInputActions = new PlayerInputActions(); // Instantiate the input actions
        playerInputActions.Player.Enable(); // Enable the input actions
        playerInputActions.Player.Interact.performed += Interact_performed; // Subscribe to the Interact action
    }
    
    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    // Action Still
    public bool OnActionStill()
    {
        if(playerInputActions.Player.Still.IsPressed())
            return true;
        else
            return false;
    }

    // Action Jump
    public bool OnActionJump()
    {
        if(playerInputActions.Player.Jump.IsPressed())
            return true;
        else
            return false;
    }
    
    // Action Slam
    public bool OnActionSlam()
    {
        if(playerInputActions.Player.Slam.IsPressed())
            return true;
        else
            return false;   
    }
    
    // Action Dash
    public bool OnActionDash()
    {
        if(playerInputActions.Player.Dash.triggered.Equals(true))
            return true;
        else
            return false;
    }
    
    // Action Throw
    public bool OnActionThrow()
    {
        if(playerInputActions.Player.Throw.triggered.Equals(true))
            return true;
        else
            return false;
    }
    
}
