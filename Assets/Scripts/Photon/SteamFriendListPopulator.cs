using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class SteamFriendListPopulator : MonoBehaviour
{
    public GameObject friendListItemPrefab;
    public Transform friendListContainer;

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized.");
            return;
        }

        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            string friendName = SteamFriends.GetFriendPersonaName(friendID);

            GameObject friendItem = Instantiate(friendListItemPrefab, friendListContainer);

            // ✅ Find the first TextMeshProUGUI child anywhere in the prefab
            TextMeshProUGUI nameText = friendItem.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = friendName;
            else
                Debug.LogWarning("Friend name text not found in prefab!");

            // ✅ Find the first Button child anywhere in the prefab
            Button inviteButton = friendItem.GetComponentInChildren<Button>();
            if (inviteButton != null)
                inviteButton.onClick.AddListener(() => OnInviteButtonClicked(friendID));
            else
                Debug.LogWarning("InviteButton not found in friend prefab!");
        }
    }

    private void OnInviteButtonClicked(CSteamID friendID)
    {
        string name = SteamFriends.GetFriendPersonaName(friendID);
        Debug.Log("Invite clicked for: " + name);

        // TODO: Send Fusion/Photon invite or trigger internal joining logic here
    }
}
