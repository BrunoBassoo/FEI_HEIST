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
    
    [Header("Sistema de Vidas")]
    [SerializeField] private int vidasIniciais = 3;
    
    // Variável para guardar a fase atual (para poder reiniciar)
    private string faseAtual = "";
    
    // Variável para guardar as vidas do player
    private int vidasAtuais = 3;
    
    // Flag para impedir múltiplos cliques durante transições
    private bool estaCarregando = false;
    
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
        
        // Inicializa as vidas
        vidasAtuais = vidasIniciais;
        
        Debug.Log($"📍 Fase atual: {faseAtual}");
        Debug.Log($"❤️ Vidas: {vidasAtuais}/{vidasIniciais}");
    }
    
    // ==================== MÉTODOS PÚBLICOS ====================
    
    // ========== NAVEGAÇÃO DE MENU ==========
    
    /// <summary>
    /// Volta para o menu inicial
    /// </summary>
    public void LoadMainMenu()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log("🏠 Voltando ao menu inicial");
        estaCarregando = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaInicial);
    }
    
    /// <summary>
    /// Carrega a tela de história (do menu)
    /// </summary>
    public void LoadHistory()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log("📖 Carregando tela de história");
        estaCarregando = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaHistoria");
    }
    
    /// <summary>
    /// Carrega a tela de instruções (do menu)
    /// </summary>
    public void LoadInstructions()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log("📋 Carregando tela de instruções");
        estaCarregando = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaInstrucoes");
    }
    
    /// <summary>
    /// Inicia o jogo (carrega a primeira fase)
    /// </summary>
    public void StartGame()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log("🎮 Iniciando o jogo - Primeira fase");
        
        // RESETA AS VIDAS ao começar novo jogo
        vidasAtuais = vidasIniciais;
        Debug.Log($"❤️ Vidas resetadas: {vidasAtuais}/{vidasIniciais}");
        
        faseAtual = "fase F"; // Salva como fase atual
        Time.timeScale = 1f;
        estaCarregando = true;
        
        // Inicia o jogo com música
        StartCoroutine(CarregarCenaComMusica("fase F"));
    }
    
    // ========== CONTROLE DE FASES ==========
    
    /// <summary>
    /// Carrega uma cena específica e salva como fase atual
    /// </summary>
    public void LoadLevel(string nomeCena)
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log($"🎮 Carregando cena: {nomeCena}");
        faseAtual = nomeCena;
        Time.timeScale = 1f;
        estaCarregando = true;
        
        // Garante que a música volta ao carregar nova fase
        StartCoroutine(CarregarCenaComMusica(nomeCena));
    }
    
    IEnumerator CarregarCenaComMusica(string nomeCena)
    {
        // Carrega a cena
        SceneManager.LoadScene(nomeCena);
        
        // Espera a cena carregar
        yield return new WaitForSeconds(0.5f);
        
        // Garante que a música de fundo toca
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.TocarMusicaDeFundo();
            Debug.Log("🎵 Música de fundo iniciada na nova fase!");
        }
        
        // Libera para novas transições
        estaCarregando = false;
    }
    
    /// <summary>
    /// Reinicia a fase atual
    /// </summary>
    public void RestartCurrentLevel()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log($"🔄 Reiniciando fase: {faseAtual}");
        Time.timeScale = 1f; // Garante que o jogo não está pausado
        estaCarregando = true;
        
        // Reinicia com música
        StartCoroutine(ReiniciarFaseComMusica());
    }
    
    /// <summary>
    /// Chamado quando o inimigo captura o player
    /// Perde 1 vida e decide: reiniciar fase ou game over
    /// </summary>
    public void PlayerCapturado()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        vidasAtuais--;
        
        Debug.Log($"💔 Player capturado! Perdeu 1 vida. Vidas restantes: {vidasAtuais}/{vidasIniciais}");
        
        Time.timeScale = 1f;
        estaCarregando = true;
        
        // Se ainda tem vidas, reinicia a fase
        if (vidasAtuais > 0)
        {
            Debug.Log($"🔄 Reiniciando fase '{faseAtual}' com {vidasAtuais} vida(s) restante(s)");
            
            // Garante que a música de fundo vai tocar ao reiniciar
            StartCoroutine(ReiniciarFaseComMusica());
        }
        else
        {
            // Sem vidas, vai para tela de derrota
            Debug.Log("💀 SEM VIDAS! Game Over!");
            SceneManager.LoadScene(cenaGameOver);
        }
    }
    
    IEnumerator ReiniciarFaseComMusica()
    {
        // Carrega a cena
        SceneManager.LoadScene(faseAtual);
        
        // Espera a cena carregar
        yield return new WaitForSeconds(0.5f);
        
        // Garante que a música de fundo volta
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.TocarMusicaDeFundo();
            Debug.Log("🎵 Música de fundo retomada após reiniciar fase!");
        }
        
        // Libera para novas transições
        estaCarregando = false;
    }
    
    /// <summary>
    /// [LEGADO] Carrega tela de Game Over diretamente (ignora sistema de vidas)
    /// </summary>
    public void LoadGameOver()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log($"💀 Game Over direto! (sem usar sistema de vidas)");
        estaCarregando = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaGameOver);
    }
    
    /// <summary>
    /// Volta para a fase onde o player morreu (chamado da tela de Game Over)
    /// RESETA AS VIDAS ao tentar novamente
    /// </summary>
    public void RetryLevel()
    {
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log($"🔄 Tentando novamente: {faseAtual}");
        
        // RESETA AS VIDAS ao tentar novamente
        vidasAtuais = vidasIniciais;
        Debug.Log($"❤️ Vidas resetadas: {vidasAtuais}/{vidasIniciais}");
        
        Time.timeScale = 1f;
        estaCarregando = true;
        
        if (!string.IsNullOrEmpty(faseAtual))
        {
            // Reinicia com música
            StartCoroutine(ReiniciarFaseComMusica());
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
        if (estaCarregando) return; // Bloqueia se já estiver carregando
        
        Debug.Log($"🏆 Vitória! Fase completada: {faseAtual}");
        estaCarregando = true;
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
    
    // ==================== SISTEMA DE VIDAS ====================
    
    /// <summary>
    /// Retorna quantas vidas o player tem atualmente
    /// </summary>
    public int GetVidas()
    {
        return vidasAtuais;
    }
    
    /// <summary>
    /// Retorna o número máximo de vidas
    /// </summary>
    public int GetVidasMaximas()
    {
        return vidasIniciais;
    }
    
    /// <summary>
    /// Reseta as vidas para o valor inicial
    /// </summary>
    public void ResetarVidas()
    {
        vidasAtuais = vidasIniciais;
        Debug.Log($"❤️ Vidas resetadas: {vidasAtuais}/{vidasIniciais}");
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

