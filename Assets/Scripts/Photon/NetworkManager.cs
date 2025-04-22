using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using Steamworks;

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void StartHost()
    {
        if (_runner != null) return;
        lobbyPanel.SetActive(true);

        string steamID = SteamUser.GetSteamID().ToString();
        Debug.Log($"🟢 Hosting with Session Name: {steamID}");

        GameObject runnerObj = Instantiate(networkRunnerPrefab);
        _runner = runnerObj.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = steamID,
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError("❌ Failed to host: " + result.ShutdownReason);
            return;
        }

        // Spawn lobby manager
        NetworkObject spawnedObj = _runner.Spawn(lobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        LobbyManager spawnedLobby = spawnedObj.GetComponent<LobbyManager>();
        // ✅ After assigning playerGrid and calling RefreshLobbyUI()
        spawnedLobby.playerGrid = playerGrid;
        Debug.Log("✅ LobbyManager spawned and playerGrid assigned");

        spawnedLobby.RefreshLobbyUI();

        // ✅ Safe place to register player after playerGrid is ready
        Debug.Log($"📝 Registering local player from NetworkManager: {_runner.LocalPlayer}");
        spawnedLobby.RegisterPlayer(_runner.LocalPlayer);
    }

    public async void JoinLobby(string steamID)
    {
        if (_runner != null) return;

        Debug.Log($"🔵 Joining Lobby with Session: {steamID}");

        GameObject runnerObj = Instantiate(networkRunnerPrefab);
        _runner = runnerObj.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = steamID,
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError("❌ Failed to join lobby: " + result.ShutdownReason);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) =>
        LobbyManager.Instance?.RegisterPlayer(player);

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) =>
        LobbyManager.Instance?.UnregisterPlayer(player);

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) => request.Accept();

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
