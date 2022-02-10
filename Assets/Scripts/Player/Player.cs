using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Animations:")]
    public float blinkTime;

    [Header("Stats")]
    public int health;
    public float speed;
    public float jumpForce;
    public float jumpTime;

    Vector2 input;
    float jumpTimeCounter;

    // CHECKS ---
    bool isJumping = false;
    bool facingRight = true;
    bool isGrounded = false;
    // ----------

    // COMPONENTS ---
    Animator anim;
    Rigidbody2D rb;
    Health healthUI;
    // --------------

    [Header("Mechanics")]
    public Transform groundCheck;
    public LayerMask whatIsGround;
    public float checkRadius;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthUI = GameObject.FindGameObjectWithTag("PlayerUI").GetComponent<Health>();
        healthUI.UpdateUI(health);

        InvokeRepeating("Blink", blinkTime, blinkTime);
    }

    void Blink() {
        anim.SetTrigger("blink");
    }

    public void OnInput(InputAction.CallbackContext context) {
        input = context.ReadValue<Vector2>();
    }

    private void Update() {
        // If holding jump, go higher
        if (isJumping) {
            if (jumpTimeCounter > 0) {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            } else {
                // When jump time runs out, stop going higher
                isJumping = false;
            }
        }
    }

    private void FixedUpdate() {
        Move();

        // Check if grounded and animate accordingly
        bool _isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        if (isGrounded && !_isGrounded) {
            anim.SetBool("isJumping", true);
        }
        else if (!isGrounded && _isGrounded) {
            anim.SetBool("isJumping", false);
        }
        isGrounded = _isGrounded;
        
    }

    void Move() {
        // Move horizontally only
        rb.velocity = new Vector2(Mathf.Round(input.x) * speed, rb.velocity.y);

        // if moving, use run anim
        if (input.x == 0) { 
            anim.SetBool("isRunning", false);
        } else {
            anim.SetBool("isRunning", true);
        }

        // Flip sprite if necessary
        if (input.x > 0 && !facingRight) {
            Flip();
        } else if (input.x < 0 && facingRight) {
            Flip();
        }
    }

    void Flip() {
        if (facingRight) {
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, 180f, transform.localRotation.z);
        } else {
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, 0, transform.localRotation.z);
        }
        facingRight = !facingRight;
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.started && isGrounded) { // On button down, jump
            isJumping = true;
            jumpTimeCounter = jumpTime;
            anim.SetTrigger("takeOff");
        }

        if (context.canceled) { // If button released, stop jumping
            isJumping = false;
        }
    }

    public void TakeDamage(int damage) {
        health -= damage;
        healthUI.UpdateUI(health);
    }
}
