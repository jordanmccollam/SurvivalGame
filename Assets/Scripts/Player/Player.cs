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
    public float balloonForce;
    public float balloonTime;
    public int food;
    public float timeToEat;
    public float lookRange;
    public int balloons;
    public float punchTime;
    public int coins;
    public float punchRange;

    Vector2 input;
    Vector2 lookDir;
    float jumpTimeCounter;
    float balloonTimeCounter;

    // CHECKS ---
    [HideInInspector] public bool isJumping = false;
    bool isBallooning = false;
    [HideInInspector] public bool canLedgeGrab = true;
    bool facingRight = true;
    bool isGrounded = false;
    bool isTiptoeing = false;
    bool isStunned = false;
    bool isLooking = false;
    bool isRunning = false;
    bool isGrabbingLedge = false;
    bool wasBallooning = false;
    [HideInInspector] public bool isPunching = false;
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
    public ParticleSystem balloonPop;

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
    public float coyoteTime;
    float coyoteTimeCounter;
    float baseGravity;
    int maxHealth;
    int maxFood;
    public Transform rangePoint;
    public LayerMask breakableLayer;
    public LayerMask enemyLayer;

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
        UI.SetBalloonCount(balloons);
        UI.SetCoinCount(coins);

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

    public void PickUpFood(int amount) {
        if (food < maxFood) {
            food += amount;
            UI.SetHunger(food);
        }

        // TODO: Add pickup food sound

        // Reset the eating timer
        CancelInvoke("Eat");
        InvokeRepeating("Eat", timeToEat, timeToEat);
    }

    public void PickUpBalloons(int amount) {
        balloons += amount;
        UI.SetBalloonCount(balloons);
        // TODO: Add pickup balloon sound
    }

    public void PickUpHearts(int amount) {
        if (health < maxHealth) {
            health += amount;
            UI.SetHealth(health);
        }
        // TODO: Add gain health sound
    }

    public void PickUpCoins(int amount) {
        coins += amount;
        UI.SetCoinCount(coins);
        // TODO: Add gain coin sound
    }

    public void OnInput(InputAction.CallbackContext context) {
        input = context.ReadValue<Vector2>();
    }

    private void Update() {
        // Coyote time stuff
        if (isGrounded) {
            coyoteTimeCounter = coyoteTime;
        } else {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter <= 0 && !isGrabbingLedge) {
            rb.gravityScale = baseGravity;
        } else {
            rb.gravityScale = 0;
        }

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

        // If ballooning, float up
        if (isBallooning) {
            isJumping = false;

            if (!wasBallooning) {
                anim.SetBool("isBallooning", true);
            }

            if (balloonTimeCounter > 0 && !isStunned) {
                rb.velocity = Vector2.up * balloonForce;
                balloonTimeCounter -= Time.deltaTime;
            } else {
                // When jump time runs out, stop going higher
                PopBalloon();
            }
        }

        bool isTouchingLedge = Physics2D.OverlapCircle(ledgeCheck.position, checkRadius, whatIsLedge);
        if (isTouchingLedge && canLedgeGrab && !isGrounded) {
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
            } else if (input.x != 0 && !isRunning && isGrounded) {
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
            if (input.x == 0 || !isGrounded) {
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
        if (context.started && (((coyoteTimeCounter > 0 || isBallooning) && !isStunned) || isGrabbingLedge)) { // On button down, jump
            if (isGrabbingLedge) {
                LetGoOfLedge();
            }
            PopBalloon();
            isJumping = true;
            jumpTimeCounter = jumpTime;
            anim.SetTrigger("takeOff");
            audio.Play("jump");
        }

        if (context.canceled) { // If button released, stop jumping
            isJumping = false;
            coyoteTimeCounter = 0;
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

    public bool isFalling { get { return (!isGrounded && rb.velocity.y < 0 && !isBallooning); } }

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
        Stun();
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
        lookCamera.m_ScreenY = 0.45f + (lookDir.y / lookRange); // .45 is the default cam posY

        if (!isLooking && lookDir.y != 0) {
            isLooking = true;
            Stun();
        }
        else if (isLooking && lookDir.y == 0) {
            isLooking = false;
            ResetStun();
        }
    }

    public void Balloon(InputAction.CallbackContext context) {
        if (context.started && balloons > 0) { // On button down, blow balloon
            isBallooning = true;
            balloonTimeCounter = balloonTime;
            anim.SetTrigger("blowBalloon");
            // TODO: Play balloon blow up sound

            balloons--;
            UI.SetBalloonCount(balloons);
        }
    }
    void PopBalloon() {
        if (isBallooning) {
            isBallooning = false;
            anim.SetBool("isBallooning", false);
            balloonPop.Play();
        }
    }

    public void OnAction(InputAction.CallbackContext context) {
        if (context.started) {
            Punch();
        }
    }

    void Punch() {
        if (!isPunching) {
            // Play attack anim
            isPunching = true;
            camera.SetTrigger("shake");
            anim.SetBool("isPunching", true);
            Invoke("StopPunchAnim", punchTime);

            // Detect enemies (or breakables) in range of attack
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(rangePoint.position, punchRange, enemyLayer);

            // Damage or break them
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.tag == "Pot") {
                    Pot pot = enemy.GetComponent<Pot>();
                    pot.BreakPot();
                }
                if (enemy.tag == "Tree") {
                    AppleTree tree = enemy.GetComponent<AppleTree>();
                    tree.ShakeTree();
                }
            }
        }
    }

    void StopPunchAnim() {
        anim.SetBool("isPunching", false);
    }
    public void StopPunch() {
        isPunching = false;
    }

    void OnDrawGizmosSelected() {
        if (rangePoint == null) {
            return;
        }

        Gizmos.DrawWireSphere(rangePoint.position, punchRange);
    }
}
