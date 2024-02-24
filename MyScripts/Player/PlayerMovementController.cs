using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    // Animation Hashes
    private int horizontalHash;
    private int verticalHash;
    private int isCrouchHash;
    private int isJumpHash;
    private int moveXHash;
    private int moveZHash;

    // Keybinds
    private KeyCode jumpKey = KeyCode.Space;
    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode crouchKey = KeyCode.LeftControl;
    private KeyCode forwardKey = KeyCode.W;
    private KeyCode leftKey = KeyCode.A;
    private KeyCode rightKey = KeyCode.D;
    private KeyCode backwardKey = KeyCode.S;

    // Movement Inputs
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    // Movement Speed Settings
    private float moveSpeed = 2.0f;
    private float walkSpeed = 2.0f;
    private float sprintSpeed = 7.0f;
    private float groundDrag = 5.0f;

    //Movement States
    [Header("States")][SerializeField] private MovementState movementState;
    private enum MovementState{idleStand, walkingForwardDiagonalLeft, walkingForward, walkingForwardDiagonalRight, walkingBackwardDiagonalLeft, walkingBackward, 
        walkingBackwardDiagonalRight,walkingLeft, walkingRight, sprinting, idleCrouch, crouchWalkForwardDiagonalLeft, crouchWalkForwardDiagonalRight, 
        crouchWalkForward, crouchWalkBackwardDiagonalLeft, crouchWalkBackward, crouchWalkBackwardDiagonalRight, crouchWalkLeft, crouchWalkRight, air}

    // Ground Check
    private float playerHeight = 1.76f;
    private float playerHeightCrouch = 1.36f;
    [SerializeField] private bool isPlayerGrounded; // Ground Check

    // Slope Movement
    private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool isExitingSlope;

    // Jump
    private float jumpForce = 12.0f;
    private float jumpCooldown = 0.25f;
    private float airMultiplier = 0.4f;
    [SerializeField] private bool isPlayerReadyToJump;

    // Crouching
    private float crouchSpeed = 1.0f;
    private float startYPosition; // Player Height Scale
    [SerializeField] private bool isPlayerCrouched;

    // Reference Objects and Components
    [Header("References")]
    public Transform playerCharacter;
    public Animator playerAnimator;
    public CapsuleCollider playerColliderStand;
    public CapsuleCollider playerColliderCrouch;
    private Rigidbody playerRigidbody;

    // Collision Layers
    private LayerMask ground;
    private LayerMask defaultObjects;
    
    // Test
    private float playerRadius;
    private Color debugColor;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        ground = LayerMask.GetMask("Ground");
        defaultObjects = LayerMask.GetMask("Default");
        isCrouchHash = Animator.StringToHash("isCrouch");
        isJumpHash = Animator.StringToHash("isJump");
        moveXHash = Animator.StringToHash("moveX");
        moveZHash = Animator.StringToHash("moveZ");
        playerRadius = (playerHeight - playerHeightCrouch) * 1.1f;
    }

    void OnDrawGizmos()
    {
        if(isPlayerCrouched) 
        {
            Gizmos.color = debugColor;
            Gizmos.DrawSphere(transform.position + transform.up * playerHeightCrouch * 0.5f, playerRadius);
        }
    }

    private void ObjectAboveCrouchPlayerCheck()
    {
        if (isPlayerCrouched)
        {
            RaycastHit sphereRaycastHit;
            if (Physics.SphereCast(transform.position, playerRadius, transform.up, out sphereRaycastHit, playerHeight, defaultObjects))
            {
                isPlayerCrouched = true;
                debugColor = Color.green;
            }
            else
            {
                if (!Input.GetKey(crouchKey))
                {
                    playerColliderStand.enabled = true;
                    playerColliderCrouch.enabled = false;
                    playerAnimator.SetBool(isCrouchHash, false);
                    isPlayerCrouched = false;
                }
                debugColor = Color.red;
            }
        }
    }

    private void PlayerInAirCheck()
    {
        RaycastHit grounCheckRayCast;
        if (Physics.Raycast(transform.position, Vector3.down, out grounCheckRayCast, playerHeight * 0.5f + 0.2f, ground))
        {
            Debug.DrawRay(transform.position, Vector3.down * grounCheckRayCast.distance, Color.green);
            isPlayerGrounded = true;
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), Color.red);
            isPlayerGrounded = false;
        }
    }

    private void PlayerGroundedCheck()
    {
        if (isPlayerGrounded)
        {
            playerRigidbody.drag = groundDrag;
        }
        else
        {
            playerRigidbody.drag = 0;
        }
    }

    private void Update()
    {
        PlayerInAirCheck();
        ObjectAboveCrouchPlayerCheck();
        MyInput();
        LimitSpeed();
        AnimatorStateHandler();
        PlayerGroundedCheck();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Input - Jump
        if (Input.GetKey(jumpKey) && isPlayerReadyToJump && isPlayerGrounded)
        {
            isPlayerReadyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Input - Start Crouch
        if (Input.GetKeyDown(crouchKey) && isPlayerGrounded && !Input.GetKey(sprintKey))
        {
            playerColliderStand.enabled = false;
            playerColliderCrouch.enabled = true;
            isPlayerCrouched = true;
        }

        // Input - Stop Crouch
        if (Input.GetKeyUp(crouchKey) && !isPlayerCrouched && isPlayerGrounded)
        {
            playerColliderStand.enabled = true;
            playerColliderCrouch.enabled = false;
            isPlayerCrouched = false;
        }
    }

    private void SetAnimatorMovement(int x, int y)
    {
        playerAnimator.SetFloat(moveXHash, x);
        playerAnimator.SetFloat(moveZHash, y);
    }

    private void AnimatorStateHandler()
    {
        // Mode - Crouching
        if (isPlayerGrounded && Input.GetKey(crouchKey) && !Input.GetKey(sprintKey) || isPlayerCrouched)
        {
            playerAnimator.SetBool(isCrouchHash, true);
            if (Input.GetKey(forwardKey))
            {
                // Mode - Crouching - Forward - Diagonal - Left
                if (Input.GetKey(leftKey))
                {
                    movementState = MovementState.crouchWalkForwardDiagonalLeft;
                    SetAnimatorMovement(1, -1);
                }
                // Mode - Crouching - Forward - Diagonal - Right
                else if (Input.GetKey(rightKey))
                {
                    movementState = MovementState.crouchWalkForwardDiagonalRight;
                    SetAnimatorMovement(1, 1);
                }
                // Mode - Crouching - Forward
                else
                {
                    movementState = MovementState.crouchWalkForward;
                    SetAnimatorMovement(1, 0);
                }
            }

            else if (Input.GetKey(backwardKey))
            {
                // Mode - Crouching - Backward - Diagonal - Left
                if (Input.GetKey(leftKey))
                {
                    movementState = MovementState.crouchWalkBackwardDiagonalLeft;
                    SetAnimatorMovement(-1, -1);
                }
                // Mode - Crouching - Backward - Diagonal - Right
                else if (Input.GetKey(rightKey))
                {
                    movementState = MovementState.crouchWalkBackwardDiagonalRight;
                    SetAnimatorMovement(1, -1);
                }
                // Mode - Crouching - Backward
                else
                {
                    movementState = MovementState.crouchWalkBackward;
                    SetAnimatorMovement(0, -1);
                }
            }

            // Mode - Crouching - Walk - Left
            else if (Input.GetKey(leftKey))
            {
                movementState = MovementState.crouchWalkLeft;
                SetAnimatorMovement(-1, 0);
            }

            // Mode - Crouching - Walk - Left
            else if (Input.GetKey(rightKey))
            {
                movementState = MovementState.crouchWalkRight;
                SetAnimatorMovement(1, 0);
            }

            // Mode - Crouching - Idle
            else
            {
                movementState = MovementState.idleCrouch;
                SetAnimatorMovement(0, 0);
            }
            moveSpeed = crouchSpeed;
            playerAnimator.SetBool(isCrouchHash, true);
        }

        // Mode - Walking & Running
        else if (isPlayerGrounded && !Input.GetKey(crouchKey) && !isPlayerCrouched!)
        {
            playerAnimator.SetBool(isCrouchHash, false);
            if (Input.GetKey(forwardKey))
            {
                if (!Input.GetKey(sprintKey) && !isPlayerCrouched)
                {
                    moveSpeed = walkSpeed;
                    // Mode - Walking - Forward - Left
                    if (Input.GetKey(leftKey))
                    {
                        movementState = MovementState.walkingForwardDiagonalLeft;
                        SetAnimatorMovement(-1, 1);
                    }
                    // Mode - Walking - Forward -Right
                    else if (Input.GetKey(rightKey))
                    {
                        movementState = MovementState.walkingForwardDiagonalRight;
                        SetAnimatorMovement(1, 1);
                    }
                    // Mode - Walking - Forward
                    else
                    {
                        movementState = MovementState.walkingForward;
                        SetAnimatorMovement(0, 1);
                    }
                }

                else
                {
                    moveSpeed = sprintSpeed;
                    // Mode - Run - Forward - Left
                    if (Input.GetKey(leftKey))
                    {
                        SetAnimatorMovement(-1, 2);
                    }
                    // Mode - Run - Forward - Right
                    else if (Input.GetKey(rightKey))
                    {
                        SetAnimatorMovement(1, 2);
                    }
                    // Mode - Run - Forward
                    else
                    {
                        movementState = MovementState.sprinting;
                        SetAnimatorMovement(0, 2);
                    }
                }
            }

            else if (Input.GetKey(backwardKey))
            {
                // Mode - Walking - Backward - Left
                moveSpeed = walkSpeed;
                if (Input.GetKey(leftKey))
                {
                    movementState = MovementState.walkingBackwardDiagonalLeft;
                    SetAnimatorMovement(-1, -1);
                }
                // Mode - Walking - Backward - Right
                else if (Input.GetKey(rightKey))
                {
                    movementState = MovementState.walkingBackwardDiagonalRight;
                    SetAnimatorMovement(1, -1);
                }
                // Mode - Walking - Backward
                else
                {
                    movementState = MovementState.walkingBackward;
                    SetAnimatorMovement(0, -1);
                }
            }

            // Mode - Walking - Left
            else if (Input.GetKey(leftKey))
            {
                moveSpeed = walkSpeed;
                movementState = MovementState.walkingLeft;
                SetAnimatorMovement(-1, 0);
            }

            // Mode - Walking - Right
            else if (Input.GetKey(rightKey))
            {
                moveSpeed = walkSpeed;
                movementState = MovementState.walkingRight;
                SetAnimatorMovement(1, 0);
            }

            // Mode - Stand - Idle
            else
            {
                movementState = MovementState.idleStand;
                SetAnimatorMovement(0, 0);
            }
            playerAnimator.SetBool(isJumpHash, false);
        }

        // Mode - Air
        else if(!isPlayerGrounded)
        {
            movementState = MovementState.air;
            playerAnimator.SetBool(isJumpHash, true);
        }
    }

    private void MovePlayer()
    {
        moveDirection = playerCharacter.forward * verticalInput + playerCharacter.right * horizontalInput;

        // On Slope
        if (OnSlope() && !isExitingSlope)
        {
            playerRigidbody.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if(playerRigidbody.velocity.y > 0)
            {
                playerRigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // On Ground
        if (isPlayerGrounded)
        {
            playerRigidbody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        
        //In Air
        else if (!isPlayerGrounded)
        {
            playerRigidbody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Turn gravity off while on slope
        playerRigidbody.useGravity = !OnSlope();
    }

    private void LimitSpeed()
    {
        // Limiting speed on slope
        if (OnSlope() && !isExitingSlope)
        {
            if(playerRigidbody.velocity.magnitude > moveSpeed)
            {
                playerRigidbody.velocity = playerRigidbody.velocity.normalized * moveSpeed;
            }
        }

        // Limiting speed on ground or in air
        else
        {
            Vector3 flatVelocity = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                playerRigidbody.velocity = new Vector3(limitedVelocity.x, playerRigidbody.velocity.y, limitedVelocity.z);
            }
        }
    }

    private void Jump()
    {
        isExitingSlope = true;
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);
        playerRigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        isPlayerReadyToJump = true;
        isExitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
