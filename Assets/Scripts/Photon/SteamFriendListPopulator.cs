using UnityEngine;
using Steamworks;

public class SteamFriendListPopulator : MonoBehaviour
{
    public GameObject friendListItemPrefab;
    public Transform friendListContainer;

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized!");
            return;
        }

        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        Debug.Log($"🧑‍🤝‍🧑 Found {friendCount} friends.");

        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            string friendName = SteamFriends.GetFriendPersonaName(friendID);

            Debug.Log($"📝 Friend {i}: {friendName} ({friendID})");

            GameObject item = Instantiate(friendListItemPrefab, friendListContainer);
            FriendListItem listItem = item.GetComponent<FriendListItem>();

            if (listItem != null)
            {
                listItem.Setup(friendName, friendID);
            }
            else
            {
                Debug.LogWarning("⚠️ FriendListItem script not found on prefab!");
            }
        }
    }
}
