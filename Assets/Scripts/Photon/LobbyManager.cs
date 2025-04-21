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

    private Dictionary<PlayerRef, string> playerNames = new();

    public override void Spawned()
    {
        Debug.Log("🚀 LobbyManager Spawned() called.");

        if (Instance == null)
            Instance = this;

        // Don't do anything UI-related here yet
    }

    public void InitializeLobby()
    {
        Debug.Log("✅ playerGrid now assigned in InitializeLobby");

        if (Object.HasInputAuthority)
        {
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"📝 Submitting Steam name: {steamName}");
            RPC_SubmitName(steamName);
        }
    }

    public void RegisterPlayer(PlayerRef player)
    {
        Debug.Log($"➕ RegisterPlayer called with: {player}");

        if (!playerNames.ContainsKey(player))
        {
            playerNames.Add(player, "Loading...");
        }

        RefreshLobbyUI();
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        Debug.Log($"➖ UnregisterPlayer called with: {player}");

        if (playerNames.ContainsKey(player))
        {
            playerNames.Remove(player);
        }

        RefreshLobbyUI();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(string steamName, RpcInfo info = default)
    {
        var sender = info.Source;
        Debug.Log($"✅ Received name from {sender}: {steamName}");

        playerNames[sender] = steamName;
        RefreshLobbyUI();
    }

    private void RefreshLobbyUI()
    {
        Debug.Log("🔄 Refreshing Lobby UI...");

        if (playerGrid == null)
        {
            Debug.LogError("❌ playerGrid is still null!");
            return;
        }

        foreach (Transform child in playerGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var kvp in playerNames)
        {
            Debug.Log($"🎮 Spawning player slot: {kvp.Value}");
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
