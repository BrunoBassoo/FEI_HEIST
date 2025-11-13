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
            
            // Inicializa as vidas no Awake (antes de qualquer Start)
            vidasAtuais = vidasIniciais;
        }
        else
        {
            // Se já existe uma instância, destrói este GameObject
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        faseAtual = SceneManager.GetActiveScene().name;
        // NÃO reseta vidas no Start - só no Awake e nos métodos específicos
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
        }
    }
    
    // ========== NAVEGAÇÃO ==========
    
    public void LoadMainMenu()
    {
        // RESETA vidas ao voltar para o menu principal
        vidasAtuais = vidasIniciais;
        Time.timeScale = 1f;
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
        // FORÇA reset de vidas quando inicia do menu
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
    
    public void CompletarFaseEProxima(string proximaFase)
    {
        faseAtual = proximaFase;
        Time.timeScale = 1f;
        SceneManager.LoadScene(proximaFase);
    }
    
    public void CompletarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaVitoria);
    }
    
    public void CarregarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }
    
    // ========== SISTEMA DE VIDAS E DERROTA ==========
    
    public void PlayerCapturado()
    {
        // VALIDAÇÃO: Não permite vidas ficarem negativas
        if (vidasAtuais <= 0)
        {
            return;
        }
        
        Time.timeScale = 1f;
        vidasAtuais--;
        
        if (vidasAtuais > 0)
        {
            // Ainda tem vidas, reinicia a MESMA fase
            if (!string.IsNullOrEmpty(faseAtual))
            {
                SceneManager.LoadScene(faseAtual);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            // Sem vidas, Game Over
            vidasAtuais = 0; // Garante que fica em 0 (não negativo)
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
        // RESETA VIDAS quando reinicia a fase
        vidasAtuais = vidasIniciais;
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
            faseAtual = "fase F";
            SceneManager.LoadScene("fase F");
        }
    }
    
    public void JogarNovamente()
    {
        vidasAtuais = vidasIniciais;
        faseAtual = "fase F";
        Time.timeScale = 1f;
        SceneManager.LoadScene(faseAtual);
    }
    
    public void ResetarVidas()
    {
        vidasAtuais = vidasIniciais;
    }
    
    // ========== GETTERS E SETTERS ==========
    
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
    
    public void SetVidas(int vidas)
    {
        vidasAtuais = Mathf.Max(0, vidas);
    }
    
    public void AdicionarVidas(int quantidade)
    {
        if (quantidade > 0)
        {
            vidasAtuais += quantidade;
            vidasAtuais = Mathf.Min(vidasAtuais, vidasIniciais);
        }
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
