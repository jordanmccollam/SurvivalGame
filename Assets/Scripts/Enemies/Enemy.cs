using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Basic Stats")]
    public int health;
    public float speed;

    [Header("For Patrol")]
    public float checkRadius;
    public Transform groundCheckPoint;
    public Transform wallCheckPoint;
    public LayerMask groundLayer;
    public float moveDir = 1;
    [HideInInspector] public bool facingRight = true;
    [HideInInspector] public bool checkingGround;
    [HideInInspector] public bool checkingWall;

    [Header("For Jumping")]
    public Transform groundCheck;
    public Vector2 boxSize;
    [HideInInspector] public bool isGrounded;

    [Header("For Seeing Player")]
    public Vector2 lineOfSight;
    public LayerMask playerLayer;
    [HideInInspector] public bool canSeePlayer;
    [HideInInspector] public Transform player;
    [HideInInspector] public Player playerComp;


    // COMPONENTS---
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator anim;
    // -------------

    [Header("For Taking Damage")]
    public ParticleSystem stunEffect;
    public float stunCooldown;
    [HideInInspector] public bool isStunned = false;

    public void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void Update() {
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null) {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            playerComp = player.GetComponent<Player>();
        }
    }

    public void FixedUpdate() {
        if (player == null || isStunned) return;

        checkingGround = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer);
        checkingWall = Physics2D.OverlapCircle(wallCheckPoint.position, checkRadius, groundLayer);
    }

    public void Patrol() {
        if (!checkingGround || checkingWall) {
            Flip();
        }
        rb.velocity = new Vector2(speed * moveDir, rb.velocity.y);
    }

    public void Flip() {
        moveDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public void FlipTowardsPlayer() {
        if (player == null || !canSeePlayer) return;

        float playerPosition = player.position.x - transform.position.x;
        if (playerPosition < 0 && facingRight) {
            Flip();
        } else if (playerPosition > 0 && !facingRight) {
            Flip();
        }
    }

    public void TakeDamage(int amount) {
        if (isStunned) return;

        health -= amount;

        // TODO: Add take damage sound effect

        // Stun enemy
        isStunned = true;
        stunEffect.Play();
        Invoke("StunCooldown", stunCooldown);

        if (health <= 0) {
            Die();
        }
    }

    void StunCooldown() {
        stunEffect.Stop();
        isStunned = false;
    }

    void Die() {
        // TODO: Add death effect
        // TODO: Add death sound
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        Gizmos.DrawWireSphere(wallCheckPoint.position, checkRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(groundCheck.position, boxSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, lineOfSight); // Regular line of sight

        Gizmos.color = Color.yellow;
        Vector2 offset = new Vector2(transform.position.x + (lineOfSight.x / 4), transform.position.y);
        Vector2 _lineOfSight = new Vector2(lineOfSight.x / 2, lineOfSight.y);
        Gizmos.DrawWireCube(offset, _lineOfSight);
    }
}
