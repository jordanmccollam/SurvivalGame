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
    bool isSneaking = false;
    [HideInInspector] public bool isStunned = false;
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
    state currentState = state.IDLE;

    enum state { IDLE, RUNNING, JUMPING, FALLING, FLOATING, LANDING, SNEAKING, PUNCHING, GRABBINGLEDGE, DEAD }

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

        audio.Play("bite");

        // Reset the eating timer
        CancelInvoke("Eat");
        InvokeRepeating("Eat", timeToEat, timeToEat);
    }

    public void PickUpBalloons(int amount) {
        balloons += amount;
        UI.SetBalloonCount(balloons);
        audio.Play("bonus");
    }

    public void PickUpHearts(int amount) {
        if (health < maxHealth) {
            health += amount;
            UI.SetHealth(health);
        }
        audio.Play("bonus");
    }

    public void PickUpCoins(int amount) {
        coins += amount;
        UI.SetCoinCount(coins);
        audio.Play("bonus");
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
                // rb.velocity = Vector2.up * jumpForce;
                rb.velocity = new Vector2(rb.velocity.x, 1f * jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            } else {
                // When jump time runs out, stop going higher
                SetState(state.FALLING);
            }
        }

        // If ballooning, float up
        if (isBallooning) {
            SetState(state.FLOATING);

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
            SetState(state.GRABBINGLEDGE);
        }
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
            // Just left the ground
        }
        else if (!isGrounded && _isGrounded) {
            SetState(state.LANDING);

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
            rb.velocity = new Vector2(Mathf.Round(input.x) * (isSneaking ? tiptoeSpeed : speed), rb.velocity.y);

            // if moving, use run anim
            if (input.x == 0 && (isRunning || isSneaking)) { 
                SetState(state.IDLE);
            } else if (input.x != 0 && !isRunning && isGrounded) {
                if (isSneaking) {
                    SetState(state.SNEAKING);
                } else {
                    SetState(state.RUNNING);
                }
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
            PopBalloon();
            SetState(state.JUMPING);
        }

        if (context.canceled) { // If button released, stop jumping
            SetState(state.FALLING);
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

    public void Sneak(InputAction.CallbackContext context) {
        if (context.started) {
            SetState(state.SNEAKING);
        }

        if (context.canceled) {
            isSneaking = false;
            isRunning = false; // TODO: do I need this line?
            SetState(state.IDLE);
        }
    }

    void Stun() {
        isStunned = true;
    }
    public void ResetStun() {
        isStunned = false;
    }

    void Die() {
        SetState(state.DEAD);
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
            SetState(state.FLOATING);
            balloons--;
            UI.SetBalloonCount(balloons);
        }
    }
    void PopBalloon() {
        if (isBallooning) {
            SetState(state.IDLE);
        }
    }

    public void OnAction(InputAction.CallbackContext context) {
        if (context.started) {
            Punch();
        }
    }

    void Punch() {
        if (!isPunching) {
            SetState(state.PUNCHING);
            Invoke("StopPunching", punchTime);

            // Detect enemies (or breakables) in range of attack
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(rangePoint.position, punchRange, enemyLayer);

            // Damage or break them
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.tag == "Pot") {
                    Pot pot = enemy.GetComponent<Pot>();
                    pot.BreakPot(audio);
                }
                if (enemy.tag == "Tree") {
                    AppleTree tree = enemy.GetComponent<AppleTree>();
                    tree.ShakeTree();
                }
                if (enemy.tag == "RockMonster") {
                    RockMonster rockMonster = enemy.GetComponent<RockMonster>();
                    rockMonster.TakeDamage(1);
                }
            }
        }
    }

    void StopPunching() {
        SetState(state.IDLE);
    }

    void OnDrawGizmosSelected() {
        if (rangePoint == null) {
            return;
        }

        Gizmos.DrawWireSphere(rangePoint.position, punchRange);
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }

    void SetState(state newState) {
        if (newState == currentState) return;

        // EXIT CURRENT ANIM
        switch (currentState)
        {
            case state.RUNNING:
                isRunning = false;
                anim.SetBool("isRunning", false);
                audio.Stop("run");
                break;
            case state.JUMPING:
                isJumping = false;
                // coyoteTimeCounter = 0; <- TODO: Do I need this line?
                break;
            case state.SNEAKING:
                anim.SetBool("isTiptoeing", false);
                break;
            case state.FLOATING:
                isBallooning = false;
                anim.SetBool("isBallooning", false);
                audio.Play("pop");
                balloonPop.Play();
                break;
            case state.PUNCHING:
                anim.SetBool("isPunching", false);
                isPunching = false;
                break;
            case state.GRABBINGLEDGE:
                anim.SetBool("isGrabbingLedge", false);
                isGrabbingLedge = false;
                rb.gravityScale = baseGravity;
                ResetStun();
                Invoke("ResetLedgeGrab", ledgeJumpCooldown);
                break;
            default:
                break;
        }
        
        
        // START NEW ANIMATION
        switch (newState)
        {
            case state.RUNNING:
                isRunning = true;
                anim.SetBool("isRunning", true);
                audio.Loop("run");
                break;
            case state.JUMPING:
                isJumping = true;
                jumpTimeCounter = jumpTime;
                anim.SetTrigger("takeOff");
                anim.SetBool("isJumping", true);
                audio.Play("jump");
                break;
            case state.SNEAKING:
                isSneaking = true;
                if (input.x != 0) {
                    anim.SetBool("isTiptoeing", true);
                } else {
                    anim.SetBool("isTiptoeing", false);
                }
                break;
            case state.LANDING:
                anim.SetBool("isJumping", false);
                audio.Play("land");
                SetState(state.IDLE);
                return;
            case state.FLOATING:
                if (!isBallooning) {
                    // BLOW BALLOON
                    anim.SetTrigger("blowBalloon");
                    audio.Play("blowBalloon");
                } 
                anim.SetBool("isBallooning", true);
                isBallooning = true;
                balloonTimeCounter = balloonTime;
                break;
            case state.PUNCHING:
                isPunching = true;
                anim.SetBool("isPunching", true);
                camera.SetTrigger("shake");
                audio.Play("punch");
                break;
            case state.GRABBINGLEDGE:
                canLedgeGrab = false;
                isGrabbingLedge = true;
                rb.gravityScale = 0;
                anim.SetBool("isGrabbingLedge", true);
                audio.Play("land");
                Stun();
                rb.velocity = Vector2.zero;
                break;
            case state.DEAD:
                Stun();
                anim.SetTrigger("die");
                break;
            default:
                break;
        }
        
        currentState = newState;
    }
}
