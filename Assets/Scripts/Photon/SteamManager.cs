using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static bool Initialized { get; private set; }

    private void Awake()
    {
        if (Initialized)
            return;

        if (SteamAPI.RestartAppIfNecessary((AppId_t)480))
        {
            Application.Quit();
            return;
        }

        if (!SteamAPI.Init())
        {
            Debug.LogError("SteamAPI.Init() failed. Is Steam running?");
            Application.Quit();
            return;
        }

        Initialized = true;
        DontDestroyOnLoad(gameObject);
        Debug.Log("Steam initialized as: " + SteamFriends.GetPersonaName());
    }

    private void Update()
    {
        if (Initialized)
            SteamAPI.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        if (Initialized)
            SteamAPI.Shutdown();
    }
}
