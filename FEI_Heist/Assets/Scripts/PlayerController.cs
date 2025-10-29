using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 5f;
    private Vector2 movement;

    public int keysCollected = 0;

    [Header("Ataque")]
    public float attackRange = 1f;      // distância do ataque
    public float attackCooldown = 0.5f; // tempo entre ataques
    private float lastAttackTime = 0f;
    public int damage = 1;              // dano que o inimigo leva

    [Header("Stealth")]
    public float stealthMultiplier = 0.5f; // anda 50% mais devagar ao segurar Shift

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Movimento
        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Se estiver segurando Shift, reduz a velocidade
        float currentSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= stealthMultiplier;
        }

        // Aplica movimento
        rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);

        // Ataque — tecla padrão: Espaço
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
{
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

    foreach (Collider2D enemy in hitEnemies)
    {
        if (enemy.CompareTag("enemy"))
        {
            Vector2 hitDirection = (enemy.transform.position - transform.position).normalized;
            enemy.GetComponent<EnemyAI>().TakeDamage(hitDirection);
            Debug.Log("Acertou o inimigo!");
        }
    }
}

    private void OnDrawGizmosSelected()
    {
        // Mostra o raio de ataque no editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("key"))
        {
            keysCollected++;
            Destroy(other.gameObject);
            Debug.Log("Chaves: " + keysCollected);
        }
    }

    // Método para usar uma chave
    public bool UseKey()
    {
        if (keysCollected > 0)
        {
            keysCollected--;
            Debug.Log("Usou uma chave. Restam: " + keysCollected);
            return true;
        }
        else
        {
            Debug.Log("Sem chaves!");
            return false;
        }
    }
}
