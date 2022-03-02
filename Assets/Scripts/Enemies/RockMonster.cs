using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockMonster : Enemy
{
    [Header("For Jump Attack")]
    public int damage;
    public float jumpHeight;
    public float timeBetweenAttacks;
    public ParticleSystem rockDust;
    bool canAttack = true;

    private void Start() {
        base.Start();
    }

    private void FixedUpdate() {
        base.FixedUpdate();

        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, boxSize, 0, groundLayer);

        if (isStunned) return;

        if (playerComp != null && playerComp.isSneaking) {
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
            base.Patrol();
        }
        if (canSeePlayer && isGrounded) {
            base.FlipTowardsPlayer();
            JumpAttack();
        }
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
}
