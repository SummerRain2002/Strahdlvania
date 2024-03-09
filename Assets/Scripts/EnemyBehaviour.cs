using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBehaviour : MonoBehaviour
{
    public Vector3[] waypoints;
    public float moveSpeed = 2f;
    public float detectionDistance = 5f;
    public float fleeDistance = 3f; // Distance to flee from the player
    public float fleeDuration = 2f; // Duration to flee before switching back to chasing

    private Transform player;
    private int currentWaypoint = 0;
    private float fleeTimer = 0f; // Timer to track fleeing duration
    private BoxCollider2D box;
    private enum State { PATROL, DETECTED, FLEEING };
    private State currentState = State.PATROL;
    private SpriteRenderer sprite;
    private Vector3 previousPosition;

    void Start()
    {
        // Assuming the player is tagged as "Player". You can adjust this based on your setup.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        previousPosition = transform.position;
    }

    void Update()
    {
        UpdateState();
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case State.PATROL:
                Patrol();
                CheckForPlayerDetection();
                break;
            case State.DETECTED:
                ChasePlayer();
                CheckForCollisionWithPlayer();
                break;
            case State.FLEEING:
                FleeFromPlayer();
                break;
        }
        float horizontalDirection = transform.position.x - previousPosition.x;
        previousPosition = transform.position;
        FlipSprite(horizontalDirection);
    }

    void Patrol()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints defined for patrol!");
            return;
        }

        // Move towards the current waypoint
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint], moveSpeed * Time.deltaTime);

        // Check if the enemy has reached the current waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint]) < 0.1f)
        {
            Debug.Log("Reached waypoint " + currentWaypoint);

            // Move to the next waypoint
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    void CheckForPlayerDetection()
    {
        if (Vector3.Distance(transform.position, player.position) < detectionDistance)
        {
            currentState = State.DETECTED;
            Debug.Log("Player detected!");
        }
    }

    void ChasePlayer()
    {
        // Move towards the player
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    void CheckForCollisionWithPlayer()
    {
        // Assuming the enemy has a BoxCollider2D component
        if (box.IsTouching(player.GetComponent<Collider2D>()))
        {
            currentState = State.FLEEING;
            fleeTimer = 0f; // Reset the flee timer
            Debug.Log("Collided with player! Fleeing!");
        }
    }

    void FleeFromPlayer()
    {
        // Calculate direction away from the player
        Vector3 fleeDirection = transform.position - player.position;
        fleeDirection.Normalize();

        // Move away from the player
        transform.position += fleeDirection * moveSpeed * Time.deltaTime;

        // Increment flee timer
        fleeTimer += Time.deltaTime;

        // Check if the flee duration has elapsed
        if (fleeTimer >= fleeDuration)
        {
            currentState = State.DETECTED; // Switch back to chasing
            Debug.Log("Stopped fleeing. Back to chasing!");
        }
    }

    void FlipSprite(float horizontalDirection)
    {
        // Get the local scale of the sprite
        Vector3 localScale = transform.localScale;

        // Check if the horizontal direction is positive (moving right) or negative (moving left)
        if (horizontalDirection < 0)
        {
            // Set the local scale to its original value (no flip)
            localScale.x = Mathf.Abs(localScale.x);
        }
        else if (horizontalDirection > 0)
        {
            // Flip the sprite by setting the local scale along the X-axis to its negative value
            localScale.x = -Mathf.Abs(localScale.x);
        }

        // Apply the updated local scale to the sprite
        transform.localScale = localScale;
    }
}
