using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD do jogo - Mostra contadores de chaves e matérias
/// Fica fixo no topo da tela durante as fases
/// </summary>
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }
    
    [Header("UI - Vidas (Canto Superior Esquerdo)")]
    [SerializeField] private GameObject painelVidas;
    [SerializeField] private Text textoVidas;
    [SerializeField] private Image iconVida;
    
    [Header("UI - Chaves (Canto Superior Centro-Esquerdo)")]
    [SerializeField] private GameObject painelChaves;
    [SerializeField] private Text textoChaves;
    [SerializeField] private Image iconChave;
    
    [Header("UI - Matérias (Canto Superior Direito)")]
    [SerializeField] private GameObject painelMaterias;
    [SerializeField] private Text textoMaterias;
    [SerializeField] private Image iconMateria;
    
    [Header("Configurações")]
    [SerializeField] private bool atualizarAutomaticamente = true;
    [SerializeField] private string formatoTextoVidas = "x {0}"; // Ex: "x 3"
    [SerializeField] private string formatoTextoChaves = "x {0}"; // Ex: "x 3"
    [SerializeField] private string formatoTextoMaterias = "{0} / {1}"; // Ex: "2 / 2"
    
    private PlayerMoviment player;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Procura o player na cena
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerMoviment>();
            
            if (player != null)
            {
                Debug.Log("✅ GameHUD encontrou o Player!");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerMoviment não encontrado!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameObject com tag 'Player' não encontrado!");
        }
        
        // Atualiza a UI inicial
        AtualizarHUD();
    }
    
    void Update()
    {
        if (atualizarAutomaticamente)
        {
            AtualizarHUD();
        }
    }
    
    /// <summary>
    /// Atualiza todos os contadores do HUD
    /// </summary>
    public void AtualizarHUD()
    {
        AtualizarContadorVidas();
        
        if (player == null) return;
        
        AtualizarContadorChaves();
        AtualizarContadorMaterias();
    }
    
    void AtualizarContadorVidas()
    {
        // Pega as vidas do GameManager
        if (GameManager.Instance == null) return;
        
        int vidas = GameManager.Instance.GetVidas();
        
        // Atualiza o texto
        if (textoVidas != null)
        {
            textoVidas.text = string.Format(formatoTextoVidas, vidas);
        }
        
        // Muda cor baseado nas vidas
        if (textoVidas != null)
        {
            if (vidas <= 1)
            {
                textoVidas.color = Color.red; // 1 vida = PERIGO!
            }
            else if (vidas == 2)
            {
                textoVidas.color = Color.yellow; // 2 vidas = Atenção
            }
            else
            {
                textoVidas.color = Color.white; // 3 vidas = Saudável
            }
        }
    }
    
    void AtualizarContadorChaves()
    {
        if (player == null) return;
        
        int chaves = player.GetChaves();
        
        // Atualiza o texto
        if (textoChaves != null)
        {
            textoChaves.text = string.Format(formatoTextoChaves, chaves);
        }
    }
    
    void AtualizarContadorMaterias()
    {
        if (player == null) return;
        
        int coletadas = player.GetMateriasColetadas();
        int necessarias = player.GetMateriasNecessarias();
        
        // Atualiza o texto
        if (textoMaterias != null)
        {
            textoMaterias.text = string.Format(formatoTextoMaterias, coletadas, necessarias);
        }
        
        // Muda cor se completou
        if (textoMaterias != null)
        {
            if (coletadas >= necessarias)
            {
                textoMaterias.color = Color.green; // Verde quando completa
            }
            else
            {
                textoMaterias.color = Color.white; // Branco quando falta
            }
        }
    }
    
    /// <summary>
    /// Força atualização manual do HUD
    /// Chame quando pegar/usar chave ou matéria
    /// </summary>
    public void ForcarAtualizacao()
    {
        AtualizarHUD();
    }
    
    /// <summary>
    /// Mostra/Esconde o HUD
    /// </summary>
    public void MostrarHUD(bool mostrar)
    {
        if (painelVidas != null)
            painelVidas.SetActive(mostrar);
        
        if (painelChaves != null)
            painelChaves.SetActive(mostrar);
        
        if (painelMaterias != null)
            painelMaterias.SetActive(mostrar);
    }
}

