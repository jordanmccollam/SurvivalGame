using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockMonster : MonoBehaviour
{
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

    [Header("Other")]
    Rigidbody2D rb;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        checkingGround = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer);
        checkingWall = Physics2D.OverlapCircle(wallCheckPoint.position, checkRadius, groundLayer);
        Patrol();
    }

    void Patrol() {
        if (!checkingGround || checkingWall) {
            Flip();
        }
        rb.velocity = new Vector2(speed * moveDir, rb.velocity.y);
    }

    void Flip() {
        moveDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        Gizmos.DrawWireSphere(wallCheckPoint.position, checkRadius);
    }
}
