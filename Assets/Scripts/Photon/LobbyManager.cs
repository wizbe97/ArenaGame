using Fusion;
using Steamworks;
using TMPro;
using UnityEngine;

public class LobbyManager : NetworkBehaviour {
    public static LobbyManager Instance;

    public Transform playerGrid;
    public GameObject playerSlotPrefab;

    [Networked]
    private NetworkDictionary<int, NetworkString<_32>> playerNames => default;

    public override void Spawned() {
        Instance = this;

        if (playerGrid == null) {
            Debug.LogWarning("⚠️ playerGrid not yet assigned.");
            return;
        }

        Debug.Log("✅ LobbyManager Spawned");

        if (HasStateAuthority) {
            RegisterPlayer(Runner.LocalPlayer);
        }
    }

    public void RegisterPlayer(PlayerRef player) {
        int id = player.RawEncoded;

        if (HasStateAuthority && !playerNames.ContainsKey(id)) {
            playerNames.Add(id, "Loading...");
        }

        if (player == Runner.LocalPlayer && Object.HasInputAuthority) {
            string name = SteamFriends.GetPersonaName();
            Debug.Log($"📤 Submitting name for {player}: {name}");
            RPC_SubmitName(id, name);
        }

        RefreshLobbyUI();
    }

    public void UnregisterPlayer(PlayerRef player) {
        int id = player.RawEncoded;

        if (HasStateAuthority && playerNames.ContainsKey(id)) {
            playerNames.Remove(id);
            Debug.Log($"🗑 Removed player {player}");
        }

        RefreshLobbyUI();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(int id, string name) {
        if (!playerNames.ContainsKey(id)) {
            playerNames.Add(id, name);
        } else {
            var updated = playerNames.Get(id);
            updated = name;  // Just reassign the value
            playerNames.Set(id, updated);  // Explicitly use Set
        }

        Debug.Log($"✅ Updated name for player ID {id}: {name}");
        RefreshLobbyUI();
    }

    public void RefreshLobbyUI() {
        if (playerGrid == null) {
            Debug.LogWarning("⚠️ playerGrid not assigned. Skipping UI refresh.");
            return;
        }

        foreach (Transform child in playerGrid) {
            Destroy(child.gameObject);
        }

        foreach (var kvp in playerNames) {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = kvp.Value.ToString();
        }

        for (int i = playerNames.Count; i < 8; i++) {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
        }

        Debug.Log("🔄 Lobby UI refreshed");
    }
}
