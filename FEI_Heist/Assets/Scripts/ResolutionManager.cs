using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    [Header("Resolução Padrão")]
    [SerializeField] private int largura = 1920;
    [SerializeField] private int altura = 1080;
    
    [Header("Modo de Tela")]
    [SerializeField] private bool telaCheia = true;
    [SerializeField] private FullScreenMode modoTelaCheia = FullScreenMode.FullScreenWindow;
    
    [Header("Opções Avançadas")]
    [Tooltip("Se true, força a resolução mesmo se o monitor não suportar")]
    [SerializeField] private bool forcarResolucao = true;
    
    [Tooltip("Taxa de atualização (0 = padrão do monitor)")]
    [SerializeField] private int taxaAtualizacao = 60;
    
    void Awake()
    {
        ConfigurarResolucao();
    }
    
    void ConfigurarResolucao()
    {
        if (forcarResolucao)
        {
            // Força a resolução especificada
            Screen.SetResolution(largura, altura, telaCheia ? modoTelaCheia : FullScreenMode.Windowed, taxaAtualizacao);
            Debug.Log($"✅ Resolução forçada: {largura}x{altura} | Tela Cheia: {telaCheia}");
        }
        else
        {
            // Usa a resolução nativa do monitor em tela cheia
            if (telaCheia)
            {
                Resolution resolucaoNativa = Screen.currentResolution;
                Screen.SetResolution(resolucaoNativa.width, resolucaoNativa.height, modoTelaCheia, resolucaoNativa.refreshRate);
                Debug.Log($"✅ Resolução nativa: {resolucaoNativa.width}x{resolucaoNativa.height}");
            }
            else
            {
                // Modo janela com resolução personalizada
                Screen.SetResolution(largura, altura, FullScreenMode.Windowed);
                Debug.Log($"✅ Modo janela: {largura}x{altura}");
            }
        }
    }
    
    // Métodos públicos para mudar resolução em runtime (ex: menu de opções)
    public void MudarResolucao(int novaLargura, int novaAltura, bool novaTelaCheia)
    {
        largura = novaLargura;
        altura = novaAltura;
        telaCheia = novaTelaCheia;
        ConfigurarResolucao();
    }
    
    public void AlternarTelaCheia()
    {
        telaCheia = !telaCheia;
        ConfigurarResolucao();
    }
}

