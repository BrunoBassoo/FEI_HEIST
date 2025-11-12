using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemManager : MonoBehaviour
{
    private static EventSystemManager instance;

    void Awake()
    {
        // Se já existe uma instância, destrói esta
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Define como instância única e não destrói ao trocar de cena
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Garante que tem um EventSystem
        if (GetComponent<EventSystem>() == null)
        {
            gameObject.AddComponent<EventSystem>();
        }

        // Garante que tem um StandaloneInputModule
        if (GetComponent<StandaloneInputModule>() == null)
        {
            gameObject.AddComponent<StandaloneInputModule>();
        }
    }
}

