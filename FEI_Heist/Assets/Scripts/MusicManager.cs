using UnityEngine;

/// <summary>
/// Gerencia a música de fundo do jogo
/// Pausa automaticamente quando outras músicas tocam e volta quando param
/// </summary>
public class MusicManager : MonoBehaviour
{
    // Singleton - só existe um MusicManager
    public static MusicManager Instance { get; private set; }
    
    [Header("Música de Fundo")]
    [SerializeField] private AudioClip musicaDeFundo;
    [SerializeField] private float volumeMusicaFundo = 0.3f;
    
    [Header("Configurações")]
    [SerializeField] private bool tocarAoIniciar = true;
    [SerializeField] private bool persistirEntreCenas = true;
    
    private AudioSource audioSource;
    private bool musicaDeFundoAtiva = true;
    private int contagemMusicasAtivas = 0; // Conta quantas músicas estão tocando
    
    void Awake()
    {
        // Implementa Singleton
        if (Instance == null)
        {
            Instance = this;
            
            // Persiste entre cenas se configurado
            if (persistirEntreCenas)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            Debug.Log("✓ MusicManager inicializado!");
        }
        else
        {
            // Já existe um MusicManager, destrói este
            Destroy(gameObject);
            return;
        }
        
        // Configura o AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configurações da música de fundo
        audioSource.clip = musicaDeFundo;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volumeMusicaFundo;
    }
    
    void Start()
    {
        // Toca a música de fundo ao iniciar se configurado
        if (tocarAoIniciar && musicaDeFundo != null)
        {
            TocarMusicaDeFundo();
        }
    }
    
    // ======================== MÚSICA DE FUNDO ========================
    
    /// <summary>
    /// Inicia a música de fundo
    /// </summary>
    public void TocarMusicaDeFundo()
    {
        if (audioSource != null && musicaDeFundo != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            musicaDeFundoAtiva = true;
            Debug.Log("🎵 Música de fundo iniciada!");
        }
    }
    
    /// <summary>
    /// Para a música de fundo completamente
    /// </summary>
    public void PararMusicaDeFundo()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            musicaDeFundoAtiva = false;
            Debug.Log("🎵 Música de fundo parada!");
        }
    }
    
    /// <summary>
    /// Pausa a música de fundo (pode continuar depois)
    /// </summary>
    public void PausarMusicaDeFundo()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("⏸️ Música de fundo pausada!");
        }
    }
    
    /// <summary>
    /// Continua a música de fundo de onde parou
    /// </summary>
    public void ContinuarMusicaDeFundo()
    {
        if (audioSource != null && !audioSource.isPlaying && musicaDeFundoAtiva)
        {
            audioSource.UnPause();
            Debug.Log("▶️ Música de fundo continuada!");
        }
    }
    
    // ======================== SISTEMA DE PRIORIDADE ========================
    
    /// <summary>
    /// Chame quando outra música começar (ex: música de perseguição)
    /// </summary>
    public void RegistrarMusicaAtiva()
    {
        contagemMusicasAtivas++;
        
        // Se é a primeira música ativa, pausa a música de fundo
        if (contagemMusicasAtivas == 1)
        {
            PausarMusicaDeFundo();
        }
    }
    
    /// <summary>
    /// Chame quando outra música parar
    /// </summary>
    public void DesregistrarMusicaAtiva()
    {
        contagemMusicasAtivas--;

        // Garante que não fica negativo
        if (contagemMusicasAtivas < 0)
        {
            Debug.LogWarning($"⚠️ ATENÇÃO! Contagem de músicas ficou negativa! Resetando para 0.");
            contagemMusicasAtivas = 0;
        }
        
        // Se não tem mais músicas ativas, volta a música de fundo
        if (contagemMusicasAtivas == 0)
        {
            ContinuarMusicaDeFundo();
            Debug.Log($"▶️ Música de fundo voltou (contagem = 0)");
        }
        else
        {
            Debug.Log($"⏸️ Música de fundo continua pausada (contagem = {contagemMusicasAtivas})");
        }
    }
    
    // ======================== CONTROLES DE VOLUME ========================
    
    /// <summary>
    /// Altera o volume da música de fundo
    /// </summary>
    public void SetVolume(float volume)
    {
        volumeMusicaFundo = Mathf.Clamp01(volume); // Garante entre 0 e 1
        if (audioSource != null)
        {
            audioSource.volume = volumeMusicaFundo;
        }
    }
    
    /// <summary>
    /// Troca a música de fundo
    /// </summary>
    public void TrocarMusicaDeFundo(AudioClip novaMusica)
    {
        if (novaMusica == null) return;
        
        bool estavaTocando = audioSource != null && audioSource.isPlaying;
        
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = novaMusica;
            musicaDeFundo = novaMusica;
            
            // Se estava tocando, toca a nova música
            if (estavaTocando)
            {
                audioSource.Play();
            }
        }
        
        Debug.Log("🎵 Música de fundo trocada!");
    }
    
    // ======================== MÉTODOS DE EMERGÊNCIA ========================
    
    /// <summary>
    /// Força a volta da música de fundo resetando a contagem
    /// Use apenas se a música de fundo parou de tocar por um bug
    /// </summary>
    public void ForcarVoltarMusicaDeFundo()
    {
        Debug.LogWarning("🚨 [EMERGÊNCIA] Forçando volta da música de fundo!");
        Debug.LogWarning($"   Contagem antes: {contagemMusicasAtivas}");
        
        contagemMusicasAtivas = 0;
        ContinuarMusicaDeFundo();
        
        Debug.LogWarning($"   Contagem resetada para: {contagemMusicasAtivas}");
        Debug.LogWarning($"   Música tocando? {EstaTocando()}");
    }
    
    // ======================== GETTERS ========================
    
    public bool EstaTocando()
    {
        return audioSource != null && audioSource.isPlaying;
    }
    
    public float GetVolume()
    {
        return volumeMusicaFundo;
    }
    
    public int GetMusicasAtivasContagem()
    {
        return contagemMusicasAtivas;
    }
    
}

