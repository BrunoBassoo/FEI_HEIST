using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    
    [Header("UI do Pause")]
    [SerializeField] private GameObject painelPause;
    
    [Header("Imagem do Pause (Opcional)")]
    [Tooltip("Imagem/Logo para mostrar no painel de pause")]
    [SerializeField] private Image imagemPause;
    
    [Header("Botões")]
    [SerializeField] private Button botaoVoltar;
    [SerializeField] private Button botaoReiniciar;
    [SerializeField] private Button botaoSair;
    
    private bool jogoPausado = false;
    
    void Awake()
    {
        // Singleton
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
        // Garante que o painel começa escondido
        if (painelPause != null)
        {
            painelPause.SetActive(false);
        }
        
        // Configura os botões
        if (botaoVoltar != null)
        {
            botaoVoltar.onClick.AddListener(Retomar);
        }
        
        if (botaoReiniciar != null)
        {
            botaoReiniciar.onClick.AddListener(ReiniciarFase);
        }
        
        if (botaoSair != null)
        {
            botaoSair.onClick.AddListener(VoltarAoMenu);
        }
    }
    
    void Update()
    {
        // Detecta tecla ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (jogoPausado)
            {
                Retomar(); // Se já está pausado, retoma
            }
            else
            {
                Pausar(); // Se não está pausado, pausa
            }
        }
    }
    
    public void Pausar()
    {
        if (painelPause == null)
        {
            Debug.LogError("PauseManager: Painel de Pause não está atribuído!");
            return;
        }
        
        jogoPausado = true;
        
        // PAUSA O JOGO COMPLETAMENTE
        Time.timeScale = 0f;
        
        // Mostra o painel de pause
        painelPause.SetActive(true);
        
        Debug.Log("⏸️ Jogo pausado!");
    }
    
    public void Retomar()
    {
        if (painelPause == null)
        {
            Debug.LogError("PauseManager: Painel de Pause não está atribuído!");
            return;
        }
        
        jogoPausado = false;
        
        // RETOMA O JOGO
        Time.timeScale = 1f;
        
        // Esconde o painel de pause
        painelPause.SetActive(false);
        
        Debug.Log("▶️ Jogo retomado!");
    }
    
    public void ReiniciarFase()
    {
        // Retoma o tempo antes de trocar de cena
        Time.timeScale = 1f;
        
        Debug.Log("🔄 Reiniciando fase...");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentLevel();
        }
        else
        {
            Debug.LogError("GameManager não encontrado!");
        }
    }
    
    public void VoltarAoMenu()
    {
        // Retoma o tempo antes de trocar de cena
        Time.timeScale = 1f;
        
        Debug.Log("🏠 Voltando para TelaInicial...");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu(); // Vai para TelaInicial
        }
        else
        {
            Debug.LogError("GameManager não encontrado!");
        }
    }
    
    public bool EstaPausado()
    {
        return jogoPausado;
    }
}

