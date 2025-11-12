using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper para conectar botões ao GameManager automaticamente
/// </summary>
public class ButtonHelper : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Nome da cena que será carregada ao clicar")]
    public string nomeDaCena;
    
    private Button botao;
    
    void Start()
    {
        botao = GetComponent<Button>();
        
        if (botao == null)
        {
            Debug.LogError("ButtonHelper: Não encontrou componente Button!");
            return;
        }
        
        // Conecta o botão à função de carregar cena
        botao.onClick.AddListener(CarregarCena);
    }
    
    void CarregarCena()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("ButtonHelper: GameManager não encontrado!");
            return;
        }
        
        if (string.IsNullOrEmpty(nomeDaCena))
        {
            Debug.LogWarning("ButtonHelper: Nome da cena não definido!");
            return;
        }
        
        GameManager.Instance.CarregarCena(nomeDaCena);
    }
}
