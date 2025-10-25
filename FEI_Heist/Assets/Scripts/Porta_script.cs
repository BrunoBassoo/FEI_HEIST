using UnityEngine;

public class Door : MonoBehaviour
{
    private bool isOpen = false;
    public float openSpeed = 2f;
    public Vector3 openOffset;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    // Collider físico que bloqueia o jogador
    private Collider2D solidCollider;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;

        // Pegamos o collider físico da porta
        // (garanta que o trigger e o sólido sejam diferentes)
        solidCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o collider que acionou é o do tipo "trigger"
        // e se o outro é o jogador
        if (other.CompareTag("Player") && !isOpen)
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null && player.UseKey()) // se tiver chave
            {
                isOpen = true;
                StopAllCoroutines();
                StartCoroutine(OpenDoor());
            }
            else
            {
                Debug.Log("A porta está trancada. Você precisa de uma chave.");
            }
        }
    }

    private System.Collections.IEnumerator OpenDoor()
    {
        // desativa o collider físico (porta deixa de bloquear)
        if (solidCollider != null)
            solidCollider.enabled = false;

        while (Vector3.Distance(transform.position, openPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Porta aberta!");
    }
}
