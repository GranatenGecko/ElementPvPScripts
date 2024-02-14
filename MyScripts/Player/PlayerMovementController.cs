using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    // Animation Hashes
    private int horizontalHash;
    private int verticalHash;
    private int isCrouchHash;
    private int isJumpHash;
    private int isRunHash;
    private int isMovingForwardHash;
    private int isMovingBackwardHash;
    private int isMovingLeftHash;
    private int isMovingRightHash;

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
    private enum MovementState{idleStand, walkingForward, walkingBackward,walkingLeft, walkingRight, sprinting, idleCrouch, crouchWalkForward, crouchWalkBackward, crouchWalkLeft, crouchWalkRight, air}

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
        isRunHash = Animator.StringToHash("isRun");
        isMovingForwardHash = Animator.StringToHash("isMovingForward");
        isMovingBackwardHash = Animator.StringToHash("isMovingBackward");
        isMovingLeftHash = Animator.StringToHash("isMovingLeft");
        isMovingRightHash = Animator.StringToHash("isMovingRight");

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

        // Jump
        if (Input.GetKey(jumpKey) && isPlayerReadyToJump && isPlayerGrounded)
        {
            isPlayerReadyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start Crouch
        if (Input.GetKeyDown(crouchKey) && isPlayerGrounded && !Input.GetKey(sprintKey))
        {
            playerColliderStand.enabled = false;
            playerColliderCrouch.enabled = true;
            isPlayerCrouched = true;
        }

        // Stop Crouch
        if (Input.GetKeyUp(crouchKey) && !isPlayerCrouched && isPlayerGrounded)
        {
            playerColliderStand.enabled = true;
            playerColliderCrouch.enabled = false;
            isPlayerCrouched = false;
        }
    }

    private void SetAnimatorMovementForward()
    {
        playerAnimator.SetBool(isMovingForwardHash, true);
        playerAnimator.SetBool(isMovingBackwardHash, false);
        playerAnimator.SetBool(isMovingLeftHash, false);
        playerAnimator.SetBool(isMovingRightHash, false);
    }

    private void SetAnimatorMovementBackward()
    {
        playerAnimator.SetBool(isMovingForwardHash, false);
        playerAnimator.SetBool(isMovingBackwardHash, true);
        playerAnimator.SetBool(isMovingLeftHash, false);
        playerAnimator.SetBool(isMovingRightHash, false);
        playerAnimator.SetBool(isRunHash, false);
    }

    private void SetAnimatorMovementLeft()
    {
        playerAnimator.SetBool(isMovingForwardHash, false);
        playerAnimator.SetBool(isMovingBackwardHash, false);
        playerAnimator.SetBool(isMovingLeftHash, true);
        playerAnimator.SetBool(isMovingRightHash, false);
        playerAnimator.SetBool(isRunHash, false);
    }

    private void SetAnimatorMovementRight()
    {
        playerAnimator.SetBool(isMovingForwardHash, false);
        playerAnimator.SetBool(isMovingBackwardHash, false);
        playerAnimator.SetBool(isMovingLeftHash, false);
        playerAnimator.SetBool(isMovingRightHash, true);
        playerAnimator.SetBool(isRunHash, false);
    }

    private void SetAnimatorMovementIdle()
    {
        playerAnimator.SetBool(isMovingForwardHash, false);
        playerAnimator.SetBool(isMovingBackwardHash, false);
        playerAnimator.SetBool(isMovingLeftHash, false);
        playerAnimator.SetBool(isMovingRightHash, false);
        playerAnimator.SetBool(isRunHash, false);
    }

    private void AnimatorStateHandler()
    {
        // Mode - Crouching
        if (isPlayerGrounded && Input.GetKey(crouchKey) && !Input.GetKey(sprintKey) || isPlayerCrouched)
        {
            playerAnimator.SetBool(isCrouchHash, true);
            if (Input.GetKey(forwardKey))
            {
                movementState = MovementState.crouchWalkForward;
                SetAnimatorMovementForward();
            }

            else if (Input.GetKey(backwardKey))
            {
                movementState = MovementState.crouchWalkBackward;
                SetAnimatorMovementBackward();
            }

            else if (Input.GetKey(leftKey))
            {
                movementState = MovementState.crouchWalkLeft;
                SetAnimatorMovementLeft();
            }

            else if (Input.GetKey(rightKey))
            {
                movementState = MovementState.crouchWalkRight;
                SetAnimatorMovementRight();
            }

            else
            {
                movementState = MovementState.idleCrouch;
                SetAnimatorMovementIdle();
            }
            moveSpeed = crouchSpeed;
            playerAnimator.SetBool(isCrouchHash, true);
        }

        // Mode - Walking
        else if (isPlayerGrounded && !Input.GetKey(crouchKey) && !isPlayerCrouched!)
        {
            playerAnimator.SetBool(isCrouchHash, false);
            if (Input.GetKey(forwardKey))
            {
                
                if (!Input.GetKey(sprintKey) && !isPlayerCrouched)
                {
                    moveSpeed = walkSpeed;
                    movementState = MovementState.walkingForward;
                    SetAnimatorMovementForward();
                    playerAnimator.SetBool(isRunHash, false);
                }
                else
                {
                    moveSpeed = sprintSpeed;
                    movementState = MovementState.sprinting;
                    playerAnimator.SetBool(isMovingForwardHash, true);
                    playerAnimator.SetBool(isRunHash, true);
                }
            }

            else if (Input.GetKey(backwardKey))
            {
                moveSpeed = walkSpeed;
                movementState = MovementState.walkingBackward;
                SetAnimatorMovementBackward();
            }

            else if (Input.GetKey(leftKey))
            {
                moveSpeed = walkSpeed;
                movementState = MovementState.walkingLeft;
                SetAnimatorMovementLeft();
            }

            else if (Input.GetKey(rightKey))
            {
                moveSpeed = walkSpeed;
                movementState = MovementState.walkingRight;
                SetAnimatorMovementRight();
            }

            else
            {
                movementState = MovementState.idleStand;
                SetAnimatorMovementIdle();
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
