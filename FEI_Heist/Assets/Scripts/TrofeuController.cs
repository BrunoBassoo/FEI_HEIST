using System.Collections;
using UnityEngine;

public class TrofeuController : MonoBehaviour
{
    [Header("Configuração da Próxima Fase")]
    [Tooltip("Nome EXATO da próxima cena (ex: 'fase E', 'fase T', 'TelaVitoria')")]
    [SerializeField] private string nomeDaProximaFase = "";
    
    [Header("Configurações de Transição")]
    [Tooltip("Se true, espera a música terminar. Se false, usa o tempo fixo")]
    [SerializeField] private bool esperarMusicaTerminar = true;
    
    [Tooltip("Tempo de espera antes de carregar (usado se não tiver música)")]
    [SerializeField] private float tempoAntesDeCarregar = 2f;
    
    [Tooltip("Se true, mostra mensagem no Console")]
    [SerializeField] private bool mostrarMensagemVitoria = true;
    
    [Header("Som (Opcional)")]
    [SerializeField] private AudioClip somTrofeu;
    private AudioSource audioSource;
    
    private bool jaColetado = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Se não tem AudioSource, cria um
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            Debug.Log("✅ AudioSource criado automaticamente no trofeu");
        }
        
        // Verifica se tem som configurado
        if (somTrofeu != null)
        {
            Debug.Log($"✅ Trofeu '{gameObject.name}' tem som configurado: {somTrofeu.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Trofeu '{gameObject.name}' não tem som configurado!");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(">>> Trofeu: Trigger detectado com " + other.gameObject.name + " | Tag: " + other.tag);
        
        // Verifica se foi o player que encostou
        if (other.CompareTag("Player") && !jaColetado)
        {
            Debug.Log(">>> Trofeu: É o Player!");
            
            // Pega o script do player para verificar se coletou as matérias
            PlayerMoviment player = other.GetComponent<PlayerMoviment>();
            
            if (player != null)
            {
                int coletadas = player.GetMateriasColetadas();
                int necessarias = player.GetMateriasNecessarias();
                
                Debug.Log($">>> Trofeu: Matérias coletadas: {coletadas}/{necessarias}");
                
                // Verifica se o player coletou todas as matérias necessárias
                if (coletadas >= necessarias)
                {
                    Debug.Log(">>> Trofeu: Todas as matérias coletadas! Pode pegar o trofeu!");
                    ColetarTrofeu();
                }
                else
                {
                    // Não coletou todas as matérias ainda
                    int faltam = necessarias - coletadas;
                    Debug.LogWarning($"⚠️ Você precisa coletar {faltam} matéria(s) antes de pegar o trofeu!");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerMoviment não encontrado! Coletando trofeu sem verificar matérias.");
                // Se não tem PlayerMoviment, coleta direto
                ColetarTrofeu();
            }
        }
        else if (!other.CompareTag("Player"))
        {
            Debug.Log(">>> Trofeu: Não é o Player, ignorando.");
        }
        else if (jaColetado)
        {
            Debug.Log(">>> Trofeu: Já foi coletado!");
        }
    }
    
    void ColetarTrofeu()
    {
        jaColetado = true;
        
        if (mostrarMensagemVitoria)
        {
            Debug.Log("🏆 PARABÉNS! Você completou a fase!");
        }
        
        // Toca o som se tiver
        if (audioSource != null && somTrofeu != null)
        {
            audioSource.PlayOneShot(somTrofeu);
            Debug.Log($"🔊 Tocando som do trofeu: {somTrofeu.name} (duração: {somTrofeu.length}s)");
        }
        else
        {
            Debug.LogWarning("⚠️ Não foi possível tocar som: AudioSource ou Som Trofeu está null");
        }
        
        // Desativa o sprite (opcional - para "coletar" visualmente)
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.enabled = false;
        }
        
        // Desativa o collider para não coletar 2x
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Carrega a próxima fase
        StartCoroutine(CarregarProximaFase());
    }
    
    IEnumerator CarregarProximaFase()
    {
        // Decide se espera a música ou usa tempo fixo
        if (esperarMusicaTerminar && somTrofeu != null)
        {
            // Espera a música do trofeu terminar
            float duracaoMusica = somTrofeu.length;
            Debug.Log($"🎵 Aguardando música do trofeu terminar... ({duracaoMusica:F1}s)");
            yield return new WaitForSeconds(duracaoMusica);
        }
        else
        {
            // Usa tempo fixo configurado
            if (tempoAntesDeCarregar > 0)
            {
                Debug.Log($"⏳ Aguardando {tempoAntesDeCarregar}s antes de carregar próxima fase...");
                yield return new WaitForSeconds(tempoAntesDeCarregar);
            }
        }
        
        // Verifica se configurou o nome da próxima fase
        if (string.IsNullOrEmpty(nomeDaProximaFase))
        {
            Debug.LogError("❌ ERRO: Campo 'Nome Da Proxima Fase' está VAZIO!");
            Debug.LogError("📝 SOLUÇÃO: Selecione o Trofeu → Inspector → Configure o nome da próxima cena");
            Debug.LogError("💡 Exemplos: 'fase E', 'fase T', 'TelaVitoria'");
            yield break;
        }
        
        // Garante que o tempo está normal
        Time.timeScale = 1f;
        
        Debug.Log($"🎮 Música finalizada! Carregando próxima fase: '{nomeDaProximaFase}'");
        
        // USA O GAME MANAGER para carregar a próxima fase
        if (GameManager.Instance != null)
        {
            // Verifica se é a tela de vitória (última fase)
            if (nomeDaProximaFase.Contains("Vitoria") || nomeDaProximaFase.Contains("vitoria"))
            {
                Debug.Log("🏆 Última fase completada! Indo para tela de vitória!");
                GameManager.Instance.CompletarJogo();
            }
            else
            {
                // Fase intermediária - mantém as vidas
                int vidasAtuais = GameManager.Instance.GetVidas();
                Debug.Log($"✅ Fase completada! Vidas mantidas: {vidasAtuais}");
                GameManager.Instance.CompletarFaseEProxima(nomeDaProximaFase);
            }
        }
        else
        {
            Debug.LogError("❌ ERRO: GameManager não encontrado!");
            Debug.LogError("📝 SOLUÇÃO: Crie um GameObject 'GameManager' na cena TelaInicial com o script GameManager.cs");
        }
    }
}

