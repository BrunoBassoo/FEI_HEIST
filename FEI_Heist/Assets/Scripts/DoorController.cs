using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    
    [Header("Configurações")]
    [SerializeField] private bool portaAberta = false;
    [SerializeField] private float tempoParaDestruir = 0f; // 0 = não destrói
    
    [Header("Som (Opcional)")]
    [SerializeField] private AudioClip somPortaAbrindo;
    [SerializeField] private AudioClip somPortaTrancada;
    private AudioSource audioSource;
    
    private Collider2D[] colliders;
    
    void Start()
    {
        // Pega o Animator se não foi atribuído
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Pega o AudioSource se existir
        audioSource = GetComponent<AudioSource>();
        
        // Pega todos os colliders da porta
        colliders = GetComponents<Collider2D>();
        
        Debug.Log("Porta inicializada: " + gameObject.name);
    }
    
    // Método chamado pelo PlayerMoviment quando tem chave
    public void AbrirPorta()
    {
        // Se já está aberta, ignora
        if (portaAberta)
        {
            Debug.Log("Porta " + gameObject.name + " já está aberta!");
            return;
        }
        
        portaAberta = true;
        Debug.Log("=== ABRINDO PORTA: " + gameObject.name + " ===");
        
        // Muda a tag para evitar reabrir
        gameObject.tag = "porta_aberta";
        
        // Toca a animação de abertura
        if (animator != null)
        {
            animator.SetBool("Aberta", true);
            animator.SetTrigger("Abrir");
            Debug.Log("Animação de abertura ativada!");
        }
        else
        {
            Debug.LogWarning("Animator não encontrado na porta " + gameObject.name);
        }
        
        // Toca o som de abertura
        if (audioSource != null && somPortaAbrindo != null)
        {
            audioSource.PlayOneShot(somPortaAbrindo);
        }
        
        // Desativa os colliders sólidos (permite passagem)
        DesativarCollidersSolidos();
        
        // Destrói a porta depois de um tempo (se configurado)
        if (tempoParaDestruir > 0)
        {
            Destroy(gameObject, tempoParaDestruir);
            Debug.Log("Porta será destruída em " + tempoParaDestruir + " segundos");
        }
    }
    
    // Método chamado quando player tenta abrir sem chave
    public void PortaTrancada()
    {
        Debug.Log("Porta " + gameObject.name + " está trancada!");
        
        // Toca animação de porta trancada (opcional)
        if (animator != null)
        {
            animator.SetTrigger("Trancada");
        }
        
        // Toca som de porta trancada
        if (audioSource != null && somPortaTrancada != null)
        {
            audioSource.PlayOneShot(somPortaTrancada);
        }
        
        // Aqui você pode adicionar:
        // - Efeito visual de "sacudir" a porta
        // - Mensagem na tela "Precisa de uma chave"
    }
    
    void DesativarCollidersSolidos()
    {
        int desativados = 0;
        
        foreach (Collider2D collider in colliders)
        {
            // Desativa apenas os colliders que NÃO são trigger
            if (!collider.isTrigger)
            {
                collider.enabled = false;
                desativados++;
                Debug.Log("Collider sólido desativado - passagem liberada!");
            }
        }
        
        if (desativados == 0)
        {
            Debug.LogWarning("Nenhum collider sólido encontrado na porta!");
        }
    }
    
    // Getters
    public bool EstaAberta()
    {
        return portaAberta;
    }
}

