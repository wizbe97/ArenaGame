using UnityEngine;
using Fusion;
using Fusion.Sockets;
using Steamworks;
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
        else Destroy(gameObject);
    }

    public async void StartHost()
    {
        if (_runner != null) return;

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

        // Inside StartHost (after spawn)
        var obj = _runner.Spawn(lobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        LobbyManager lobby = obj.GetComponent<LobbyManager>();
        lobby.AssignPlayerGrid(playerGrid); // 🔧 Assign dynamically
        lobbyPanel.SetActive(true);

        // Inside JoinLobby (in client, after lobbyPanel.SetActive)
        StartCoroutine(WaitAndAssignGrid());


    }
    private System.Collections.IEnumerator WaitAndAssignGrid()
    {
        while (LobbyManager.Instance == null)
            yield return null;

        LobbyManager.Instance.AssignPlayerGrid(playerGrid);
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
            return;
        }

        lobbyPanel.SetActive(true);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"🔹 OnPlayerJoined: {player}");
        LobbyManager.Instance?.RegisterPlayer(player);
    }

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
