using System.Collections;
using UnityEngine;

public class EnemyChasePatrol : MonoBehaviour
{
    [Header("Movimentação")]
    public float speed = 2f;
    public float chaseSpeedMultiplier = 3f; // 3x mais rápido ao perseguir
    private bool movingRight = true;
    private Rigidbody2D rb;

    [Header("Detecção do Jogador")]
    public float visionRange = 5f;
    private Transform player;
    private bool isChasing = false;

    [Header("Vida, Dano e Stun")]
    public int maxHealth = 2;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float stunDuration = 5f;

    private int currentHealth;
    private bool isTakingDamage = false;
    private bool isStunned = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        var pObj = GameObject.FindGameObjectWithTag("Player");
        player = pObj ? pObj.transform : null;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player == null) return;
        if (isTakingDamage || isStunned) return;

        float dist = Vector2.Distance(transform.position, player.position);
        isChasing = dist <= visionRange;
    }

    void FixedUpdate()
    {
        if (isTakingDamage || isStunned) { rb.velocity = Vector2.zero; return; }

        if (isChasing && player != null)
        {
            // Persegue em X e Y, 3x mais rápido
            Vector2 dir = (player.position - transform.position).normalized;
            rb.velocity = dir * (speed * chaseSpeedMultiplier);

            // vira visualmente
            if ((dir.x > 0 && !movingRight) || (dir.x < 0 && movingRight)) Flip();
        }
        else
        {
            // Patrulha: esquerda/direita até bater
            float moveDir = movingRight ? 1 : -1;
            rb.velocity = new Vector2(moveDir * speed, rb.velocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length == 0) return;

        // Se bateu lateralmente (parede/tilemap), inverte
        Vector2 n = collision.contacts[0].normal;
        if (Mathf.Abs(n.x) > 0.5f)
            Flip();
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // ===== DANO / STUN =====
    public void TakeDamage(Vector2 hitDirection)
    {
        if (isTakingDamage || isStunned) return;

        currentHealth--;

        if (currentHealth <= 0)
        {
            // entra em stun (parado 5s) e depois “recupera”
            StartCoroutine(StunRoutine());
        }
        else
        {
            // aplica knockback curto
            StartCoroutine(Knockback(hitDirection));
        }
    }

    private IEnumerator Knockback(Vector2 hitDirection)
    {
        isTakingDamage = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        isTakingDamage = false;
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;
        rb.velocity = Vector2.zero;
        // opcional: piscar, trocar cor etc.
        yield return new WaitForSeconds(stunDuration);

        // “recupera” após o stun
        currentHealth = maxHealth;
        isStunned = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
