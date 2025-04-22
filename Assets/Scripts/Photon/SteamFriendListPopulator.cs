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

        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

            if (SteamFriends.GetFriendPersonaState(friendID) == EPersonaState.k_EPersonaStateOffline)
                continue;

            if (SteamFriends.GetFriendGamePlayed(friendID, out FriendGameInfo_t gameInfo))
            {
                if (gameInfo.m_gameID.AppID() != new AppId_t(480))
                    continue; // Wrong game
            }
            else
            {
                continue; // Not in a game
            }

            string friendName = SteamFriends.GetFriendPersonaName(friendID);
            GameObject item = Instantiate(friendListItemPrefab, friendListContainer);
            FriendListItem listItem = item.GetComponent<FriendListItem>();
            listItem.Setup(friendName, friendID);
        }
    }
}
