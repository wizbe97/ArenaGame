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

    // ✅ Use a NetworkDictionary so it's synced across all clients
    private NetworkDictionary<PlayerRef, string> playerNames = new();

    public override void Spawned()
    {
        Debug.Log("✅ LobbyManager Spawned");

        if (Instance == null)
            Instance = this;

        if (Object.HasStateAuthority)
        {
            Debug.Log("📝 Registering host player");
            RegisterPlayer(Runner.LocalPlayer);
        }
    }

    public void RegisterPlayer(PlayerRef player)
    {
        if (!playerNames.ContainsKey(player))
        {
            playerNames.Add(player, "Loading...");
            Debug.Log($"➕ Added placeholder for {player}");
        }

        // Submit Steam name if this is the local player
        if (player == Runner.LocalPlayer && Object.HasInputAuthority)
        {
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"📤 Submitting name for {player}: {steamName}");
            RPC_SubmitName(player, steamName);
        }

        RefreshLobbyUI();
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        if (playerNames.ContainsKey(player))
        {
            playerNames.Remove(player);
            Debug.Log($"🗑️ Removed player {player}");
        }

        RefreshLobbyUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(PlayerRef fromPlayer, string steamName, RpcInfo info = default)
    {
        Debug.Log($"✅ Received name from {fromPlayer}: {steamName}");

        if (playerNames.ContainsKey(fromPlayer))
            playerNames[fromPlayer] = steamName;

        RefreshLobbyUI();
    }

    private void RefreshLobbyUI()
    {
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
