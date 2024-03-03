using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    private float rotationSpeed = 10f;
    private Animator animator;
    private float defaultSpeed = 5f;
    private float runningMultiplier = 2f;
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
        Move();
        ApplyGravity();
        ApplyJump();
    }

    void Move()
    {
        animator.SetBool("isMoving", false);
        animator.SetBool("isRunning", false);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? defaultSpeed * runningMultiplier : defaultSpeed;

        moveDirection = (Camera.main.transform.forward * verticalInput + Camera.main.transform.right * horizontalInput).normalized;
        moveDirection.y = 0;
        character.Move(moveDirection * moveSpeed * Time.deltaTime);

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
