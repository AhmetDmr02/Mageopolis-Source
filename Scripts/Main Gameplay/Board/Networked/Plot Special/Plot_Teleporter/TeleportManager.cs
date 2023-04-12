using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeleportManager : NetworkBehaviour
{
    [Header("Main Stats")]
    [SerializeField] private int teleporterPrice;
    private bool teleporterOpen;
    public bool TeleporterOpen => teleporterOpen;
    private uint currentTeleporterPlayer;
    private int[] eligibleIndexes = { 1, 2, 3, 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 22, 23, 25, 26, 27, 28, 31, 32, 33, 35, 36, 37, 38, 39 };
    public static TeleportManager instance;
    private List<uint> teleportQueue = new List<uint>();
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        BoardMoveManager.onMoveDone += checkIfPlayerLandedTeleporter;
        QueueManager.queueChanged += listenForQueue;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= checkIfPlayerLandedTeleporter;
        QueueManager.queueChanged -= listenForQueue;
    }
    private void checkIfPlayerLandedTeleporter(PlotClass plotClass_, BoardPlayer boardPlayer_)
    {
        //30 is where teleport located
        if (!isServer) return;
        if (plotClass_.landIndex != 30) return;
        NetworkPlayerCON netPC = boardPlayer_.networkPlayerObject.GetComponent<NetworkPlayerCON>();
        if (netPC.Eliminated) return;
        if (netPC.Disconnected) return;
        if (netPC.PlayerMoney < teleporterPrice)
        {
            AnimatedTextCreator.instance.CreateAnimatedText($"{netPC.PlayerName} doesn't have enough gold to use teleporter", Color.white);
            return;
        }
        if (teleporterOpen) return;
        teleportQueue.Add(boardPlayer_.representedPlayerId);
        AnimatedTextCreator.instance.CreateAnimatedText($"{netPC.PlayerName} Will be able to use teleporter next tour", Color.white);

    }
    public void QueueTimePassed()
    {
        if (currentTeleporterPlayer == 0) return;
        if (!isServer) return;
        Debug.Log(currentTeleporterPlayer + "Current Teleporter Player ID");
        if (UtulitiesOfDmr.ReturnCorrespondPlayerById(currentTeleporterPlayer) == null) return;
        NetworkPlayerCON netPC = UtulitiesOfDmr.ReturnCorrespondPlayerById(currentTeleporterPlayer).GetComponent<NetworkPlayerCON>();
        approveRequest(netPC.netIdentity.connectionToClient);
        resetTeleporter();
    }
    private void listenForQueue(uint playerId)
    {
        if (!isServer) return;
        if (teleportQueue.Contains(playerId))
        {
            teleportQueue.Remove(playerId);
            DiceTimer.instance.setTimeFull();
            DiceTimer.instance.setDecreaseRate(0.3f);
            currentTeleporterPlayer = playerId;
            teleporterOpen = true;
            NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(playerId).GetComponent<NetworkPlayerCON>();
            initTeleportMode(netPc.netIdentity.connectionToClient);
        }
    }
    #region Cmds
    [Command(requiresAuthority = false)]
    public void RequestTeleport(int targetPlotIndex, NetworkConnectionToClient conn = null)
    {
        #region Checking for eligibility
        if (!isServer) return;
        if (!teleporterOpen)
        {
            rejectRequest(conn, "Teleport mode is not open.");
            return;
        }
        if (targetPlotIndex < 0 || targetPlotIndex > 39)
        {
            rejectRequest(conn, "Something went wrong try again. (maybe without cheating)");
            return;
        }
        if (conn.identity.netId != currentTeleporterPlayer)
        {
            rejectRequest(conn, "You are not the approved player.");
            return;
        }
        if (!eligibleIndexes.Contains(targetPlotIndex))
        {
            rejectRequest(conn, "Plot that you selected is not eligible.");
            return;
        }
        GameObject netPcObject = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId);
        if (netPcObject == null)
        {
            rejectRequest(conn, "Server cannot find your player.");
            return;
        }
        if (netPcObject.GetComponent<NetworkPlayerCON>() == null)
        {
            rejectRequest(conn, "Something went wrong.");
            return;
        }
        NetworkPlayerCON netPC = netPcObject.GetComponent<NetworkPlayerCON>();
        if (netPC.PlayerMoney < teleporterPrice)
        {
            //Eventhough this shouldn't be happening im gonna add this just to be safe
            rejectRequest(conn, "You don't have enough money.");
            return;
        }
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(conn.identity.netId);
        if (boardPlayer == null)
        {
            rejectRequest(conn, "Something went wrong.");
            return;
        }
        #endregion



        //To prevent from spamming
        teleporterOpen = false;
        approveRequest(conn);

        netPC.PlayerMoney -= teleporterPrice;

        //Will Artifically Use Dice
        DiceTimer.instance.DiceFiredByPlayer(0, 0, null);
        DiceManager.instance.currentQueueAlreadyUsedDice = true;
        DiceManager.instance.spamPreventer = true;
        boardPlayer.previousDiceOne = 1;
        boardPlayer.previousDiceTwo = 2;
        closeTimerForDice(conn);
        Quicksand.instance.RemovePlayerFromDiceDict(conn.identity.netId);
        BoardMoveManager.instance.MovePlayerTo(targetPlotIndex, boardPlayer);
        resetTeleporter();
    }
    [TargetRpc]
    private void closeTimerForDice(NetworkConnection conn)
    {
        //Variables Doesn't Matter
        DiceButton.instance.closeButton(0, 0, null);
    }
    [Command(requiresAuthority = false)]
    public void RequestSkip(NetworkConnectionToClient conn = null)
    {
        #region Checking for eligibility
        if (!isServer) return;
        if (!teleporterOpen)
        {
            rejectRequest(conn, "Teleport mode is not open.");
            return;
        }
        if (conn.identity.netId != currentTeleporterPlayer)
        {
            rejectRequest(conn, "You are not the approved player.");
            return;
        }
        GameObject netPcObject = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId);
        if (netPcObject == null)
        {
            rejectRequest(conn, "Server cannot find your player.");
            return;
        }
        if (netPcObject.GetComponent<NetworkPlayerCON>() == null)
        {
            rejectRequest(conn, "Something went wrong.");
            return;
        }
        NetworkPlayerCON netPC = netPcObject.GetComponent<NetworkPlayerCON>();
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(conn.identity.netId);
        if (boardPlayer == null)
        {
            rejectRequest(conn, "Something went wrong.");
            return;
        }
        #endregion

        //To prevent from spamming
        teleporterOpen = false;
        approveRequest(netPC.netIdentity.connectionToClient);
        resetTeleporter();
    }
    #endregion

    #region Target Rpcs
    [TargetRpc]
    private void initTeleportMode(NetworkConnection conn)
    {
        //30 is where plot teleporter script is located
        Plot_Teleporter plotTeleporter = BoardMoveManager.instance.LandGameobject[30].GetComponent<Plot_Teleporter>();
        plotTeleporter.QueueInitTeleportHighlighter(eligibleIndexes);
    }
    [TargetRpc]
    private void rejectRequest(NetworkConnection conn, string rejectString)
    {
        NotificationCreator.instance.createNotification("Error", rejectString);
        return;
    }
    [TargetRpc]
    private void approveRequest(NetworkConnection conn)
    {
        //30 is where plot teleporter script is located
        Plot_Teleporter plotTeleporter = BoardMoveManager.instance.LandGameobject[30].GetComponent<Plot_Teleporter>();
        plotTeleporter.CloseTeleportHighlight();
    }
    #endregion

    private void resetTeleporter()
    {
        teleporterOpen = false;
        //No player will be 0
        currentTeleporterPlayer = 0;
        DiceTimer.instance.setDecreaseRate(1);
    }
}
