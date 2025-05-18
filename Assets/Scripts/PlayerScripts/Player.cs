using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

/*
 * ToDo:
 * 01. Basic Movement           [X]
 * 02. Collision Detection      [X]
 * 03. Jump                     [X]
 * 04. Crouch                   [X]
 * 05. CrouchJump               [X]
 * 06. Dash                     [X]
 * 07. Slam                     [X]
 * 08. Interact                 [X]
 * 09. Pick up                  [X]
 * 10. Place                    [X]
 * 11. Drop                     [X]
 */

public class Player : MonoBehaviour, IWorkshopObjectParent
{
    [SerializeField] private GameInputs gameInputs;
    [Header("Movement")] 
    [SerializeField] private float walkSpeed;
    private float moveSpeed; 
    [SerializeField] private Transform cameraOrientation; 
    [SerializeField] private float rotationSpeed; 
    private bool isStill = false; 
    [SerializeField] private float groundDrag; 
    private enum MovementState
    {
        idle,
        walking,
        air,
        still,
        slamming
    }
    [SerializeField] private MovementState playerState; 
    
    [Header("Jumping")]
    [SerializeField] private float leapForce;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airSpeed; 
    private bool readyToJump = true;
    
    [Header("Slam Settings")]
    [SerializeField] private float slamDownForce; 
    [SerializeField] private float freezeDuration; 
    [SerializeField] private float slamCooldown; 
    private bool isSlamming = false; 
    
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed; 
    [SerializeField] private float dashDuration; 
    [SerializeField] private float dashCooldown; 
    private bool isDashing; 
    private float dashTimer; 
    private float dashCooldownTimer; 
    
    [Header("Ground Check")]
    [SerializeField] private float groundRayHeight; 
    [SerializeField] private LayerMask whatIsGround; 
    private bool isGrounded; 
    [Header("Wall Check")]
    [SerializeField] private LayerMask whatIsWall; 
    // Visualization variables for wall checks
    [SerializeField] private bool visualizeWallCheck; 
    [SerializeField] private float visualizationHeight;  
    [SerializeField] private float visualizationLength;  
    [SerializeField] private float capsuleBottomOffset; 
    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle; 
    [SerializeField] private float slopeMultiplier; 
    private RaycastHit slopeHit; 
    private bool exitingSlope; 
    
    private PlayerInputActions piActions; 

    [Header("Interactions")] 
    [SerializeField] private LayerMask whatIsInteractable; 
    private BaseContraption selectedContraption; 
    private Vector3 lastInteractDir; 
    
    private WorkshopObject workshopObject;
    [SerializeField] private Transform resourceHoldPoint;
    /*
    ******************************************************************************************
    [SerializeField] private LayerMask whatIsInteractable; // Layer to identify interactable objects
    public Transform itemHoldPoint; // The empty GameObject where picked ITEMS will be parented
    public Transform toolHoldPoint; // The empty GameObject where picked TOOLS will be parented
    ******************************************************************************************
    */
    
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpwardAngle = 0.3f; // arc adjustment
    
    private float horizontalInput; 
    private float verticalInput; 
    private Vector3 moveDirection; 
    private Rigidbody rb;
    
    public static Player Instance { get; private set; } //will change for multiplayer
    
    public event EventHandler<OnSelectContraptionChangeEventArgs> OnSelectedContraptionChanged;
    public class OnSelectContraptionChangeEventArgs : EventArgs
    {
        public BaseContraption SelectedContraption;
    }

    private void Start()
    {
        gameInputs.OnInteractAction += GameInputs_OnInteractAction; // Subscribe to the interact action event
    }
    private void GameInputs_OnInteractAction(object sender, EventArgs e)
    {
        if (selectedContraption != null)
        {
            selectedContraption.Interact(this); // Call the interact method on the selected object
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Access the Rigidbody component attached to the same object
        rb.freezeRotation = true; // Prevent Rigidbody from rotating
        if (Instance != null)
        {
            Debug.LogError("There is more than one Player instance in the scene.");
        }
        Instance = this; // Set the static instance to this object (will change for multiplayer)
    }
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundRayHeight + 0.2f, whatIsGround);
        MyInput();
        SpeedControl(); 
        StateHandler(); 
        RotatePlayer();

        isStill = gameInputs.OnActionStill();
        
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    private void FixedUpdate()
    {
        MovePlayer(); 
        HandleInteract();
    }
    private void MyInput()
    {
        // Read movement vector, normalize it and assign to horizontal and vertical inputs
        Vector2 inputVector = gameInputs.GetMovementVectorNormalized();
        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;

        if (gameInputs.OnActionJump())
        {
            OnJump();
        }
        if (gameInputs.OnActionSlam())
        {
            OnSlam();
        }
        if (gameInputs.OnActionDash())
        {
            OnDash();
        }
        if (gameInputs.OnActionThrow())
        {
            OnThrow();
        }
    }
    private void StateHandler()
    {
        if (isGrounded) 
        {
            if (horizontalInput == 0 && verticalInput == 0) // If the player is not moving, consider the player to be idle
            {
                playerState = MovementState.idle;
            }
            else if (isStill) // If the player is still, consider the player to be still
            {
                playerState = MovementState.still;
            }
            else // Otherwise, consider the player to be walking
            {
                playerState = MovementState.walking;
                moveSpeed = walkSpeed; // Set the move speed to the walk speed
            }
        }
        else // Otherwise, consider the player to be in the air
        {
            moveSpeed = airSpeed; // Set the move speed to the air speed
            if(isSlamming)
                playerState = MovementState.slamming;
            else
                playerState = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        // Player movement code
        // disable movement if the player is still on the ground
        if (isStill && isGrounded) {
            // Skip movement code
            return;
        }
        moveDirection = cameraOrientation.forward * verticalInput + cameraOrientation.right * horizontalInput; // Calculate movement direction
        
        // Capsule cast variables (for wall checks)
        float capsuleHeight = visualizationHeight; // Ensure the capsule has a minimum size
        float capsuleRadius = 0.01f; // Radius of the capsule, should be less than half the player width
        Vector3 capsuleBottom = transform.position + Vector3.up * (capsuleBottomOffset - groundRayHeight); // Adjust bottom using offset
        Vector3 capsuleTop = capsuleBottom + Vector3.up * capsuleHeight; // Top of the capsule

        // Perform a capsule cast to check for walls in the direction of movement
        if (Physics.CapsuleCast(capsuleBottom, capsuleTop, capsuleRadius, moveDirection.normalized, out RaycastHit hit, visualizationLength, whatIsWall))
        {
            // Calculate the angle between the surface normal and the up vector
            float contactAngle = Vector3.Angle(Vector3.up, hit.normal);

            // If the capsule cast hits a wall and the angle is too steep, log it and do not apply movement
            if (contactAngle > maxSlopeAngle)
            {
                return; 
            }
        }
        
        // Visualize the capsule cast for debugging
        if (visualizeWallCheck)
        {
            Debug.DrawLine(capsuleBottom, capsuleBottom + moveDirection.normalized * visualizationLength, Color.green, 0.1f);
            Debug.DrawLine(capsuleTop, capsuleTop + moveDirection.normalized * visualizationLength, Color.green, 0.1f);
            Debug.DrawLine(capsuleBottom + moveDirection.normalized * visualizationLength, capsuleTop + moveDirection.normalized * visualizationLength, Color.green, 0.1f);
            Debug.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.down * (groundRayHeight), Color.red);
        }
        
        // If on a slope and not exiting, apply forces differently
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection().normalized * (moveSpeed * slopeMultiplier), ForceMode.Force); // Apply force based on slope direction

            // If moving upward on a slope, apply additional downward force
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force); // Apply downward force to stick to the slope
        }
        else
        {
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10), ForceMode.Force); // Apply force based on movement direction (Default)
        }
        

        // Control gravity application based on whether player is on a slope
        rb.useGravity = !OnSlope();
    }
    private void SpeedControl()
    {
        // Check if the player is on a slope and not exiting
        if (OnSlope() && !exitingSlope)
        {
            // If moving too fast, limit the velocity
            if (rb.velocity.magnitude > walkSpeed)
                rb.velocity = rb.velocity.normalized * walkSpeed; // Limit the velocity to the move speed
        }
        else 
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Get the horizontal velocity
            // If moving too fast, limit the velocity
            if (flatVel.magnitude > walkSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * walkSpeed; // Limit the velocity to the move speed
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z); // Update the velocity
            }
        }
    }
    private bool OnSlope()
    {
        // Handle slope detection
        // Perform a raycast downward to check for slopes
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundRayHeight + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); // Calculate the angle between the surface normal and the up vector
            return angle < maxSlopeAngle && angle != 0; // Return true if the angle is less than the maximum slope angle
        }
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        // Handle slope movement direction
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized; // Project the movement direction on the slope normal
    }
    private void RotatePlayer()
    {
        // Handle player rotation based on movement direction
        moveDirection = cameraOrientation.forward * verticalInput + cameraOrientation.right * horizontalInput; // Calculate movement direction
        // Check if there is significant movement to warrant a rotation update (Using sqrMagnitude for efficiency)
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up); // Calculate the target rotation
            // Spherically interpolate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }  
    }
    
    private void OnJump()
    {// Handle player jumping
        // Check if the player is ready to jump and is on the ground
        if (readyToJump && isGrounded) 
        {
            readyToJump = false; // Set the flag to false
            if(isStill)
            {
                rb.velocity = new Vector3(rb.velocity.x * 0.6f, 0, rb.velocity.z * 0.6f); // Reset the vertical velocity
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse); // Apply an impulse force upwards
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x * 0.3f, 0, rb.velocity.z * 0.3f); // Reset the vertical velocity
                rb.AddForce(transform.up * leapForce, ForceMode.Impulse); // Apply an impulse force upwards
            }
            Invoke(nameof(JumpReset), jumpCooldown); // Call the JumpReset method after the cooldown
        }

    }
    private void JumpReset()
    {
        // Handle jump cooldown
        readyToJump = true; // Set the flag to true
    }
    
    private void OnSlam() 
    {
        // Handle the slam action
        // Check if the player is not isGrounded and not currently slamming
        if (!isGrounded && !isSlamming) {
            StartCoroutine(PerformSlam()); // Perform the slam action
        }
    }
    private IEnumerator PerformSlam()
    {
        // Perform the slam action
        isSlamming = true;
        // Freezes player movement and physics
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        
        yield return new WaitForSeconds(freezeDuration); // Wait for the specified freeze duration
        rb.isKinematic = false; // Re-enable physics and apply slam down force
        rb.AddForce(Vector3.down * slamDownForce, ForceMode.Impulse); // Apply slam down force
        
        Invoke(nameof(SlamReset), slamCooldown); // Call the JumpReset method after the cooldown
    }
    private void SlamReset()
    {
        // Handle jump cooldown
        isSlamming = false; // Set the flag to true
    }
    
    private void OnDash()
    {
        if (!isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(PerformDash());
        }
    }
    private IEnumerator PerformDash()
    {
        isDashing = true;
        Vector3 dashDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            rb.velocity = dashDirection * dashSpeed;
            yield return null;
        }

        rb.velocity = Vector3.zero; // Optional: Stop the player completely after dashing
        isDashing = false;
        dashCooldownTimer = dashCooldown;
    }

    private void HandleInteract()
    {
        if (moveDirection != Vector3.zero)
            lastInteractDir = moveDirection;

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, whatIsInteractable))
        {
            BaseContraption baseContraption = raycastHit.transform.GetComponent<BaseContraption>();
            if (baseContraption != null)
            {
                if (baseContraption != selectedContraption)
                {
                    SetSelectedContraption(baseContraption);
                }
            }
            else
            {
                // Check for unparented WorkshopObject
                WorkshopObject workshopObject = raycastHit.transform.GetComponent<WorkshopObject>();
                if (workshopObject != null && 
                    workshopObject.GetWorkshopObjectParent() == null && 
                    workshopObject.CanBePickedUp())
                {
                    SetSelectedContraption(new FreeWorkshopObject(workshopObject));
                }

                else
                {
                    SetSelectedContraption(null);
                }
            }
        }
        else
        {
            SetSelectedContraption(null);
        }
    }
    
    private void SetSelectedContraption(BaseContraption selectedContraption)
    {
        this.selectedContraption = selectedContraption; // Set the selected conveyor belt
        OnSelectedContraptionChanged?.Invoke(this, new OnSelectContraptionChangeEventArgs
        {
            SelectedContraption = selectedContraption
        });
    }

    public Transform GetWorkshopObjectTransform()
    {
        return resourceHoldPoint;
    }
    
    public void SetWorkshopObject(WorkshopObject workshopObject)
    {
        this.workshopObject = workshopObject;
    }
    
    public WorkshopObject GetWorkshopObject()
    {
        return workshopObject;
    }
    
    public void ClearWorkshopObject()
    {
        workshopObject = null;
    }
    
    public bool HasWorkshopObject()
    {
        return workshopObject != null;
    }
    
    private void OnThrow()
    {
        if (HasWorkshopObject())
        {
            WorkshopObject obj = GetWorkshopObject();
            ClearWorkshopObject();

            obj.transform.parent = null;
            obj.SetWorkshopObjectParent(null);

            Rigidbody objRb = obj.GetComponent<Rigidbody>();
            Collider objCol = obj.GetComponent<Collider>();

            if (objRb != null)
            {
                objRb.constraints &= ~RigidbodyConstraints.FreezePositionX;
                objRb.constraints &= ~RigidbodyConstraints.FreezePositionY;
                objRb.constraints &= ~RigidbodyConstraints.FreezePositionZ;

                Vector3 throwDirection = transform.forward + Vector3.up * 0.2f;
                objRb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
            }

            if (objCol != null)
            {
                objCol.enabled = true;
            }

            obj.gameObject.layer = LayerMask.NameToLayer("Resource");

            obj.SetCanBePickedUp(false);
            StartCoroutine(ReenablePickup(obj, 0.5f));

            // Add FreeWorkshopObject to make it interactable
            if (!obj.TryGetComponent(out FreeWorkshopObject fwc))
            {
                fwc = obj.gameObject.AddComponent<FreeWorkshopObject>();
            }
            fwc.Initialize(obj);
        }
    }


    private IEnumerator ReenablePickup(WorkshopObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetCanBePickedUp(true);
    }

}
