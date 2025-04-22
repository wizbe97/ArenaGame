using Fusion;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    public Transform playerGrid;
    public GameObject playerSlotPrefab;

    [Networked] private NetworkDictionary<int, string> playerNames => default;

    public override void Spawned()
    {
        if (Instance == null)
            Instance = this;

        Debug.Log("✅ LobbyManager Spawned");

        if (playerGrid == null && NetworkManager.Instance != null)
        {
            playerGrid = NetworkManager.Instance.playerGrid;
            Debug.Log("✅ playerGrid dynamically assigned by NetworkManager.");
        }

        if (Runner.IsServer)
        {
            Debug.Log("📝 Registering host player");
            RegisterPlayer(Runner.LocalPlayer);
        }

        RefreshLobbyUI();
    }

    public void RegisterPlayer(PlayerRef player)
    {
        if (!HasStateAuthority) return;

        int key = player.RawEncoded;

        if (!playerNames.ContainsKey(key))
        {
            playerNames.Add(key, "Loading...");
            Debug.Log($"➕ Added placeholder for Player {player}");
        }

        if (player == Runner.LocalPlayer && Object.HasInputAuthority)
        {
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"📤 Submitting name for {player}: {steamName}");
            RPC_SubmitName(key, steamName);
        }

        RefreshLobbyUI();
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        if (!HasStateAuthority) return;

        int key = player.RawEncoded;
        if (playerNames.ContainsKey(key))
        {
            playerNames.Remove(key);
            Debug.Log($"🗑️ Removed player {player}");
        }

        RefreshLobbyUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(int playerId, string steamName, RpcInfo info = default)
    {
        if (!playerNames.ContainsKey(playerId))
            playerNames.Add(playerId, steamName);
        else
            playerNames.Set(playerId, steamName);

        Debug.Log($"✅ Received name for player ID {playerId}: {steamName}");
        RPC_RefreshLobbyUI(); // Push updated UI to all clients
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RefreshLobbyUI()
    {
        RefreshLobbyUI();
    }

    private void RefreshLobbyUI()
    {
        if (playerGrid == null)
        {
            Debug.LogWarning("⚠️ playerGrid is null, cannot update UI");
            return;
        }

        Debug.Log("🔄 Refreshing Lobby UI");

        foreach (Transform child in playerGrid)
            Destroy(child.gameObject);

        foreach (var kvp in playerNames)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = kvp.Value;
        }

        int emptySlots = 8 - playerNames.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
        }
    }
}
