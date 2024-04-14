using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private readonly float rotationSpeed = 5f;
    private readonly float defaultSpeed = 3f;
    private readonly float runningMultiplier = 1.8f;
    private Animator animator;
    private bool isGrounded = true;
    private CharacterController character;
    private Vector3 moveDirection;
    private Vector3 velocity;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        character = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!GameManager.Instance.dialogueManager.isDialogueOpen)
        {
            Move();
            ApplyGravity();
            ApplyJump();
            return;
        }
        StopMovementAnimation();
    }

    private void StopMovementAnimation ()
    {
        animator.SetBool("isMoving", false);
        animator.SetBool("isRunning", false);
    }

    void Move()
    {
        StopMovementAnimation();
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? defaultSpeed * runningMultiplier : defaultSpeed;

        moveDirection = (Camera.main.transform.forward * verticalInput + Camera.main.transform.right * horizontalInput).normalized;
        moveDirection.y = 0;
        character.Move(moveSpeed * Time.deltaTime * moveDirection);

        if (moveDirection != Vector3.zero)
        {
            animator.SetBool("isMoving", true);
            if (moveSpeed > defaultSpeed)
            {
                animator.SetBool("isRunning", true);
            }
            ApplyRotation();
        }
    }

    void ApplyGravity()
    {
        velocity.y += Physics.gravity.y * Time.deltaTime;
        character.Move(velocity * Time.deltaTime);
        isGrounded = character.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            animator.SetBool("isJumping", false);
            velocity.y = -1f;
        }
    }

    void ApplyJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            animator.SetBool("isJumping", true);
            velocity.y = 5f;
        }
    }

    void ApplyRotation ()
    {
        Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
    }

}
