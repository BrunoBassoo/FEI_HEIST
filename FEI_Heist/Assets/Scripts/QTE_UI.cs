using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI simples para o QTE do inimigo
/// Mostra "Aperte [E] para sair" e o progresso
/// Usa Text (Legacy) ao invés de TextMeshPro
/// </summary>
public class QTE_UI : MonoBehaviour
{
    public static QTE_UI Instance { get; private set; }
    
    [Header("UI Elements")]
    [SerializeField] private GameObject painelQTE;
    [SerializeField] private Text textoInstrucao;
    [SerializeField] private Text textoProgresso;
    [SerializeField] private Text textoTempo;
    [SerializeField] private Image imagemQTE; // Imagem decorativa (logo, ícone, etc)
    
    [Header("Configurações")]
    [SerializeField] private string mensagemAntes = "APERTE "; // Texto antes da tecla
    [SerializeField] private string mensagemDepois = " PARA SAIR!"; // Texto depois da tecla
    [SerializeField] private bool usarEfeitoPulsacao = true; // Se true, texto de instrução pulsa
    
    private EnemyAI inimigoAtual;
    private bool qteAtivo = false;
    
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
        // Esconde o painel no início
        if (painelQTE != null)
        {
            painelQTE.SetActive(false);
        }
    }
    
    void Update()
    {
        if (!qteAtivo || inimigoAtual == null) return;
        
        // Verifica se o QTE ainda está ativo
        if (!inimigoAtual.EstaEmQTE())
        {
            EsconderUI();
            return;
        }
        
        // Atualiza a UI
        AtualizarUI();
    }
    
    public void MostrarUI(EnemyAI inimigo)
    {
        inimigoAtual = inimigo;
        qteAtivo = true;
        
        if (painelQTE != null)
        {
            painelQTE.SetActive(true);
        }
        
        // Monta o texto com a tecla em VERMELHO
        if (textoInstrucao != null)
        {
            // Pega a tecla configurada no inimigo
            KeyCode tecla = inimigo.GetTeclaEscape();
            string nomeTecla = tecla.ToString();
            
            // Monta texto com Rich Text (tecla em vermelho)
            string textoCompleto = mensagemAntes + "<color=red>[" + nomeTecla + "]</color>" + mensagemDepois;
            textoInstrucao.text = textoCompleto;
            
            Debug.Log($"🎮 UI do QTE ativada! Tecla: {nomeTecla}");
        }
    }
    
    public void EsconderUI()
    {
        qteAtivo = false;
        inimigoAtual = null;
        
        if (painelQTE != null)
        {
            painelQTE.SetActive(false);
        }
        
        Debug.Log("🎮 UI do QTE desativada!");
    }
    
    void AtualizarUI()
    {
        if (inimigoAtual == null) return;
        
        // Pega dados do inimigo
        int apertosAtuais = inimigoAtual.GetApertosAtuais();
        int apertosNecessarios = inimigoAtual.GetApertosNecessarios();
        float tempoRestante = inimigoAtual.GetTempoRestanteQTE();
        
        // Atualiza APENAS OS NÚMEROS do texto de progresso
        // Cores e fontes são configuradas no Unity Inspector
        if (textoProgresso != null)
        {
            textoProgresso.text = $"{apertosAtuais} / {apertosNecessarios}";
        }
        
        // Atualiza APENAS O NÚMERO do tempo restante
        // Cores e fontes são configuradas no Unity Inspector
        if (textoTempo != null)
        {
            textoTempo.text = $"{tempoRestante:F1}s";
        }
        
        // Efeito de pulsação (opcional - desmarque no Inspector se não quiser)
        if (usarEfeitoPulsacao && textoInstrucao != null)
        {
            float escala = 1f + Mathf.Sin(Time.time * 8f) * 0.15f;
            textoInstrucao.transform.localScale = Vector3.one * escala;
        }
    }
}

