using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configurações de Cor")]
    [Tooltip("Cor original do texto")]
    public Color corOriginal = Color.white;
    
    [Tooltip("Cor quando o mouse estiver em cima")]
    public Color corHover = Color.black;
    
    [Header("Configurações de Transição")]
    [Tooltip("Velocidade da transição de cor (0 = instantâneo)")]
    [Range(0f, 10f)]
    public float velocidadeTransicao = 0f;
    
    private Text textoLegacy;
    private Color corAlvo;
    private bool mouseEmCima = false;

    void Start()
    {
        // Obtém o componente Text (Legacy)
        textoLegacy = GetComponent<Text>();
        
        if (textoLegacy == null)
        {
            Debug.LogError("TextHoverEffect: Componente Text não encontrado! Certifique-se de adicionar este script em um objeto com componente Text.");
            enabled = false;
            return;
        }
        
        // Define a cor inicial
        corOriginal = textoLegacy.color;
        corAlvo = corOriginal;
    }

    void Update()
    {
        // Transição suave de cor
        if (velocidadeTransicao > 0)
        {
            textoLegacy.color = Color.Lerp(textoLegacy.color, corAlvo, Time.deltaTime * velocidadeTransicao);
        }
        else
        {
            textoLegacy.color = corAlvo;
        }
    }

    // Chamado quando o mouse entra na área do texto
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEmCima = true;
        corAlvo = corHover;
    }

    // Chamado quando o mouse sai da área do texto
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEmCima = false;
        corAlvo = corOriginal;
    }
    
    // Método público para mudar a cor original (útil se você quiser mudar via código)
    public void DefinirCorOriginal(Color novaCor)
    {
        corOriginal = novaCor;
        if (!mouseEmCima)
        {
            corAlvo = corOriginal;
        }
    }
    
    // Método público para mudar a cor de hover (útil se você quiser mudar via código)
    public void DefinirCorHover(Color novaCor)
    {
        corHover = novaCor;
        if (mouseEmCima)
        {
            corAlvo = corHover;
        }
    }
}

