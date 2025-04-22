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

    private NetworkDictionary<int, string> playerNames = new();

    public override void Spawned()
    {
        Instance = this;

        Debug.Log("✅ LobbyManager Spawned");

        if (playerGrid == null)
        {
            Debug.Log("⚠️ Waiting for NetworkManager to assign playerGrid...");
        }

        // Check if this is our local player and we're the server
        if (Runner.IsServer)
        {
            Debug.Log("📝 Registering host player");
            RegisterPlayer(Runner.LocalPlayer);
        }

        if (playerGrid != null)
        {
            Debug.Log("✅ playerGrid dynamically assigned by NetworkManager.");
        }

        RefreshLobbyUI();
    }

    public void RegisterPlayer(PlayerRef player)
    {
        int key = player.RawEncoded;

        if (!playerNames.ContainsKey(key))
        {
            playerNames.Add(key, "Loading...");
            Debug.Log($"➕ Placeholder added for player {key}");
        }

        if (player == Runner.LocalPlayer && Object.HasInputAuthority)
        {
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"📤 Submitting name for {steamName} ({key})");
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
            Debug.Log($"🗑️ Removed player {key}");
        }

        RefreshLobbyUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(int playerId, string steamName, RpcInfo info = default)
    {
        Debug.Log($"✅ Received name: {steamName} for ID {playerId}");

        if (playerNames.ContainsKey(playerId))
            playerNames[playerId] = steamName;
        else
            playerNames.Add(playerId, steamName);

        RefreshLobbyUI();
    }

    private void RefreshLobbyUI()
    {
        if (playerGrid == null)
        {
            Debug.LogWarning("⚠️ playerGrid not assigned. Skipping UI refresh.");
            return;
        }

        Debug.Log("🔄 Refreshing Lobby UI");

        foreach (Transform child in playerGrid)
            Destroy(child.gameObject);

        try
        {
            foreach (var kvp in playerNames)
            {
                var slot = Instantiate(playerSlotPrefab, playerGrid);
                slot.GetComponentInChildren<TextMeshProUGUI>().text = kvp.Value;
            }
        }
        catch (System.InvalidOperationException)
        {
            Debug.LogWarning("⚠️ Skipped UI refresh: NetworkDictionary not ready.");
            return;
        }

        int emptySlots = 8 - playerNames.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
        }
    }

    public void AssignPlayerGrid(Transform grid)
    {
        playerGrid = grid;
        Debug.Log("✅ playerGrid assigned via AssignPlayerGrid.");
    }
}
