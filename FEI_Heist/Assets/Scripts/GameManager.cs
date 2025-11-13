using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Cenas")]
    [SerializeField] private string cenaInicial = "TelaInicial";
    [SerializeField] private string cenaGameOver = "TelaDerrota";
    [SerializeField] private string cenaVitoria = "TelaVitoria";
    
    [Header("Sistema de Vidas")]
    [SerializeField] private int vidasIniciais = 3;
    
    private string faseAtual = "";
    private int vidasAtuais = 3;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        faseAtual = SceneManager.GetActiveScene().name;
        vidasAtuais = vidasIniciais;
        
        Debug.Log($"🎮 GameManager iniciado | Fase: {faseAtual} | Vidas: {vidasAtuais}/{vidasIniciais}");
    }
    
    void OnEnable()
    {
        // Atualiza a fase atual sempre que uma cena é carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Só atualiza faseAtual se for uma fase de jogo (não menu/game over)
        if (!scene.name.Contains("Tela") && !scene.name.Contains("Menu"))
        {
            faseAtual = scene.name;
            Debug.Log($"📍 Fase atual atualizada: {faseAtual}");
        }
    }
    
    // ========== NAVEGAÇÃO ==========
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(cenaInicial);
    }
    
    public void LoadHistory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaHistoria");
    }
    
    public void LoadInstructions()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TelaInstrucoes");
    }
    
    public void StartGame()
    {
        vidasAtuais = vidasIniciais;
        faseAtual = "fase F";
        Time.timeScale = 1f;
        SceneManager.LoadScene("fase F");
    }
    
    public void LoadLevel(string nomeCena)
    {
        faseAtual = nomeCena;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeCena);
    }
    
    public void CarregarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }
    
    // ========== SISTEMA DE VIDAS E DERROTA ==========
    
    public void PlayerCapturado()
    {
        vidasAtuais--;
        Time.timeScale = 1f;
        
        Debug.Log($"💔 PLAYER CAPTURADO! Vidas: {vidasAtuais}/{vidasIniciais}");
        
        if (vidasAtuais > 0)
        {
            // Ainda tem vidas, reinicia a MESMA fase
            Debug.Log($"♻️ Reiniciando fase '{faseAtual}' | Vidas restantes: {vidasAtuais}");
            
            if (!string.IsNullOrEmpty(faseAtual))
            {
                SceneManager.LoadScene(faseAtual);
            }
            else
            {
                // Fallback: recarrega a cena atual se faseAtual não estiver definida
                Debug.LogWarning("⚠️ faseAtual vazia! Recarregando cena atual...");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            // Sem vidas, Game Over
            Debug.Log($"☠️ GAME OVER! Sem vidas restantes. Indo para tela de derrota...");
            SceneManager.LoadScene(cenaGameOver);
        }
    }
    
    public void LoadGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaGameOver);
    }
    
    public void LoadVictory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaVitoria);
    }
    
    // ========== RESET DE FASES ==========
    
    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(faseAtual);
    }
    
    public void RetryLevel()
    {
        vidasAtuais = vidasIniciais;
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(faseAtual))
        {
            SceneManager.LoadScene(faseAtual);
        }
        else
        {
            SceneManager.LoadScene(cenaInicial);
        }
    }
    
    public void ResetarVidas()
    {
        vidasAtuais = vidasIniciais;
    }
    
    // ========== GETTERS ==========
    
    public int GetVidas()
    {
        return vidasAtuais;
    }
    
    public int GetVidasMaximas()
    {
        return vidasIniciais;
    }
    
    public string GetCurrentLevel()
    {
        return faseAtual;
    }
    
    // ========== OUTROS ==========
    
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
