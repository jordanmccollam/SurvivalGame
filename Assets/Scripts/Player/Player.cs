using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;
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
    public int food;
    public float timeToEat;
    public float lookRange;

    Vector2 input;
    Vector2 lookDir;
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
    CinemachineFramingTransposer lookCamera;
    [HideInInspector] public Rigidbody2D rb;
    PlayerUI UI;
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
    public float deathToGameOverTime;
    string currentLookDir = "na";
    float baseGravity;
    int maxHealth;
    int maxFood;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Animator>();
        lookCamera = camera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();

        maxHealth = health;
        maxFood = food;
        UI = GameObject.FindGameObjectWithTag("PlayerUI").GetComponent<PlayerUI>();
        UI.SetMaxHealth(health);
        UI.SetMaxHunger(food);

        baseGravity = rb.gravityScale;

        InvokeRepeating("Blink", blinkTime, blinkTime);
        InvokeRepeating("Eat", timeToEat, timeToEat);
    }

    void Blink() {
        anim.SetTrigger("blink");
    }

    void Eat() {
        // Player is hungry (and not about to die)
        if (food <= 0 && health > 1) {
            // No food: Take damage
            TakeDamage(1);
        } else {
            // Eat one food
            food--;
            UI.SetHunger(food);
        }
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
        Look();

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
            wasFalling = false;
        }
        isGrounded = _isGrounded;

        // Fall damage stuff
        if (!wasFalling && isFalling) {
            wasFalling = true;
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
                anim.SetBool("isTiptoeing", false);
                audio.Stop("run");
            } else if (input.x != 0 && !isRunning) {
                isRunning = true;
                anim.SetBool("isRunning", true);
                audio.Loop("run");

                if (isTiptoeing) {
                    anim.SetBool("isTiptoeing", true);
                } 
            }

            // Bug fixes ---
            if (input.x != 0 && isTiptoeing) {
                audio.Stop("run");
            }
            if (input.x == 0) {
                anim.SetBool("isRunning", false);
            }
            // ------------

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
            audio.Stop("run");
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
        UI.SetHealth(health);
        camera.SetTrigger("shake");
        anim.SetTrigger("takeDamage");
        audio.Play("hurt");
        Instantiate(blood, new Vector2(transform.position.x, transform.position.y + 1f), Quaternion.identity);
        Stun();

        if (health <= 0) {
            Die();
        }
    }

    public bool isFalling { get { return (!isGrounded && rb.velocity.y < 0); } }

    public void Tiptoe(InputAction.CallbackContext context) {
        if (context.started) {
            isTiptoeing = true;
            if (isRunning) {
                anim.SetBool("isTiptoeing", true); 
            }
        }

        if (context.canceled) {
            isTiptoeing = false;
            isRunning = false;
            anim.SetBool("isTiptoeing", false); 
        }
    }

    void Stun() {
        isStunned = true;
    }
    public void ResetStun() {
        isStunned = false;
    }

    void Die() {
        anim.SetTrigger("die");

        Invoke("ResetGame", deathToGameOverTime);
    }

    void ResetGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnLook(InputAction.CallbackContext context) {
        lookDir = context.ReadValue<Vector2>();
    }

    void Look() {
        // .45 is the default cam posY
        lookCamera.m_ScreenY = 0.45f + (lookDir.y / lookRange);
    }
}
