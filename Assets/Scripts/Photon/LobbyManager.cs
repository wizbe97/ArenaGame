using Fusion;
using Steamworks;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    public Transform playerGrid;
    public GameObject playerSlotPrefab;

    [Networked, Capacity(8)]
    private NetworkDictionary<int, NetworkString<_32>> playerNames => default;


    private Dictionary<int, string> queuedNames = new(); // For clients to retry name submission

    public override void Spawned()
    {
        Instance = this;

        if (playerGrid == null)
        {
            Debug.LogWarning("⚠️ playerGrid not assigned.");
            return;
        }

        Debug.Log("✅ LobbyManager Spawned");

        if (HasStateAuthority)
        {
            RegisterPlayer(Runner.LocalPlayer);
        }

        RefreshLobbyUI();
    }

    public void RegisterPlayer(PlayerRef player)
    {
        int id = player.RawEncoded;

        if (HasStateAuthority && !playerNames.ContainsKey(id))
        {
            playerNames.Add(id, "Loading...");
        }

        if (player == Runner.LocalPlayer && Object.HasInputAuthority)
        {
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"📤 Submitting name for {player}: {steamName}");
            RPC_SubmitName(id, steamName);
        }

        RefreshLobbyUI();
    }


    private void TrySubmitName(int id, string name)
    {
        if (HasStateAuthority || !Object.HasInputAuthority) return;

        if (playerNames.ContainsKey(id))
        {
            RPC_SubmitName(id, name);
        }
        else
        {
            Debug.LogWarning("🕒 Dictionary not ready. Queuing name...");
            queuedNames[id] = name;
            Invoke(nameof(RetryQueuedNames), 0.5f);
        }
    }

    private void RetryQueuedNames()
    {
        foreach (var pair in queuedNames)
        {
            if (playerNames.ContainsKey(pair.Key))
            {
                Debug.Log("🔁 Retrying name submit...");
                RPC_SubmitName(pair.Key, pair.Value);
                queuedNames.Remove(pair.Key);
                break;
            }
        }

        if (queuedNames.Count > 0)
        {
            Invoke(nameof(RetryQueuedNames), 0.5f);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(int id, string name)
    {
        if (!playerNames.ContainsKey(id))
        {
            playerNames.Add(id, name);
        }
        else
        {
            playerNames.Set(id, name);
        }

        Debug.Log($"✅ Updated name for player ID {id}: {name}");
        RefreshLobbyUI();
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        int id = player.RawEncoded;

        if (HasStateAuthority && playerNames.ContainsKey(id))
        {
            playerNames.Remove(id);
            Debug.Log($"🗑 Removed player {player}");
        }

        RefreshLobbyUI();
    }

    public void RefreshLobbyUI()
    {
        if (playerGrid == null)
        {
            Debug.LogWarning("⚠️ playerGrid not assigned. Skipping UI refresh.");
            return;
        }

        foreach (Transform child in playerGrid)
        {
            Destroy(child.gameObject);
        }

        List<int> sortedKeys = new();
        foreach (var kvp in playerNames)
        {
            sortedKeys.Add(kvp.Key);
        }
        sortedKeys.Sort();

        foreach (int id in sortedKeys)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = playerNames[id].ToString();
        }

        for (int i = playerNames.Count; i < 8; i++)
        {
            var slot = Instantiate(playerSlotPrefab, playerGrid);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
        }

        Debug.Log("🔄 Lobby UI refreshed");
    }

}
