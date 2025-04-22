using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
	private static bool _initialized;
	public static bool Initialized => _initialized;

	private void Awake()
	{
		if (_initialized)
			return;

		// Optional: disable this during local testing
		// if (SteamAPI.RestartAppIfNecessary((AppId_t)480))
		// {
		// 	Application.Quit();
		// 	return;
		// }

		if (!SteamAPI.Init())
		{
			Debug.LogError("SteamAPI.Init() failed. Is Steam running?");
			Application.Quit();
			return;
		}

		_initialized = true;
		DontDestroyOnLoad(gameObject);
		Debug.Log("Steam initialized as: " + SteamFriends.GetPersonaName());
	}

	private void Update()
	{
		if (_initialized)
			SteamAPI.RunCallbacks();
	}

	private void OnApplicationQuit()
	{
		if (_initialized)
			SteamAPI.Shutdown();
	}
}
