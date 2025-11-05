using UnityEngine;

/// <summary>
/// Script opcional para testar o sistema de música
/// Adicione em um objeto vazio na cena para ter controles de teste
/// </summary>
public class MusicTestHelper : MonoBehaviour
{
    [Header("Atalhos de Teste (apenas no Editor)")]
    [SerializeField] private KeyCode teclaPausarMusica = KeyCode.M; // M = Music
    [SerializeField] private KeyCode teclaAumentarVolume = KeyCode.Plus;
    [SerializeField] private KeyCode teclaDiminuirVolume = KeyCode.Minus;
    
    void Update()
    {
        #if UNITY_EDITOR
        if (MusicManager.Instance == null) return;
        
        // Pausar/Continuar música de fundo
        if (Input.GetKeyDown(teclaPausarMusica))
        {
            if (MusicManager.Instance.EstaTocando())
            {
                MusicManager.Instance.PausarMusicaDeFundo();
                Debug.Log("🎵 [TESTE] Música pausada");
            }
            else
            {
                MusicManager.Instance.ContinuarMusicaDeFundo();
                Debug.Log("🎵 [TESTE] Música continuada");
            }
        }
        
        // Aumentar volume
        if (Input.GetKeyDown(teclaAumentarVolume))
        {
            float novoVolume = MusicManager.Instance.GetVolume() + 0.1f;
            MusicManager.Instance.SetVolume(novoVolume);
            Debug.Log($"🎵 [TESTE] Volume: {novoVolume:F1}");
        }
        
        // Diminuir volume
        if (Input.GetKeyDown(teclaDiminuirVolume))
        {
            float novoVolume = MusicManager.Instance.GetVolume() - 0.1f;
            MusicManager.Instance.SetVolume(novoVolume);
            Debug.Log($"🎵 [TESTE] Volume: {novoVolume:F1}");
        }
        
        // Menu de ajuda
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("=== CONTROLES DE MÚSICA ===");
            Debug.Log($"[{teclaPausarMusica}] - Pausar/Continuar música de fundo");
            Debug.Log($"[{teclaAumentarVolume}] - Aumentar volume");
            Debug.Log($"[{teclaDiminuirVolume}] - Diminuir volume");
            Debug.Log("[F2] - Mostrar esta ajuda");
            Debug.Log("===========================");
        }
        #endif
    }
}

