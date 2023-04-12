using Mirror;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class MainGameManager : NetworkBehaviour
{
    public static MainGameManager instance;
    [SerializeField] private List<Color> availablePlayerColors = new List<Color>();
    [SerializeField] public SyncList<Color> usedPlayerColors = new SyncList<Color>();
    [SyncVar(hook = "onServerStateChanged")]
    public ServerGameState serverGameState;

    [SyncVar(hook = "removeWaitingForPlayerPanel")]
    public bool isEveryOneLoaded;
    public List<Color> AvailablePlayerColors => availablePlayerColors;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        if (isServer) serverGameState = ServerGameState.LobbySelecting;
    }
    #region Commands
    [Command(requiresAuthority = false)]
    public void CmdChangeUsedColors(Color oldColor, Color wantedColor, NetworkIdentity identity)
    {
        if (oldColor == wantedColor)
        {
            RpcSendTargetError(identity.connectionToClient, "Choose another color", "You are already have this color.");
            return;
        }
        if (usedPlayerColors.Contains(wantedColor))
        {
            RpcSendTargetError(identity.connectionToClient, "Choose another color", "Someone is already chosen this color.");
            return;
        }
        if (!availablePlayerColors.Contains(wantedColor))
        {
            RpcSendTargetError(identity.connectionToClient, "Choose another color", "This color is not in the container?");
            return;
        }
        try
        {
            if (usedPlayerColors.Contains(oldColor))
                usedPlayerColors.Remove(oldColor);
            usedPlayerColors.Add(wantedColor);
            identity.gameObject.GetComponent<NetworkPlayerCON>().playerColor = wantedColor;
        }
        catch (System.Exception)
        {
            return;
        }
    }

    [Command(requiresAuthority = true)]
    public void startGameFromSelecting()
    {
        GameObject[] players = MainPlayerRefrences.instance.PlayerObjects.ToArray();
        bool nonColorFound = false;
        foreach (GameObject player in players)
        {
            NetworkPlayerCON NetPC = player.GetComponent<NetworkPlayerCON>();
            if (!availablePlayerColors.Contains(NetPC.playerColor))
            {
                NotificationCreator.instance.createNotification("Error", "Everybody should select color");
                nonColorFound = false;
                return;
            }
            else
            {
                nonColorFound = true;
            }
        }
        if (nonColorFound)
        {
            //TODO: Start Game
            if (serverGameState == ServerGameState.Ingame) return;
            serverGameState = ServerGameState.Ingame;
            foreach (GameObject GO in MainPlayerRefrences.instance.PlayerObjects)
            {
                NetworkPlayerCON localNetPC = GO.GetComponent<NetworkPlayerCON>();
                localNetPC.PlayerMoney = 1500000;
            }
            gameStartedEvent();
        }
    }

    public void recalculateUsedColors()
    {
        if (!isServer) return;
        GameObject[] players = MainPlayerRefrences.instance.PlayerObjects.ToArray();
        usedPlayerColors.Clear();
        foreach (GameObject go in players)
        {
            usedPlayerColors.Add(go.gameObject.GetComponent<NetworkPlayerCON>().playerColor);
        }
    }
    #endregion

    #region ClientRpcs 


    [ClientRpc]
    public void gameStartedEvent()
    {
        PlayerStatsGUI.instance.recalculateWithColor = true;
        PlayerStatsGUI.instance.RecalculateGUI(MainPlayerRefrences.instance.PlayerObjects.ToArray());
    }

    public void removeWaitingForPlayerPanel(bool oldVal, bool newVal)
    {
        if (newVal == false) return;
        if (PlayerStatsGUI.instance == null) return;
        PlayerStatsGUI.instance.closeWaitingForPlayers();
    }
    #endregion

    #region TargetRpcs
    [TargetRpc]
    public void RpcSendTargetError(NetworkConnection connection, string errorTitle, string errorMes)
    {
        NotificationCreator.instance.createNotification(errorTitle, errorMes);
    }
    #endregion

    #region Hooks
    public void onServerStateChanged(ServerGameState oldState, ServerGameState newState)
    {
        if (newState == ServerGameState.Ingame)
        {
            PlayerStatsGUI.instance.gameObject.GetComponent<PlayerSelectModelAndColor>().closeSelectPanel();
            BoardMoveManager.instance.setupBoard();
        }
    }
    #endregion

    [System.Serializable]
    public enum ServerGameState
    {
        LobbySelecting,
        Ingame,
        Paused,
        Finished,
    }
}
