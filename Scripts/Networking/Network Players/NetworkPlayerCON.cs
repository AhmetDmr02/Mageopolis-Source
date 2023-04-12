using UnityEngine;
using Mirror;
using System;

public class NetworkPlayerCON : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;
    [SyncVar(hook = "recalculateIngameUI")]
    public int PlayerMoney;
    [SyncVar(hook = "recalculateLobbyUI")]
    public Color playerColor;
    [SyncVar(hook = "recalculateLobbyUI2")]
    public PlayerModel playerModel;
    [SyncVar]
    public bool Eliminated;
    [SyncVar]
    public bool Disconnected;

    public static NetworkPlayerCON localPlayerCON;

    public event Action<int, int> playerMoneyChanged;
    private void Start()
    {
        setupIngamePlayerCON();
    }
    private void Update()
    {
        //It doesn't work with awake or start methods
        setupLocalPlayerObject();
    }
    public void recalculateLobbyUI(Color a, Color b)
    {
        PlayerStatsGUI.instance.gameObject.GetComponent<PlayerSelectModelAndColor>().recalculateAll();
    }
    public void recalculateLobbyUI2(PlayerModel a, PlayerModel b)
    {
        PlayerStatsGUI.instance.gameObject.GetComponent<PlayerSelectModelAndColor>().recalculateAll();
    }
    private void setupLocalPlayerObject()
    {
        if (!isLocalPlayer) return;
        if (localPlayerCON == null)
        {
            localPlayerCON = this;
            PlayerStatsGUI.instance.gameObject.GetComponent<PlayerSelectModelAndColor>().recalculateAll();
        }
    }
    private void setupIngamePlayerCON()
    {
        if (!isLocalPlayer) return;

        GameObject[] roomPlayers = LobbySlotManager.instance.players.ToArray();
        foreach (GameObject go in roomPlayers)
        {
            NetworkRoomPlayerRev NPR = go.GetComponent<NetworkRoomPlayerRev>();
            if (NPR.isOwned)
            {
                PlayerName = NPR.player_Name;
                CmdUpdateName(PlayerName);
                CmdUpdatePlayerID(this.gameObject.GetComponent<NetworkIdentity>().netId);
                break;
            }
        }
    }
    #region commands
    [Command]
    private void CmdUpdateName(string newValue)
    {
        RpcUpdateName(newValue);
    }
    [Command]
    private void CmdUpdatePlayerID(uint id)
    {
        MainPlayerRefrences.instance.mainPlayerIds.Add(id);
    }
    [Command]
    public void CmdUpdatePlayerModel(bool next)
    {
        playerModel = playerModel.Next();
    }
    [Command]
    public void CmdRemovemeFromList(uint id)
    {
        MainPlayerRefrences.instance.mainPlayerIds.Remove(id);
    }
    #endregion;

    #region Client Rpcs
    [ClientRpc]
    private void RpcUpdateName(string newValue)
    {
        this.PlayerName = newValue;
    }
    [ClientRpc]
    public void RpcCalculateModelAndColorAttribute()
    {
        if (PlayerStatsGUI.instance == null) return;
        PlayerStatsGUI.instance.gameObject.GetComponent<PlayerSelectModelAndColor>().recalculateAll();
    }
    #endregion

    #region hooks
    public void recalculateIngameUI(int oldValue, int newValue)
    {
        if (PlayerStatsGUI.instance != null)
            PlayerStatsGUI.instance.RecalculateGUI(MainPlayerRefrences.instance.PlayerObjects.ToArray());
        playerMoneyChanged?.Invoke(oldValue, newValue);
    }
    #endregion
    private void OnDestroy()
    {
        if (!NetworkServer.active) return;
        if (MainPlayerRefrences.instance != null) MainPlayerRefrences.instance.removeFromId();
    }
    public enum PlayerModel
    {
        Zabu,
        Kabu,
        Rabu,
        Ububu,
    }
}
