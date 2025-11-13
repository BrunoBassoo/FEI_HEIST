using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class CleanupTool : MonoBehaviour
{
    [MenuItem("Tools/Limpar Missing Scripts de Todos os GameObjects")]
    static void CleanupMissingScripts()
    {
        int removidos = 0;
        
        // Procura em todos os GameObjects da cena
        GameObject[] objetos = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in objetos)
        {
            int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            if (count > 0)
            {
                Debug.Log($"✅ Removido {count} script(s) faltando de: {obj.name}");
                removidos += count;
            }
        }
        
        if (removidos > 0)
        {
            Debug.Log($"🎉 Total: {removidos} script(s) faltando removidos!");
        }
        else
        {
            Debug.Log("✅ Nenhum script faltando encontrado!");
        }
    }
    
    [MenuItem("Tools/Encontrar GameObjects com TextHoverEffect")]
    static void FindTextHoverEffects()
    {
        TextHoverEffect[] scripts = FindObjectsOfType<TextHoverEffect>();
        
        if (scripts.Length == 0)
        {
            Debug.Log("Nenhum TextHoverEffect encontrado na cena.");
            return;
        }
        
        Debug.Log($"📝 Encontrados {scripts.Length} TextHoverEffect(s):");
        
        foreach (TextHoverEffect script in scripts)
        {
            bool temText = script.GetComponent<UnityEngine.UI.Text>() != null;
            
            if (temText)
            {
                Debug.Log($"✅ {script.gameObject.name} - OK (tem Text component)");
            }
            else
            {
                Debug.LogError($"❌ {script.gameObject.name} - ERRO (SEM Text component)", script.gameObject);
            }
        }
    }
}
#endif

