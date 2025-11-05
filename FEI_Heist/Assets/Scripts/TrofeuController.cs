using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrofeuController : MonoBehaviour
{
    [Header("Configuração da Próxima Fase")]
    [SerializeField] private string nomeDaProximaFase = "";
    
    [Header("Configurações de Transição")]
    [SerializeField] private float tempoAntesDeCarregar = 1f;
    [SerializeField] private bool mostrarMensagemVitoria = true;
    
    [Header("Som (Opcional)")]
    [SerializeField] private AudioClip somTrofeu;
    private AudioSource audioSource;
    
    private bool jaColetado = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
        Debug.Log($">>> Aguardando {tempoAntesDeCarregar} segundos antes de carregar...");
        
        // Espera um tempo antes de carregar
        yield return new WaitForSeconds(tempoAntesDeCarregar);
        
        // Verifica se o nome da fase foi configurado
        if (string.IsNullOrEmpty(nomeDaProximaFase))
        {
            Debug.LogError("❌ ERRO: Nome da próxima fase não foi configurado no Inspector do Trofeu!");
            Debug.LogError(">>> Configure o campo 'Nome Da Proxima Fase' no Inspector!");
            yield break;
        }
        
        Debug.Log($">>> Tentando carregar a fase: '{nomeDaProximaFase}'");
        
        // Garante que o tempo está normal
        Time.timeScale = 1f;
        
        // Tenta carregar a cena
        try
        {
            Debug.Log($"🎮 Carregando cena: {nomeDaProximaFase}");
            SceneManager.LoadScene(nomeDaProximaFase);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ ERRO ao carregar a fase '{nomeDaProximaFase}'!");
            Debug.LogError($"Mensagem de erro: {e.Message}");
            Debug.LogError(">>> Verifique:");
            Debug.LogError("1. O nome da fase está EXATAMENTE igual ao nome da Scene?");
            Debug.LogError("2. A Scene foi adicionada no Build Settings?");
            Debug.LogError("   (File → Build Settings → Add Open Scenes)");
        }
    }
}

