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
        Debug.Log("✅ LobbyManager Spawned");

        if (Instance == null)
            Instance = this;

    }


    public void RegisterPlayer(PlayerRef player)
    {
        Debug.Log($"➕ RegisterPlayer called for: {player}");

        if (!playerNames.ContainsKey(player))
        {
            playerNames[player] = "Loading...";
            Debug.Log("🟡 Added placeholder entry to name dictionary.");
        }

        // If this is the local player, submit Steam name to host
        if (player == Runner.LocalPlayer)
        {
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"📝 Submitting local Steam name: {steamName}");

            RPC_SubmitName(player, steamName);
        }

        RefreshLobbyUI();
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        Debug.Log($"❌ UnregisterPlayer called for: {player}");

        if (playerNames.ContainsKey(player))
        {
            playerNames.Remove(player);
            Debug.Log("🗑️ Removed from name dictionary.");
        }

        RefreshLobbyUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(PlayerRef fromPlayer, string steamName, RpcInfo info = default)
    {
        Debug.Log($"✅ Received name from {fromPlayer}: {steamName}");

        playerNames[fromPlayer] = steamName;
        RefreshLobbyUI();
    }

    public void RefreshLobbyUI()
    {
        Debug.Log("🔄 Refreshing Lobby UI...");

        foreach (Transform child in playerGrid)
            Destroy(child.gameObject);

        foreach (var kvp in playerNames)
        {
            Debug.Log($"👤 Spawning player slot: {kvp.Value}");
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
