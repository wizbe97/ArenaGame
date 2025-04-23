using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using Steamworks;
using System;
using System.Collections;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;

    public GameObject networkRunnerPrefab;
    public GameObject lobbyPanel;
    public NetworkObject lobbyManagerPrefab;
    public Transform playerGrid;

    private NetworkRunner _runner;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(WaitForSteamAndStartHost());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator WaitForSteamAndStartHost()
    {
        // 🕒 Wait until Steam is ready
        while (!SteamManager.Initialized)
        {
            Debug.Log("⏳ Waiting for Steam to initialize...");
            yield return null;
        }

        Debug.Log("✅ Steam ready. Starting host.");
        StartHost();
    }

    public async void StartHost()
    {
        if (_runner != null) return;

        string steamID = SteamUser.GetSteamID().ToString();
        Debug.Log($"🟢 Hosting with Session Name: lobby_{steamID}");

        GameObject runnerObj = Instantiate(networkRunnerPrefab);
        _runner = runnerObj.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = $"lobby_{steamID}",
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError("❌ Failed to host: " + result.ShutdownReason);
            return;
        }

        var obj = _runner.Spawn(lobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        LobbyManager lobby = obj.GetComponent<LobbyManager>();
        lobby.playerGrid = playerGrid;
    }

    public void OnClickPlay()
    {
        lobbyPanel.SetActive(true);
        LobbyManager.Instance?.RefreshLobbyUI();
    }

    public async void JoinLobby(string steamID)
    {
        Debug.Log($"🔵 Joining lobby: lobby_{steamID}");

        GameObject runnerObj = Instantiate(networkRunnerPrefab);
        _runner = runnerObj.GetComponent<NetworkRunner>();
        _runner.ProvideInput = false;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = $"lobby_{steamID}",
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError($"❌ Failed to join: {result.ShutdownReason}");
            return;
        }

        lobbyPanel.SetActive(true);
    }

    // --- Fusion Callbacks ---

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"🔹 OnPlayerJoined: {player}");
        LobbyManager.Instance?.RegisterPlayer(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => LobbyManager.Instance?.UnregisterPlayer(player);
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) => request.Accept();
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
}
