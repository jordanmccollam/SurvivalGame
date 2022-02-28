using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockMonster : MonoBehaviour
{
    [Header("Health")]
    public int health;

    [Header("For patrol")]
    public float speed;
    public float checkRadius;
    public Transform groundCheckPoint;
    public Transform wallCheckPoint;
    public LayerMask groundLayer;
    public float moveDir = 1;
    bool facingRight = true;
    bool checkingGround;
    bool checkingWall;

    [Header("For Jump Attack")]
    public int damage;
    public float jumpHeight;
    Transform player;
    Player playerComp;
    public Transform groundCheck;
    public Vector2 boxSize;
    public float timeBetweenAttacks;
    public ParticleSystem rockDust;
    bool isGrounded;
    bool canAttack = true;

    [Header("For Seeing Player")]
    public Vector2 lineOfSight;
    public LayerMask playerLayer;
    bool canSeePlayer;

    [Header("Other")]
    Rigidbody2D rb;
    Animator anim;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update() {
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null) {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            playerComp = player.GetComponent<Player>();
        }
    }

    private void FixedUpdate() {
        if (player == null) return;

        bool wasGrounded = isGrounded;

        checkingGround = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer);
        checkingWall = Physics2D.OverlapCircle(wallCheckPoint.position, checkRadius, groundLayer);
        isGrounded = Physics2D.OverlapBox(groundCheck.position, boxSize, 0, groundLayer);

        if (playerComp.isSneaking) {
            Vector2 offset = new Vector2(transform.position.x + (lineOfSight.x / 4), transform.position.y);
            Vector2 _lineOfSight = new Vector2(lineOfSight.x / 2, lineOfSight.y);
            canSeePlayer = Physics2D.OverlapBox(offset, _lineOfSight, 0, playerLayer);
        } else {
            canSeePlayer = Physics2D.OverlapBox(transform.position, lineOfSight, 0, playerLayer);
        }

        if (isGrounded && !wasGrounded) {
            anim.SetBool("isJumping", false);
            Invoke("ReadyAttack", timeBetweenAttacks);
        }
        if (!canSeePlayer && isGrounded) {
            Patrol();
        }
        if (canSeePlayer && isGrounded) {
            FlipTowardsPlayer();
            JumpAttack();
        }
    }

    void Patrol() {
        if (!checkingGround || checkingWall) {
            Flip();
        }
        rb.velocity = new Vector2(speed * moveDir, rb.velocity.y);
    }

    void JumpAttack() {
        if (player == null || !canAttack) return;

        canAttack = false;
        anim.SetBool("isJumping", true);

        float distanceFromPlayer = player.position.x - transform.position.x;

        if (isGrounded) {
            rb.AddForce(new Vector2(distanceFromPlayer, jumpHeight), ForceMode2D.Impulse);
        }
    }

    void ReadyAttack() {
        canAttack = true;
    }
    
    void FlipTowardsPlayer() {
        if (player == null || !canSeePlayer) return;

        float playerPosition = player.position.x - transform.position.x;
        if (playerPosition < 0 && facingRight) {
            Flip();
        } else if (playerPosition > 0 && !facingRight) {
            Flip();
        }
    }

    void Flip() {
        moveDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    void Slam() {
        rockDust.Play();
        // TODO: Make slame noise here
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player") {
            Player playerComp = other.gameObject.GetComponent<Player>();
            if (!playerComp.isStunned) {
                playerComp.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int amount) {
        health -= amount;
        if (health <= 0) {
            Die();
        }
    }

    void Die() {
        // TODO: Add death effect
        // TODO: Add death sound
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected() {
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
