using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sistema de UI que mostra quando o player está sendo capturado
/// Exibe painel de alerta, barra de progresso e timer
/// </summary>
public class CaptureWarningUI : MonoBehaviour
{
    [Header("Referências UI - Painel Principal")]
    [SerializeField] private GameObject painelCaptura;
    [SerializeField] private TextMeshProUGUI textoAlerta;
    [SerializeField] private TextMeshProUGUI textoTimer;
    
    [Header("Barra de Progresso")]
    [SerializeField] private Image barraProgresso;
    [SerializeField] private Image fundoBarra;
    
    [Header("Efeitos Visuais")]
    [SerializeField] private Image overlayVermelho; // Tela fica vermelha
    [SerializeField] private bool usarEfeitoPulsacao = true;
    [SerializeField] private bool usarOverlayVermelho = true;
    
    [Header("Configurações")]
    [SerializeField] private string mensagemAlerta = "SENDO CAPTURADO!";
    [SerializeField] private Color corInicial = Color.yellow;
    [SerializeField] private Color corMedio = new Color(1f, 0.5f, 0f); // Laranja
    [SerializeField] private Color corPerigo = Color.red;
    
    private EnemyAI[] inimigos;
    private EnemyAI inimigoSegurando = null;
    private bool mostrandoAviso = false;
    
    void Start()
    {
        // Procura todos os inimigos
        inimigos = FindObjectsOfType<EnemyAI>();
        
        // Esconde tudo no início
        if (painelCaptura != null)
        {
            painelCaptura.SetActive(false);
        }
        
        if (overlayVermelho != null)
        {
            overlayVermelho.gameObject.SetActive(false);
        }
        
        Debug.Log("CaptureWarningUI iniciado. Inimigos encontrados: " + inimigos.Length);
    }
    
    void Update()
    {
        bool algumInimigoSegurando = false;
        inimigoSegurando = null;
        
        // Verifica se algum inimigo está segurando o player
        foreach (EnemyAI inimigo in inimigos)
        {
            if (inimigo != null && inimigo.EstaTocandoPlayer())
            {
                algumInimigoSegurando = true;
                inimigoSegurando = inimigo;
                break;
            }
        }
        
        // Mostra ou esconde o aviso
        if (algumInimigoSegurando)
        {
            if (!mostrandoAviso)
            {
                MostrarAviso();
            }
            AtualizarAviso();
        }
        else
        {
            if (mostrandoAviso)
            {
                EsconderAviso();
            }
        }
    }
    
    void MostrarAviso()
    {
        mostrandoAviso = true;
        
        if (painelCaptura != null)
        {
            painelCaptura.SetActive(true);
        }
        
        if (overlayVermelho != null && usarOverlayVermelho)
        {
            overlayVermelho.gameObject.SetActive(true);
        }
        
        if (usarEfeitoPulsacao)
        {
            StartCoroutine(EfeitoPulsacao());
        }
        
        Debug.Log("⚠️ AVISO DE CAPTURA ATIVADO!");
    }
    
    void EsconderAviso()
    {
        mostrandoAviso = false;
        
        if (painelCaptura != null)
        {
            painelCaptura.SetActive(false);
        }
        
        if (overlayVermelho != null)
        {
            overlayVermelho.gameObject.SetActive(false);
        }
        
        StopAllCoroutines();
        
        Debug.Log("✓ Aviso de captura desativado - Player escapou!");
    }
    
    void AtualizarAviso()
    {
        if (inimigoSegurando == null) return;
        
        float tempoRestante = inimigoSegurando.GetTempoParaCapturar() - inimigoSegurando.GetTempoSegurandoPlayer();
        float progresso = inimigoSegurando.GetProgressoCaptura();
        
        // Atualiza o texto do timer
        if (textoTimer != null)
        {
            textoTimer.text = $"{tempoRestante:F1}s";
        }
        
        // Atualiza o texto de alerta
        if (textoAlerta != null)
        {
            textoAlerta.text = mensagemAlerta;
        }
        
        // Atualiza a barra de progresso
        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = progresso;
        }
        
        // Muda as cores conforme o perigo
        Color corAtual = GetCorPorProgresso(progresso);
        
        if (textoAlerta != null)
        {
            textoAlerta.color = corAtual;
        }
        
        if (textoTimer != null)
        {
            textoTimer.color = corAtual;
        }
        
        if (barraProgresso != null)
        {
            barraProgresso.color = corAtual;
        }
        
        // Atualiza o overlay vermelho (intensidade aumenta)
        if (overlayVermelho != null && usarOverlayVermelho)
        {
            Color overlayColor = overlayVermelho.color;
            overlayColor.a = progresso * 0.5f; // Máximo 50% de opacidade
            overlayVermelho.color = overlayColor;
        }
    }
    
    Color GetCorPorProgresso(float progresso)
    {
        if (progresso < 0.33f)
        {
            return corInicial; // Amarelo
        }
        else if (progresso < 0.66f)
        {
            return corMedio; // Laranja
        }
        else
        {
            return corPerigo; // Vermelho
        }
    }
    
    IEnumerator EfeitoPulsacao()
    {
        while (mostrandoAviso)
        {
            // Efeito de pulsação no texto
            if (textoAlerta != null)
            {
                float escala = 1f + Mathf.Sin(Time.time * 8f) * 0.2f; // Pulsação rápida
                textoAlerta.transform.localScale = Vector3.one * escala;
            }
            
            // Efeito de pulsação no painel
            if (painelCaptura != null && inimigoSegurando != null)
            {
                float progresso = inimigoSegurando.GetProgressoCaptura();
                
                // Aumenta velocidade de pulsação conforme se aproxima da captura
                float velocidade = 3f + (progresso * 7f); // De 3 a 10
                float intensidade = 0.1f + (progresso * 0.1f); // De 0.1 a 0.2
                
                float escala = 1f + Mathf.Sin(Time.time * velocidade) * intensidade;
                painelCaptura.transform.localScale = Vector3.one * escala;
            }
            
            yield return null;
        }
        
        // Reseta escalas
        if (textoAlerta != null)
        {
            textoAlerta.transform.localScale = Vector3.one;
        }
        
        if (painelCaptura != null)
        {
            painelCaptura.transform.localScale = Vector3.one;
        }
    }
}

