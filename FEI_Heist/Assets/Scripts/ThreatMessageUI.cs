using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThreatMessageUI : MonoBehaviour
{
    [Header("Referências UI")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private UnityEngine.UI.Image barraProgresso; // Opcional
    
    [Header("Configurações")]
    [SerializeField] private string mensagem = "Vou te pegar, moleque!!";
    [SerializeField] private Color corTexto = Color.red;
    [SerializeField] private bool mostrarTimer = true;
    
    private EnemyAI[] inimigos;
    private bool mostrandoMensagem = false;
    private EnemyAI inimigoSegurando = null;

    void Start()
    {
        // Procura todos os inimigos na cena
        inimigos = FindObjectsOfType<EnemyAI>();
        
        // Esconde a mensagem no início
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        
        // Configura o texto
        if (messageText != null)
        {
            messageText.text = mensagem;
            messageText.color = corTexto;
        }
        
        Debug.Log("ThreatMessageUI iniciado. Inimigos encontrados: " + inimigos.Length);
    }

    void Update()
    {
        bool algumInimigoTocando = false;
        inimigoSegurando = null;
        
        // Verifica se algum inimigo está tocando o player
        foreach (EnemyAI inimigo in inimigos)
        {
            if (inimigo != null && inimigo.EstaTocandoPlayer())
            {
                algumInimigoTocando = true;
                inimigoSegurando = inimigo;
                break;
            }
        }
        
        // Mostra ou esconde a mensagem
        if (algumInimigoTocando && !mostrandoMensagem)
        {
            MostrarMensagem();
        }
        else if (!algumInimigoTocando && mostrandoMensagem)
        {
            EsconderMensagem();
        }
        
        // Atualiza o timer se estiver mostrando
        if (mostrandoMensagem && inimigoSegurando != null && mostrarTimer)
        {
            AtualizarTimer();
        }
    }
    
    void MostrarMensagem()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
            mostrandoMensagem = true;
            
            // Efeito de pulsação
            StartCoroutine(EfeitoPulsacao());
        }
    }
    
    void EsconderMensagem()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
            mostrandoMensagem = false;
            StopAllCoroutines();
        }
    }
    
    IEnumerator EfeitoPulsacao()
    {
        while (mostrandoMensagem)
        {
            if (messageText != null)
            {
                float escala = 1f + Mathf.Sin(Time.time * 5f) * 0.15f;
                messageText.transform.localScale = Vector3.one * escala;
            }
            yield return null;
        }
        
        // Reseta a escala
        if (messageText != null)
        {
            messageText.transform.localScale = Vector3.one;
        }
    }
    
    void AtualizarTimer()
    {
        if (inimigoSegurando == null) return;
        
        float tempoRestante = inimigoSegurando.GetTempoParaCapturar() - inimigoSegurando.GetTempoSegurandoPlayer();
        float progresso = inimigoSegurando.GetProgressoCaptura();
        
        // Atualiza o texto com o timer (se configurado para mostrar)
        if (messageText != null && mostrarTimer)
        {
            messageText.text = $"{mensagem}\n⏱️ {tempoRestante:F1}s";
            
            // Muda a cor conforme o perigo aumenta
            if (progresso < 0.33f)
            {
                messageText.color = Color.yellow;
            }
            else if (progresso < 0.66f)
            {
                messageText.color = new Color(1f, 0.5f, 0f); // Laranja
            }
            else
            {
                messageText.color = Color.red;
            }
        }
        else if (messageText != null)
        {
            // Só mostra a mensagem sem timer
            messageText.text = mensagem;
            messageText.color = corTexto;
        }
        
        // Atualiza a barra de progresso (se existir)
        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = progresso;
            
            // Muda a cor da barra
            if (progresso < 0.33f)
            {
                barraProgresso.color = Color.yellow;
            }
            else if (progresso < 0.66f)
            {
                barraProgresso.color = new Color(1f, 0.5f, 0f); // Laranja
            }
            else
            {
                barraProgresso.color = Color.red;
            }
        }
    }
}

