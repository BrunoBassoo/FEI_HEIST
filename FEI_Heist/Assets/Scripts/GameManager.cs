using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Manager - Controla tudo relacionado ao jogo:
/// - Troca de cenas
/// - Estado do jogo
/// - Salvamento de progresso
/// - Gerenciamento de fase atual
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton - Só existe 1 Game Manager no jogo
    public static GameManager Instance { get; private set; }
    
    [Header("Controle de Cenas")]
    [SerializeField] private string cenaInicial = "TelaInicial";
    [SerializeField] private string cenaGameOver = "TelaDerrota";
    [SerializeField] private string cenaVitoria = "TelaVitoria";
    
    // Variável para guardar a fase atual (para poder reiniciar)
    private string faseAtual = "";
    
    void Awake()
    {
        // Implementa o padrão Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Não destrói ao trocar de cena
            Debug.Log("🎮 Game Manager inicializado!");
        }
        else
        {
            Destroy(gameObject); // Se já existe outro, destrói este
            return;
        }
    }
    
    void Start()
    {
        // Salva a cena inicial como fase atual
        faseAtual = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Fase atual: {faseAtual}");
    }
    
    // ==================== MÉTODOS PÚBLICOS ====================
    
    // ========== NAVEGAÇÃO DE MENU ==========
    
    /// <summary>
    /// Volta para o menu inicial
    /// </summary>
    public void LoadMainMenu()
    {
        Debug.Log("🏠 Voltando ao menu inicial");
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaInicial);
    }
    
    /// <summary>
    /// Carrega a tela de história (do menu)
    /// </summary>
    public void LoadHistory()
    {
        Debug.Log("📖 Carregando tela de história");
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaHistoria");
    }
    
    /// <summary>
    /// Carrega a tela de instruções (do menu)
    /// </summary>
    public void LoadInstructions()
    {
        Debug.Log("📋 Carregando tela de instruções");
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaInstrucoes");
    }
    
    /// <summary>
    /// Inicia o jogo (carrega a primeira fase)
    /// </summary>
    public void StartGame()
    {
        Debug.Log("🎮 Iniciando o jogo - Primeira fase");
        faseAtual = "fase F"; // Salva como fase atual
        Time.timeScale = 1f;
        SceneManager.LoadScene("fase F");
    }
    
    // ========== CONTROLE DE FASES ==========
    
    /// <summary>
    /// Carrega uma cena específica e salva como fase atual
    /// </summary>
    public void LoadLevel(string nomeCena)
    {
        Debug.Log($"🎮 Carregando cena: {nomeCena}");
        faseAtual = nomeCena;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeCena);
    }
    
    /// <summary>
    /// Reinicia a fase atual
    /// </summary>
    public void RestartCurrentLevel()
    {
        Debug.Log($"🔄 Reiniciando fase: {faseAtual}");
        Time.timeScale = 1f; // Garante que o jogo não está pausado
        SceneManager.LoadScene(faseAtual);
    }
    
    /// <summary>
    /// Carrega tela de Game Over e permite voltar para a mesma fase
    /// </summary>
    public void LoadGameOver()
    {
        Debug.Log($"💀 Game Over! Fase atual salva: {faseAtual}");
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaGameOver);
    }
    
    /// <summary>
    /// Volta para a fase onde o player morreu (chamado da tela de Game Over)
    /// </summary>
    public void RetryLevel()
    {
        Debug.Log($"🔄 Tentando novamente: {faseAtual}");
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(faseAtual))
        {
            SceneManager.LoadScene(faseAtual);
        }
        else
        {
            Debug.LogWarning("⚠️ Nenhuma fase salva! Indo para tela inicial.");
            SceneManager.LoadScene(cenaInicial);
        }
    }
    
    /// <summary>
    /// Carrega a tela de vitória
    /// </summary>
    public void LoadVictory()
    {
        Debug.Log($"🏆 Vitória! Fase completada: {faseAtual}");
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaVitoria);
    }
    
    /// <summary>
    /// Sai do jogo
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("👋 Saindo do jogo...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // ==================== GETTERS ====================
    
    /// <summary>
    /// Retorna o nome da fase atual
    /// </summary>
    public string GetCurrentLevel()
    {
        return faseAtual;
    }
}

