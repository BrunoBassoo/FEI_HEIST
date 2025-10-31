using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 5f;
    private Vector2 movement;

    public int keysCollected = 0;
    public int materiasCollected = 0; // contador de matérias

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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var col in hits)
        {
            if (col.CompareTag("enemy"))
            {
                // direção do golpe para empurrar o inimigo pra trás
                Vector2 hitDir = (col.transform.position - transform.position).normalized;

                // tenta achar o script correto no inimigo
                if (col.TryGetComponent<EnemyChasePatrol>(out var enemy))
                {
                    enemy.TakeDamage(hitDir);
                }
                else
                {
                    // se o collider é filho, tenta no pai
                    var enemyOnParent = col.GetComponentInParent<EnemyChasePatrol>();
                    if (enemyOnParent) enemyOnParent.TakeDamage(hitDir);
                }
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

        if (other.CompareTag("trofeu"))
        {
            Destroy(other.gameObject);
            Debug.Log("Pegou o troféu!");
        }

        // --- NOVO: coleta de matéria ---
        if (other.CompareTag("materia"))
        {
            if (materiasCollected < 2)
            {
                materiasCollected++;
                Destroy(other.gameObject);
                Debug.Log("Matérias: " + materiasCollected + "/2");
            }
            else
            {
                Debug.Log("Já está com o limite máximo de matérias (2/2)");
            }
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
