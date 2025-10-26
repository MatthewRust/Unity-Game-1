using System;
using System.Data;
using System.Runtime;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class Player : MonoBehaviour
{
    private Animator ani;
    private Rigidbody2D rb;


    [Header("Attack Details")]

    [SerializeField] private LayerMask checkIfEnemy;
    [SerializeField] private float heavyAttackDamage = 10.0f;
    [SerializeField] private float lightAttackDamage = 4.0f;
    [SerializeField] private float specialAttackDamage = 7.0f;
    private string currentAttackType = "lightAttack";

    [Header("Player Movement Details")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float slideSpeed = 5.0f;
    [SerializeField] private float slideDuration = 0.5f;
    private float slideTimer = 0f;
    private bool isSliding = false;
    [SerializeField] private float jumpForce = 8.0f;
    private float xInput;
    private bool canMove = true;
    private bool canJump = true;


    [Header("Attack Hitboxes")]
    [SerializeField] private float heavyAttackRadius;
    [SerializeField] private float lightAttackRadius;
    [SerializeField] private float specialAttackRadius;
    [SerializeField] private Transform heavyAttackPoint;
    [SerializeField] private Transform lightAttackPoint;
    [SerializeField] private Transform specialAttackPoint;


    private bool playerFacingRight = true;

    [Header("Collision Detials")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask ground;
    private bool isGround;
    private CapsuleCollider2D standingCol;
    private BoxCollider2D slidingCol;


    [SerializeField] private float health;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponentInChildren<Animator>();
        standingCol = GetComponent<CapsuleCollider2D>();
        slidingCol = GetComponent<BoxCollider2D>();

        standingCol.enabled = true;
        slidingCol.enabled = false;
   }



    private void Update()
    {
        handleCollision();
        handleInputs();
        handleMovement();
        handleAnimation();

    }

    public void enableJumpAndMovement(bool enable)
    {
        enableJump(enable);
        enableMovement(enable);
    }

    public void enableJump(bool enable)
    {
        canJump = enable;
    }

    public void enableMovement(bool enable)
    {
        canMove = enable;
    }

    private void handleAnimation()
    {
        ani.SetFloat("xVelocity", rb.linearVelocity.x);
        ani.SetBool("isGrounded", isGround);
        ani.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private void tryToJump()
    {
        if (isGround && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void tryToSlide()
    {
        if (isGround && Mathf.Abs(rb.linearVelocity.x) > 0.1f && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            ani.SetTrigger("slide");

            // Switch colliders
            standingCol.enabled = false;
            slidingCol.enabled = true;

            // Apply forward boost depending on facing direction
            float direction = playerFacingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * slideSpeed, rb.linearVelocity.y);
        }
    }

    private void handleMovement()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f)
            {
                // Before standing up, check if there’s space above
                if (canStandUp())
                {
                    isSliding = false;
                    standingCol.enabled = true;
                    slidingCol.enabled = false;
                }
                else
                {
                    // keep sliding collider until there’s room to stand
                    slideTimer = 0.1f; 
                }
            }

            return; // don't overwrite velocity during slide
        }

        if (canMove)
        {
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
            handlePlayerFlip();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }



    private bool canStandUp()
    {
        // Cast upwards from player position slightly above sliding collider
        float checkDistance = 1.0f; // adjust this based on your sprite height
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, checkDistance, ground);
        return hit.collider == null;
    }



    private void handleInputs()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            tryToJump();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            tryToAttack("lightAttack");
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            tryToAttack("heavyAttack");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            tryToAttack("specialAttack");
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            tryToSlide();
        }
    }

    private void tryToAttack(string attackType)
    {
        if (isGround)
        {
            currentAttackType = attackType;
            ani.SetTrigger(attackType);
        }
    }

    public void damageEnemies()
    {
        if (currentAttackType == "lightAttack")
        {
            Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(lightAttackPoint.position, lightAttackRadius, checkIfEnemy);
            foreach (Collider2D enemy in enemyColliders)
                enemy.GetComponent<Enemy>().takeDamange(lightAttackDamage);
        }
        else if (currentAttackType == "heavyAttack")
        {
            Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(heavyAttackPoint.position, heavyAttackRadius, checkIfEnemy);
            foreach (Collider2D enemy in enemyColliders)
                enemy.GetComponent<Enemy>().takeDamange(heavyAttackDamage);
        }
        else if (currentAttackType == "specialAttack")
        {
            Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(specialAttackPoint.position, specialAttackRadius, checkIfEnemy);
            foreach (Collider2D enemy in enemyColliders)
                enemy.GetComponent<Enemy>().takeDamange(specialAttackDamage);
        }
    }


    [ContextMenu("flipPlayer")]
    private void flipPlayer()
    {
        transform.Rotate(0, 180, 0);
        playerFacingRight = !playerFacingRight;
    }

    private void handlePlayerFlip()
    {
        if (rb.linearVelocity.x > 0 && playerFacingRight == false)
        {
            flipPlayer();
        }
        else if (rb.linearVelocity.x < 0 && playerFacingRight == true)
        {
            flipPlayer();
        }
    }
    private void handleCollision()
    {
        isGround = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, ground);
    }


    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(heavyAttackPoint.position, attackRadius);
        //Gizmos.DrawWireSphere(lightAttackPoint.position, lightAttackRadius);
        //Gizmos.DrawWireSphere(specialAttackPoint.position, specialAttackRadius);
    }

    public float getLightDamage()
    {
        return lightAttackDamage;
    }
}

