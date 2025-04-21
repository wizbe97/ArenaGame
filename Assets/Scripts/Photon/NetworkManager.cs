using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;

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

        GameObject runnerObj = Instantiate(networkRunnerPrefab);
        _runner = runnerObj.GetComponent<NetworkRunner>();
        _runner.name = "NetworkRunner";
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = "MyLobby",
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError($"❌ Failed to start host: {result.ShutdownReason}");
            return;
        }

        // ✅ Spawn and assign scene references
        NetworkObject spawnedObj = _runner.Spawn(lobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        LobbyManager spawnedLobby = spawnedObj.GetComponent<LobbyManager>();
        spawnedLobby.playerGrid = playerGrid; // assign grid reference
        spawnedLobby.InitializeLobby();       // now it’s safe to call RPC and UI


        Debug.Log("✅ LobbyManager spawned and playerGrid assigned");

        lobbyPanel.SetActive(true);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"➕ OnPlayerJoined triggered! PlayerRef: {player}");

        // Avoid registering self; name will be submitted via RPC from Spawned()
        if (player != runner.LocalPlayer)
        {
            Debug.Log("➡️ Registering other player in lobby...");
            LobbyManager.Instance?.RegisterPlayer(player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"❌ Player left: {player}");
        LobbyManager.Instance?.UnregisterPlayer(player);
    }


    public async void JoinLobby(string sessionName)
    {
        if (_runner != null) return;

        GameObject runnerObj = Instantiate(networkRunnerPrefab);
        _runner = runnerObj.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.name = "NetworkRunner_Join";
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            lobbyPanel.SetActive(true);
            Debug.Log("✅ Joined lobby successfully!");
        }
        else
        {
            Debug.LogError("❌ Failed to join lobby: " + result.ShutdownReason);
        }
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        request.Accept();
    }

    // Other unused INetworkRunnerCallbacks
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
