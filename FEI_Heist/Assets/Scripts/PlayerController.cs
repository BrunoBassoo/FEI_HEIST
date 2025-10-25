using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 5f;
    private Vector2 movement;

    public int keysCollected = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
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
