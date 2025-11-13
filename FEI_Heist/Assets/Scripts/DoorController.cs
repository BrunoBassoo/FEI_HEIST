using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    
    [Header("Configurações")]
    [SerializeField] private bool portaAberta = false;
    [SerializeField] private float tempoParaDestruir = 0f; // 0 = não destrói
    
    [Header("Destruir Objeto ao Abrir")]
    [SerializeField] private bool destruirObjetoComTag = false;
    [SerializeField] private string tagParaDestruir = "porta_visual"; // Tag do objeto a destruir
    [SerializeField] private GameObject objetoParaDestruir; // OU arraste o objeto diretamente aqui
    [SerializeField] private float delayParaDestruir = 0f; // Delay antes de destruir (para animação)
    
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
        
        // Destrói objeto com tag específica (se configurado)
        if (destruirObjetoComTag)
        {
            DestruirObjetoEspecifico();
        }
        
        // Destrói a porta depois de um tempo (se configurado)
        if (tempoParaDestruir > 0)
        {
            Destroy(gameObject, tempoParaDestruir);
            Debug.Log("Porta será destruída em " + tempoParaDestruir + " segundos");
        }
    }
    
    void DestruirObjetoEspecifico()
    {
        // Se tem um objeto referenciado diretamente, usa ele
        if (objetoParaDestruir != null)
        {
            Debug.Log($"🗑️ Destruindo objeto '{objetoParaDestruir.name}' referenciado diretamente");
            Destroy(objetoParaDestruir, delayParaDestruir);
            return;
        }
        
        // Caso contrário, procura pela tag
        if (!string.IsNullOrEmpty(tagParaDestruir))
        {
            GameObject[] objetosComTag = GameObject.FindGameObjectsWithTag(tagParaDestruir);
            
            if (objetosComTag.Length > 0)
            {
                // Procura o objeto mais próximo desta porta
                GameObject objetoMaisProximo = EncontrarObjetoMaisProximo(objetosComTag);
                
                if (objetoMaisProximo != null)
                {
                    Debug.Log($"🗑️ Destruindo objeto '{objetoMaisProximo.name}' com tag '{tagParaDestruir}'");
                    Destroy(objetoMaisProximo, delayParaDestruir);
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Nenhum objeto encontrado com tag '{tagParaDestruir}'!");
            }
        }
    }
    
    GameObject EncontrarObjetoMaisProximo(GameObject[] objetos)
    {
        if (objetos.Length == 0) return null;
        
        GameObject maisProximo = objetos[0];
        float menorDistancia = Vector3.Distance(transform.position, maisProximo.transform.position);
        
        // Procura o objeto mais próximo
        foreach (GameObject obj in objetos)
        {
            float distancia = Vector3.Distance(transform.position, obj.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = obj;
            }
        }
        
        return maisProximo;
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

