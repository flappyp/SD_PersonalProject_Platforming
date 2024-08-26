using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; //lets the enemy konw where the player is 
    public float chaseSpeed = 2f; //the speed the enemy moves towards the player
    public float jumpForce = 2f; //the force that the enemy will jump
    public LayerMask groundLayer; // a layer mask used to determine what the ground is 

    private Rigidbody2D rb; // reference to the enemy's rigid body 2D which is used to control the enemy movement
    private bool isGrounded; // a bool to check if the enemy is grounded
    private bool shouldJump; // a bool to determine if the enemy should jump

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //called once when the game starts and gets the reference to the rigid body 2D component attached to the enemy
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer); // uses a ray cast to check if there is ground directly beneath the enemy this prevents the enemy from jumping in the air

        float direction = Mathf.Sign(player.position.x - transform.position.x); // calculates the direction towards the player, if the player is to the right the direction will be positive and the left will be negative

        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 2f, 1 << player.gameObject.layer); // uses ray cast to check if the player is directly above the enemy and within a certain distance, will help the enemy decide if the enemy should jump onto the platform above it.

        if (isGrounded)
        {
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y); // if the enemy is grounded it will move horizontally towards the player at the chase speed set in the inspector

            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer); // checks if there is ground directly in front of the enemy

            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer); // checks if there is a gap directly ahead of the enemy 

            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, groundLayer); // checks if there is platform above the enemy

            if (!groundInFront.collider && !gapAhead.collider) // should jump is true if there is a gap ahead or no ground in from and the player is above
            {
                shouldJump = true;
            }
            else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate() //this method is used for physics related updates such as applying forces, it runs at a consistent intervals
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;

            Vector2 direction = (player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpDirection.y), ForceMode2D.Impulse);
        }
    }
}
