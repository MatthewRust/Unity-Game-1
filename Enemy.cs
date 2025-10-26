using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;

    [Header("Enemy Stats")]
    [SerializeField] private float redColourDuration;

    [SerializeField] private float health;
    [SerializeField] private float deathDelay;


    [Header("Attack Details")]
    [SerializeField] private float attackRadius;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask checkIfPlayer;
    [SerializeField] private float enemyHeavyAttackDamage;
    [SerializeField] private float enemyLightAttackDamage;


    private bool isDead;
    private float currentTime;
    private float lastTimeDamage;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }
    public void takeDamange(float damage)
    {
        if (isDead) return;
        sr.color = Color.red;
        lastTimeDamage = Time.time;
        health -= damage;
        if (health <= 0)
        {
            die();
        }
    }

    private void Update()
    {
        if (isDead) return;
        changeColorWhenDamage();
    }

    private void changeColorWhenDamage()
    {
        currentTime = Time.time;
        if (currentTime > lastTimeDamage + redColourDuration)
        {
            if (sr.color != Color.white)
            {
                sr.color = Color.white;
            }
        }
    }

    public void damagePlayer()
    {
        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, checkIfPlayer);

        foreach (Collider2D enemy in enemyColliders)
        {
            enemy.GetComponent<Enemy>().takeDamange(enemyLightAttackDamage);
        }
    }
    private void die()
    {
        isDead = true;
        if (col != null) col.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        Destroy(gameObject, deathDelay);
    }
}


