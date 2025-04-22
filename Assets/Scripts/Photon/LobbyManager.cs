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
        }

        if (player == Runner.LocalPlayer && Object.HasInputAuthority)
        {
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"📝 Submitting local Steam name: {steamName}");

            RPC_SubmitName(player, steamName);
        }

        RefreshLobbyUI();
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        if (playerNames.Remove(player))
        {
            Debug.Log($"❌ Player removed: {player}");
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
        foreach (Transform child in playerGrid)
            Destroy(child.gameObject);

        foreach (var entry in playerNames)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = entry.Value;
        }

        int empty = 8 - playerNames.Count;
        for (int i = 0; i < empty; i++)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
        }
    }
}
