using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Animations:")]
    public float blinkTime;

    [Header("Stats")]
    public int health;
    public float speed;
    public float tiptoeSpeed;
    public float jumpForce;
    public float jumpTime;

    Vector2 input;
    float jumpTimeCounter;

    // CHECKS ---
    [HideInInspector] public bool isJumping = false;
    [HideInInspector] public bool canLedgeGrab = true;
    bool facingRight = true;
    bool isGrounded = false;
    bool isTiptoeing = false;
    bool isStunned = false;
    bool isRunning = false;
    bool isGrabbingLedge = false;
    // ----------

    // COMPONENTS ---
    [HideInInspector] public Animator anim;
    Animator camera;
    Rigidbody2D rb;
    Health healthUI;
    [HideInInspector] public AudioManager audio;
    // --------------

    // Fall damage ---
    bool wasFalling;
    float startOfFall;
    // ---------------

    [Header("Effects")]    
    public GameObject blood;

    [Header("Mechanics")]
    public Transform groundCheck;
    public Transform ledgeCheck;
    public LayerMask whatIsGround;
    public LayerMask whatIsLedge;
    public float checkRadius;
    public float minFallDistance;
    public int fallDamage;
    public Transform climbEndSpot;
    public float ledgeJumpCooldown;
    float baseGravity;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        healthUI = GameObject.FindGameObjectWithTag("PlayerUI").GetComponent<Health>();
        healthUI.AddHearts(health);
        baseGravity = rb.gravityScale;
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Animator>();

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
            if (jumpTimeCounter > 0 && !isStunned) {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            } else {
                // When jump time runs out, stop going higher
                isJumping = false;
            }
        }

        bool isTouchingLedge = Physics2D.OverlapCircle(ledgeCheck.position, checkRadius, whatIsLedge);
        if (isTouchingLedge && canLedgeGrab) {
            GrabLedge();
        }
    }

    public void GrabLedge() {
        if (canLedgeGrab) {
            canLedgeGrab = false;
            isGrabbingLedge = true;
            rb.gravityScale = 0;
            Stun();
            rb.velocity = Vector2.zero;
            anim.SetTrigger("grabLedge");
        }
    }

    public void LetGoOfLedge() {
        isGrabbingLedge = false;
        rb.gravityScale = baseGravity;
        ResetStun();
        Invoke("ResetLedgeGrab", ledgeJumpCooldown);
    }

    void ResetLedgeGrab() {
        canLedgeGrab = true;
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
            float fallDistance = startOfFall - transform.position.y;
            if (fallDistance > minFallDistance) {
                TakeDamage(fallDamage);
            }
        }
        isGrounded = _isGrounded;
        
        // Fall damage stuff
        if (!wasFalling && isFalling) {
            startOfFall = transform.position.y;
            audio.Stop("run");
            isRunning = false;
        }
    }

    void Move() {
        if (!isStunned) {
            // Move horizontally only
            rb.velocity = new Vector2(Mathf.Round(input.x) * (isTiptoeing ? tiptoeSpeed : speed), rb.velocity.y);

            // if moving, use run anim
            if (input.x == 0 && isRunning) { 
                isRunning = false;
                anim.SetBool("isRunning", false);
                anim.SetBool("isTiptoeing", false);
                audio.Stop("run");
            } else if (input.x != 0 && !isRunning) {
                isRunning = true;
                anim.SetBool("isRunning", true);
                if (isTiptoeing) anim.SetBool("isTiptoeing", true);
                audio.Loop("run");
            }

            // Flip sprite if necessary
            if (input.x > 0 && !facingRight) {
                Flip();
            } else if (input.x < 0 && facingRight) {
                Flip();
            }
        } else {
            // If stunned, set horizontal movement to 0 and stop running anims
            rb.velocity = new Vector2(0, rb.velocity.y);
            anim.SetBool("isTiptoeing", false); 
            anim.SetBool("isRunning", false);
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
        if (context.started && ((isGrounded && !isStunned) || isGrabbingLedge)) { // On button down, jump
            if (isGrabbingLedge) {
                LetGoOfLedge();
            }
            isJumping = true;
            jumpTimeCounter = jumpTime;
            anim.SetTrigger("takeOff");
            audio.Play("jump");
        }

        if (context.canceled) { // If button released, stop jumping
            isJumping = false;
        }
    }

    public void TakeDamage(int damage) {
        health -= damage;
        healthUI.RemoveHearts(damage);
        anim.SetTrigger("takeDamage");
        Instantiate(blood, new Vector2(transform.position.x, transform.position.y + 3f), Quaternion.identity);
        Stun();
    }

    bool isFalling { get { return (!isGrounded && rb.velocity.y < 0); } }

    public void Tiptoe(InputAction.CallbackContext context) {
        if (context.started) {
            isTiptoeing = true;
            anim.SetBool("isTiptoeing", true); 
        }

        if (context.canceled) {
            isTiptoeing = false;
            anim.SetBool("isTiptoeing", false); 
        }
    }

    void Stun() {
        isStunned = true;
    }
    public void ResetStun() {
        isStunned = false;
        if (health <= 0) {
            Die();
        }
    }

    void Die() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
