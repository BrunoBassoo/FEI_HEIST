using UnityEngine;
using TMPro;

/// <summary>
/// Sistema de esconderijo - Player pode se esconder em objetos
/// Use em armários, caixas, arbustos, etc.
/// </summary>
public class HidingSpot : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private KeyCode teclaEsconder = KeyCode.F;
    [SerializeField] private string mensagemEsconder = "Pressione [F] para esconder";
    [SerializeField] private string mensagemSair = "Pressione [F] para sair";
    
    [Header("UI (Opcional)")]
    [SerializeField] private GameObject painelMensagem;
    [SerializeField] private TextMeshProUGUI textoMensagem;
    
    [Header("Efeitos (Opcional)")]
    [SerializeField] private AudioClip somEntrar;
    [SerializeField] private AudioClip somSair;
    private AudioSource audioSource;
    
    private bool playerPerto = false;
    private bool playerEscondido = false;
    private GameObject playerObj = null;
    private PlayerMoviment playerScript = null;
    
    void Start()
    {
        // Pega ou cria AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (somEntrar != null || somSair != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Esconde a mensagem no início
        if (painelMensagem != null)
        {
            painelMensagem.SetActive(false);
        }
    }
    
    void Update()
    {
        // Se o player está perto e aperta F
        if (playerPerto && Input.GetKeyDown(teclaEsconder))
        {
            if (!playerEscondido)
            {
                EsconderPlayer();
            }
            else
            {
                MostrarPlayer();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerPerto = true;
            playerObj = other.gameObject;
            playerScript = other.GetComponent<PlayerMoviment>();
            
            // Mostra mensagem
            if (!playerEscondido)
            {
                MostrarMensagem(mensagemEsconder);
            }
            
            Debug.Log($"Player perto do esconderijo: {gameObject.name}");
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Só permite sair se não estiver escondido
            if (!playerEscondido)
            {
                playerPerto = false;
                playerObj = null;
                playerScript = null;
                
                EsconderMensagem();
                
                Debug.Log("Player saiu do alcance do esconderijo");
            }
        }
    }
    
    void EsconderPlayer()
    {
        if (playerObj == null || playerScript == null) return;
        
        playerEscondido = true;
        
        Debug.Log($"🚪 Player se escondeu em: {gameObject.name}");
        
        // Toca som de entrar
        if (audioSource != null && somEntrar != null)
        {
            audioSource.PlayOneShot(somEntrar);
        }
        
        // Desativa o sprite renderer (player fica invisível)
        SpriteRenderer playerSprite = playerObj.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.enabled = false;
        }
        
        // Desativa o collider do player (inimigo não colide)
        Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        // Para o movimento do player
        if (playerScript != null)
        {
            playerScript.PararMovimento();
        }
        
        // Atualiza mensagem
        MostrarMensagem(mensagemSair);
    }
    
    void MostrarPlayer()
    {
        if (playerObj == null || playerScript == null) return;
        
        playerEscondido = false;
        
        Debug.Log($"👤 Player saiu do esconderijo: {gameObject.name}");
        
        // Toca som de sair
        if (audioSource != null && somSair != null)
        {
            audioSource.PlayOneShot(somSair);
        }
        
        // Reativa o sprite renderer
        SpriteRenderer playerSprite = playerObj.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.enabled = true;
        }
        
        // Reativa o collider
        Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        
        // Libera o movimento
        if (playerScript != null)
        {
            playerScript.LiberarMovimento();
        }
        
        // Atualiza mensagem
        MostrarMensagem(mensagemEsconder);
    }
    
    void MostrarMensagem(string mensagem)
    {
        if (painelMensagem != null)
        {
            painelMensagem.SetActive(true);
        }
        
        if (textoMensagem != null)
        {
            textoMensagem.text = mensagem;
        }
    }
    
    void EsconderMensagem()
    {
        if (painelMensagem != null)
        {
            painelMensagem.SetActive(false);
        }
    }
    
    // Método público para verificar se o player está escondido
    public bool PlayerEstaEscondido()
    {
        return playerEscondido;
    }
    
    // Se o player for detectado por um inimigo, pode forçar sair do esconderijo
    public void ForcarSairDoEsconderijo()
    {
        if (playerEscondido)
        {
            Debug.LogWarning("⚠️ Player foi descoberto! Saindo do esconderijo...");
            MostrarPlayer();
        }
    }
}

