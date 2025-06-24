using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetWalk : MonoBehaviour
{
    public float speed = 2f;
    public float maxWalkTime = 6f;
    public float idleTime = 3f;
    public float maxDistanceFromCenter = 10f; // Maximum horizontal distance from the origin (center of the viewport)

    private float timer;
    private int direction = 1;
    private bool isWalking = false;

    private Animator animator;

    // Store the initial starting X position to constrain movement
    private float initialXPosition;

    private void Start()
    {
        timer = -1f;
        animator = GetComponent<Animator>();
        initialXPosition = transform.position.x; // Record the initial X position
    }

    private void Update()
    {
        if (timer < 0f) timer = Random.Range(1f, maxWalkTime);

        if (isWalking)
        {
            // Calculate the next position
            float nextXPosition = transform.position.x + (direction * speed * Time.deltaTime);

            // Check if the next position is within the allowed bounds
            if (Mathf.Abs(nextXPosition - initialXPosition) > maxDistanceFromCenter)
            {
                // If it goes out of bounds, reverse direction immediately
                ChangeDirection();
                // Recalculate nextXPosition with the new direction
                nextXPosition = transform.position.x + (direction * speed * Time.deltaTime);
            }

            // Move the animal
            transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isWalking = false;
                timer = idleTime;
                animator.SetBool("isWalking", false); // Switch to idle animation
            }
        }
        else
        {
            // Idle phase
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isWalking = true;
                timer = -1f; // Resets timer to get a new random walk time

                ChangeDirection(); // Change direction when starting to walk again
                animator.SetBool("isWalking", true);
            }
        }
    }

    /// <summary>
    /// Changes the horizontal direction of the GameObject and flips its local scale.
    /// </summary>
    private void ChangeDirection()
    {
        // Randomly choose between -1 and 1 for direction
        int newDir = Random.Range(0, 2) * 2 - 1; // Generates either -1 or 1

        // Only change direction if it's different from the current one,
        // or if we're forcing a change due to hitting a boundary
        if (newDir != direction)
        {
            direction = newDir;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction; // Ensure scale.x is positive or negative based on direction
            transform.localScale = scale;
        }
    }
}