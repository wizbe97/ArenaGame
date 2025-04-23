using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    public static bool Initialized { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if !DISABLESTEAMWORKS
        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test failed.");
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test failed.");
        }

        try
        {
            if (!SteamAPI.Init())
            {
                Debug.LogError("SteamAPI_Init() failed.");
                Initialized = false;
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError($"[Steamworks.NET] Could not load Steam DLL: {e}");
            Initialized = false;
            return;
        }
#endif

        Initialized = true;
        Debug.Log($"Steam initialized as: {SteamFriends.GetPersonaName()}");
    }

    private void OnApplicationQuit()
    {
        if (Initialized)
        {
            SteamAPI.Shutdown();
        }
    }

    private void Update()
    {
        if (Initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }
}
