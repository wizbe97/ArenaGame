using Fusion;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [Networked] private NetworkDictionary<int, NetworkString<_32>> playerNames => default;

    [HideInInspector] public Transform playerGrid;
    public GameObject playerSlotPrefab;

    public override void Spawned()
    {
        if (Instance == null)
            Instance = this;

        Debug.Log("✅ LobbyManager Spawned");

        // Assign grid at runtime from NetworkManager (if available)
        if (NetworkManager.Instance != null)
        {
            playerGrid = NetworkManager.Instance.playerGrid;
            Debug.Log("✅ playerGrid dynamically assigned by NetworkManager.");
        }

        if (Runner.IsServer)
        {
            Debug.Log("📝 Registering host player");
            RegisterPlayer(Runner.LocalPlayer);
        }
    }

    public void RegisterPlayer(PlayerRef player)
    {
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
        Debug.Log($"✅ Received name for player ID {playerId}: {steamName}");

        if (playerNames.ContainsKey(playerId))
            playerNames.Set(playerId, steamName); // Proper setter to avoid CS1612
        else
            playerNames.Add(playerId, steamName);

        RefreshLobbyUI();
    }

    private void RefreshLobbyUI()
    {
        if (playerGrid == null)
        {
            Debug.LogError("❌ playerGrid is not assigned.");
            return;
        }

        Debug.Log("🔄 Refreshing Lobby UI");

        foreach (Transform child in playerGrid)
            Destroy(child.gameObject);

        foreach (var kvp in playerNames)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = kvp.Value.ToString();
        }

        int emptySlots = 8 - playerNames.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
        }
    }
}
