using UnityEngine;
using Mirror;

public class NetworkRoomPlayerRev : NetworkRoomPlayer
{
    [SyncVar]
    public string player_Name;
    [SerializeField] private bool basicSetupDone;
    public override void OnClientEnterRoom()
    {
        if (Application.loadedLevel != 0) return;
        if (!isLocalPlayer) return;
        if (basicSetupDone) return;
        if (!NetworkClient.ready) Debug.Log("Not Ready Yet");
        base.OnClientEnterRoom();
        MainMenuButton.isPanelActive = false;
        setMainManagerObject();
        sendInitialInformations();
    }
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        if (!isLocalPlayer) return;
        if (Application.loadedLevel != 0) return;
        if (!NetworkClient.ready) return;
        base.ReadyStateChanged(oldReadyState, newReadyState);
        Debug.Log("Ready State Changed Log From: " + this.gameObject.name);
        MainLobbyUI.instance.refreshSlots();
    }
    private void setMainManagerObject()
    {
        if (LobbySlotManager.instance == null && isServer)
        {
            GameObject go = Instantiate(ManagerRefrenceHolder.instance.networkManagerObject);
            NetworkServer.Spawn(go, this.gameObject);
        }
    }
    private void sendInitialInformations()
    {
        player_Name = MainMenuManager.instance.playerName;
        CmdSendPlayerNamePacket(player_Name);
        CmdSendPlayerObjectToList(this.gameObject.GetComponent<NetworkIdentity>().netId);
        MainMenuManager.instance.switchMenuToRoom();
        basicSetupDone = true;
    }

    #region commands
    [Command]
    private void CmdSendPlayerNamePacket(string playerName)
    {
        player_Name = playerName;
    }
    [Command]
    private void CmdSendPlayerObjectToList(uint netID)
    {
        LobbySlotManager.instance.addObjectToList(netID);
    }
    [Command]
    public void CmdChangedReady(bool readyState)
    {
        if (connectionToClient.isReady)
        {
            RpcChangedReady(readyState);
            readyToBegin = readyState;
        }
        else
        {
            Debug.Log("Client Is Not Ready");
        }
    }
    #endregion

    #region rpcs
    [ClientRpc]
    private void RpcChangedReady(bool readyState)
    {
        readyToBegin = readyState;
        MainLobbyUI.instance.refreshSlots();
    }
    [TargetRpc]
    public void RpcKickPlayerNotification(NetworkConnection target)
    {
        NotificationCreator.instance.createNotification("You have been kicked", "Owner of the lobby kicked you!");
    }
    [TargetRpc]
    public void RpcKickPlayer(NetworkConnection target)
    {
        NotificationCreator.instance.createNotification("You have been kicked", "Owner of the lobby kicked you!");
        target.Disconnect();
    }
    [TargetRpc]
    public void RpcKickEpicPlayer(NetworkConnection target)
    {
        NotificationCreator.instance.createNotification("You have been kicked", "Owner of the lobby kicked you!");
        MainMenuManager.instance.leaveServer();
    }
    #endregion
}
