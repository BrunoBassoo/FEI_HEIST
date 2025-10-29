using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 2f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool movingRight = true;
    private Rigidbody2D rb;

    [Header("Visão do jogador")]
    public float visionRange = 5f;
    private Transform player;

    [Header("Vida e dano")]
    public int maxHealth = 2;
    private int currentHealth;
    public float knockbackForce = 5f;  // força que o inimigo é empurrado ao levar dano
    public float knockbackDuration = 0.2f;

    private bool isTakingDamage = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isTakingDamage) return; // enquanto estiver sofrendo dano, não anda

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= visionRange)
        {
            // Persegue o jogador (movimento em X e Y)
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * (speed * 3f); // 3x mais rápido quando persegue
        }
        else
        {
            // Patrulha: anda pra um lado, e ao colidir, inverte
            float moveDirection = movingRight ? 1 : -1;
            rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se colidir com parede ou Tilemap, muda de direção
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            movingRight = !movingRight;
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Método para o inimigo tomar dano
    public void TakeDamage(Vector2 hitDirection)
    {
        if (isTakingDamage) return;

        currentHealth--;
        Debug.Log("Inimigo levou dano! Vidas restantes: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Recuo (knockback)
        StartCoroutine(Knockback(hitDirection));
    }

    private IEnumerator Knockback(Vector2 hitDirection)
    {
        isTakingDamage = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(hitDirection * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        isTakingDamage = false;
    }

    void Die()
    {
        Debug.Log("Inimigo morreu!");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
