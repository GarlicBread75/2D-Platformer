using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    #region Variables
    [Header("Health")]
    [SerializeField] int maxHp;
    [SerializeField] int currentHp;
    bool dead;

    [Header("Respawning")]
    [SerializeField] float respawnDelay;
    [SerializeField] Transform respawnPoint;

    [Header("IFrames")]
    [SerializeField] float iframesDuration;
    [SerializeField] int iframesFlashes;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] Material flashMaterial;
    [SerializeField] float flashDuration;
    Material originalMaterial;

    [Header("Combat")]
    [SerializeField] float damage;
    [SerializeField] float range;
    bool attacking;
    bool sword;

    [Space]
    [Space]
    [Space]
    [Header("Movement")]
    [Space]
    [Header("Running")]
    [SerializeField] float maxSpeed;
    [SerializeField] float velPower;
    [SerializeField] float acceleration;
    [SerializeField] float deceleration;
    [SerializeField] Vector2 rbSpeed;
    Rigidbody2D rb;
    Vector2 moveInput;


    [Header("Jumping")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpMultiplier;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject jumpParticles;
    [SerializeField] float apexGravity;
    [SerializeField] float downwardsGravity;
    [SerializeField] float maxFallSpeed;
    float normalGravity;
    bool jumpPressed;
    bool extraJumpPressed;
    bool extraJump = true;
    bool landing;


    [Header("Crouching")]
    [SerializeField] float crouchSpeed;
    CapsuleCollider2D col;
    float originalSpeed;
    bool crouched;
    bool crouchPressed;


    [Header("Dashing")]
    [SerializeField] Vector2 dashPower;
    [SerializeField] float dashDuration;
    [SerializeField] Vector2 afterDashVelocity;
    TrailRenderer tr;
    Vector2 dashDir;
    bool dashPressed;
    bool isDashing;
    bool canDash = true;


    [Header("Audio")]
    [SerializeField] UnityEvent[] Sounds;


    // Animation
    Animator anim;

    // Misc
    SpriteRenderer sr;
    bool facingRight = true;
    bool alrChecked;
    #endregion

    #region Start
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        tr = GetComponent<TrailRenderer>();
        originalSpeed = maxSpeed;
        normalGravity = rb.gravityScale;
        originalMaterial = sr.material;
        currentHp = maxHp;
    }
    #endregion

    #region Update
    void Update()
    {
        if (dead)
        {
            return;
        }

        #region Attacking
        if (Input.GetMouseButtonDown(0))
        {
            attacking = true;
        }
        #endregion

        #region Running
        moveInput.x = Input.GetAxis("Horizontal");
        #endregion

        #region Jumping
        if (Input.GetKeyDown(KeyCode.Space) && Grounded() && !crouched)
        {
            jumpPressed = true;
        }
        else
        if (Input.GetKeyDown(KeyCode.Space) && !Grounded() && extraJump)
        {
            extraJumpPressed = true;
            dashPressed = false;
            isDashing = false;
            canDash = true;
        }
        #endregion

        #region Crouching
        if (Input.GetKey(KeyCode.LeftControl) && Grounded())
        {
            crouchPressed = true;
        }
        else
        {
            crouchPressed = false;
        }
        #endregion

        #region Dashing
        if (Input.GetKeyDown(KeyCode.LeftShift) && !dashPressed && !isDashing && canDash)
        {
            dashPressed = true;
            isDashing = true;
            canDash = false;
        }
        #endregion
    }
    #endregion

    #region Fixed Update
    private void FixedUpdate()
    {
        if (dead)
        {
            return;
        }

        #region Health
        if (currentHp == 0)
        {
            StartCoroutine(Die());
        }
        #endregion

        #region Running

        float targetSpeed = moveInput.x * maxSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (rb.velocity.x > 0)
        {
            facingRight = true;
            Flip();
        }
        else
        if (rb.velocity.x < 0)
        {
            facingRight = false;
            Flip();
        }

        if (Grounded() && !landing && !crouched)
        {
            if (rb.velocity.x < -2 || rb.velocity.x > 2)
            {
                Sounds[0].Invoke();
            }
            else
            {
                Sounds[1].Invoke();
            }
        }
        else
        {
            Sounds[1].Invoke();
        }
        #endregion

        #region Jumping
        if (jumpPressed)
        {
            rb.velocity = new Vector2 (rb.velocity.x, jumpForce);
            Sounds[4].Invoke();
            Invoke(nameof(DelayAC), 0.01f);
            anim.SetBool("Jumping", true);
            anim.SetBool("Landing", false);
            jumpPressed = false;
        }
        else
        if (extraJumpPressed)
        {
            GameObject thing = Instantiate(jumpParticles, transform.position, Quaternion.identity);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpMultiplier);
            rb.gravityScale = normalGravity;
            Sounds[4].Invoke();
            Invoke(nameof(DelayAC), 0.01f);
            sr.color = Color.white;
            tr.emitting = false;
            extraJump = false;
            anim.SetBool("Jumping", true);
            anim.SetBool("Flipping", true);
            anim.SetBool("Jumping", false);
            Destroy(thing, 0.6f);
            extraJumpPressed = false;
        }

        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }
        #endregion

        #region Crouching
        if (crouchPressed)
        {
            crouched = true;
            maxSpeed = crouchSpeed;
            ChangeColliderSize();
            anim.SetBool("Crouching", true);
        }
        else
        {
            crouched = false;
            maxSpeed = originalSpeed;
            ChangeColliderSize();
            anim.SetBool("Crouching", false);
        }
        #endregion

        #region Dashing
        if (dashPressed)
        {
            dashPressed = false;
            sr.color = Color.cyan;
            tr.emitting = true;
            Sounds[6].Invoke();
            isDashing = true;
            dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (dashDir == Vector2.zero)
            {
                if (transform.localScale.x > 0)
                {
                    rb.velocity = Vector2.right * dashPower;
                }
                else
                if (transform.localScale.x < 0)
                {
                    rb.velocity = Vector2.left * dashPower;
                }
            }
            else
            {
                rb.velocity = dashDir * dashPower;
            }

            StartCoroutine(StopDashing());
        }
        #endregion

        if (!Grounded() && extraJump)
        {
            anim.SetBool("Jumping", true);
            alrChecked = false;
        }

        rbSpeed = rb.velocity;
        UpdateAnimParameters();
    }
    #endregion

    #region Collision Enter
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6 && Grounded())
        {
            Sounds[5].Invoke();
            tr.emitting = false;
            sr.color = Color.white;
            canDash = true;

            if (!alrChecked)
            {
                extraJump = true;
                dashPressed = false;
                rb.gravityScale = normalGravity;
                landing = true;
                anim.SetBool("Landing", true);
                anim.SetBool("Flipping", false);
                anim.SetBool("Jumping", false);
                alrChecked = true;
            }
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(TakeDamage(1));
        }
    }
    #endregion

    #region Trigger Enter
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Dash Gem"))
        {
            dashPressed = false;
            isDashing = false;
            canDash = true;
            tr.emitting = false;
            sr.color = Color.white;
            Destroy(collision.gameObject);
        }
    }
    #endregion

    #region Methods
    IEnumerator TakeDamage(int dmg)
    {
        if (StompEnemy())
        {
            rb.velocity = new Vector2(transform.localScale.x/5.1f * 7, 10);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Sounds[7].Invoke();
            currentHp -= dmg;
            yield return new WaitForSeconds(0.1f);
            // iframes
            Physics2D.IgnoreLayerCollision(3, 7, true);
            sr.material = flashMaterial;
            yield return new WaitForSeconds(iframesDuration);
            sr.material = originalMaterial;
            Physics2D.IgnoreLayerCollision(3, 7, false);
        }
    }

    void Heal(int heal)
    {
        currentHp += heal;
    }

    void IncreaseMaxHp(int num)
    {
        maxHp += num;
    }

    void DecreaseMaxHp(int num)
    {
        maxHp -= num;
    }

    IEnumerator Die()
    {
        // disable player input
        dead = true;
        // flash white once (changing player sprite material)
        sr.material = flashMaterial;
        // wait a bit before changing sprite material to normal
        yield return new WaitForSeconds(flashDuration);
        Sounds[1].Invoke();
        // change player sprite material to normal
        sr.material = originalMaterial;
        // wait until player is grounded
        if (!Grounded())
        {
            yield return new WaitUntil(() => Grounded());
        }
        // play death animation
        anim.SetBool("Dead", true);
        // disabling everything else while dead
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        rb.gravityScale = 0;
        col.enabled = false;
        // do transition without changing scenes

        // wait a short delay before respawning
        yield return new WaitForSeconds(respawnDelay);
        // move player
        transform.position = respawnPoint.position;
        // reset current hp
        currentHp = maxHp;
        // re-enable everything
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 0);
        rb.gravityScale = normalGravity;
        col.enabled = true;
        // enable player input
        anim.SetBool("Dead", false);
        dead = false;
    }

    bool Grounded()
    {
        if (!crouched)
        {
            return Physics2D.BoxCast(col.bounds.center, col.bounds.size - new Vector3(0.1f, 0.1f, 0), 0, Vector2.down, 0.1f, groundLayer);
        }
        else
        {
            return Physics2D.BoxCast(col.bounds.center, col.bounds.size - new Vector3(0.1f, 0.1f, 0), 0, Vector2.down, 0.1f, groundLayer);
        }
    }

    bool StompEnemy()
    {
        return Physics2D.BoxCast(col.bounds.center, col.bounds.size - new Vector3(0.1f, 0, 0), 0, Vector2.down, 0.1f, enemyLayer);
    }

    void Flip()
    {
        if (facingRight && transform.localScale.x < 0)
        {
            Vector2 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }
        else
        if (!facingRight && transform.localScale.x > 0)
        {
            Vector2 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }
    }

    void ChangeColliderSize()
    {
        if (crouched)
        {
            col.offset = new Vector2 (col.offset.x, -0.063f);
            col.size = new Vector2(col.size.x, 0.19f);
        }
        else
        {
            col.offset = new Vector2(col.offset.x, -0.01017f);
            col.size = new Vector2(col.size.x, 0.3f);
        }
    }

    IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashDuration);
        rb.velocity = dashDir * afterDashVelocity;
        tr.emitting = false;
        isDashing = false;
    }

    void UpdateAnimParameters()
    {
        anim.SetFloat("SpeedX", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("SpeedY", rb.velocity.y);
    }

    void DisableLanding()
    {
        landing = false;
        anim.SetBool("Landing", false);
    }

    void DisableAttack()
    {
        attacking = false;
        anim.SetBool("Attacking", false);
    }

    void DelayAC()
    {
        alrChecked = false;
    }
    #endregion
}