using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    bool isFacingRight = true;
    public ParticleSystem smokeFX;
    public float moveSpeed = 5f;
    public float jumpPower = 10f;
    public int maxJumps = 2;
    int jumpsRemaining;
    private float horizontalMovement;
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    bool isWallSliding;
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f); // Fixed colon to semicolon

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>(); // Assign Rigidbody2D if not set in Inspector
        }

        jumpsRemaining = maxJumps;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        GroundCheck();
        Gravity();
        ProcessWallSlide();
        ProcessWallJump();

        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
            Flip();
        }
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetFloat("magnitude", rb.velocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    private void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void MovePlayer()
    {
        rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0 && context.performed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            jumpsRemaining--;
            JumpFX(); // Close the if block here
        }
        else if (context.canceled)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            JumpFX();
        }

        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;
            JumpFX();
            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);
        }
    }

    private void JumpFX()
        {
            animator.SetTrigger("jump");
            smokeFX.Play();

        }
        private void GroundCheck()
        {
            if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
            {
                jumpsRemaining = maxJumps;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }

        private bool WallCheck()
        {
            return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer) != null;
        }

        private void ProcessWallSlide()
        {
            if (!isGrounded && WallCheck() && horizontalMovement != 0) // Fixed & to &&
            {
                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
            }
            else
            {
                isWallSliding = false;
            }
        }

        private void ProcessWallJump()
        {
            if (isWallSliding)
            {
                isWallJumping = false;
                wallJumpDirection = -transform.localScale.x;
                wallJumpTimer = wallJumpTime;

                CancelInvoke(nameof(CancelWallJump));
            }
            else if (wallJumpTimer > 0f)
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }

        private void CancelWallJump()
        {
            isWallJumping = false;
        }

        private void Flip()
        {
            if ((isFacingRight && horizontalMovement < 0) || (!isFacingRight && horizontalMovement > 0))
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
                if (rb.velocity.y == 0)
                {
                    smokeFX.Play();

                }

            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
        }
    }


