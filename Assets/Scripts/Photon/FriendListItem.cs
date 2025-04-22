using UnityEngine;
using TMPro;
using Steamworks;

public class FriendListItem : MonoBehaviour
{
    public TextMeshProUGUI friendNameText;
    private CSteamID friendID;

    public void Setup(string name, CSteamID id)
    {
        friendNameText.text = name;
        friendID = id;
    }

    public void OnJoinClicked()
    {
        NetworkManager.Instance.JoinLobby(friendID.ToString());
    }

    public void OnInviteClicked()
    {
        SteamFriends.ActivateGameOverlayInviteDialog(friendID);
    }
}
