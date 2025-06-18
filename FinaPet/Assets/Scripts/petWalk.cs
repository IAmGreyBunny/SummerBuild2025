using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Random;

public class DogWalk : MonoBehaviour
{
    public float speed = 2f;
    public float maxWalkTime = 6f;
    public float idleTime = 3f;

    private float timer;
    private int direction = 1;
    private bool isWalking = false;

    private Animator animator;

    private void Start()
    {
        timer = -1f;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(timer < 0f) timer = Random.Range(1f, maxWalkTime);

        if (isWalking)
        {
            // Move the animal
            transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isWalking = false;
                timer = idleTime;
                animator.SetBool("isWalking", false);  // Switch to idle animation
            }
        }
        else
        {
            // Idle phase
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isWalking = true;
                //timer = walkTime;
                timer = -1f;

                //Random r = new Random();
                var values = new[] { -1, 1 };
                int dirChange = values[Random.Range(0,values.Length)];
                // Flip direction
                Vector3 scale = transform.localScale;
                direction *= dirChange;
                scale.x *= dirChange;
                transform.localScale = scale;
                animator.SetBool("isWalking", true);
            }
        }
    }
}