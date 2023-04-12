using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class Quicksand : NetworkBehaviour
{
    [Header("Main Stats")]
    [SerializeField] private int getOutCost;
    [SerializeField] private int doubleRollsBeforeGoingQuicksand = 3;
    [SerializeField] private int doubleRollsBeforeReleaseQuicksand = 3;
    [Header("Misc Stats"), Space(20)]
    public SyncList<uint> TrappedPlayers = new SyncList<uint>();
    [SerializeField] private Dictionary<uint, int> playerDiceCounter = new Dictionary<uint, int>();
    [SerializeField] private Dictionary<uint, int> playerReleaseDiceCounter = new Dictionary<uint, int>();

    public static Quicksand instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        QueueManager.queueChanged += checkForInterface;
        DiceManager.diceFired += checkForInterfaceDice;
        BoardMoveManager.onMoveDone += checkIfPlayerInQuicksand;
    }
    private void OnDestroy()
    {
        QueueManager.queueChanged -= checkForInterface;
        DiceManager.diceFired += checkForInterfaceDice;
        BoardMoveManager.onMoveDone -= checkIfPlayerInQuicksand;
    }

    #region Auto Checks
    private void checkIfPlayerInQuicksand(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        if (!isServer) return;
        if (plotClass.landName != "Quicksand") return;
        if (TrappedPlayers.Contains(boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId)) return;
        else
        {
            TrappedPlayers.Add(boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId);
        }
    }
    /// <summary>
    /// returns true if player should go jail
    /// </summary>
    public bool CheckAndAdjustForQuicksand(int firstDice, int secondDice, BoardPlayer player)
    {
        if (!isServer) return false;
        uint playerId = player.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId;
        if (!playerDiceCounter.ContainsKey(playerId)) playerDiceCounter.Add(playerId, 0);
        if (firstDice == secondDice)
        {
            if (playerDiceCounter[playerId] == doubleRollsBeforeGoingQuicksand - 1)
            {
                //Go To Quicksand
                string playerName = UtulitiesOfDmr.ReturnCorrespondPlayerById(playerId).GetComponent<NetworkPlayerCON>().PlayerName;
                AnimatedTextCreator.instance.CreateAnimatedText($"{playerName} rolled double dice 3 times in a row.", Color.red);
                playerDiceCounter[playerId] = 0;
                if (!TrappedPlayers.Contains(playerId)) TrappedPlayers.Add(playerId);
                return true;
            }
            else
            {
                playerDiceCounter[playerId] += 1;
                return false;
            }
        }
        else
        {
            playerDiceCounter[playerId] = 0;
            return false;
        }
    }
    private void checkForInterface(uint playerId)
    {
        if (!TrappedPlayers.Contains(NetworkPlayerCON.localPlayerCON.netId))
        {
            ToggleQuicksandInterface(false);
            return;
        }
        if (playerId != NetworkPlayerCON.localPlayerCON.netId)
        {
            //Board Move Manager Toggles On Interface
            ToggleQuicksandInterface(false);
        }
        else
        {
            if (TrappedPlayers.Contains(playerId))
            {
                ToggleQuicksandInterface(true);
            }
        }
    }
    private void checkForInterfaceDice(int i, int i2, BoardPlayer boardPlayer)
    {
        if (!TrappedPlayers.Contains(NetworkPlayerCON.localPlayerCON.netId))
        {
            ToggleQuicksandInterface(false);
            return;
        }
        ToggleQuicksandInterface(false);
    }
    public void AdjustReleaseDict(uint playerId)
    {
        if (!isServer) return;
        if (!playerReleaseDiceCounter.ContainsKey(playerId)) playerReleaseDiceCounter.Add(playerId, 0);
        if (playerReleaseDiceCounter[playerId] == doubleRollsBeforeReleaseQuicksand - 1)
        {
            if (TrappedPlayers.Contains(playerId)) TrappedPlayers.Remove(playerId);
            playerReleaseDiceCounter.Remove(playerId);
        }
        else
        {
            playerReleaseDiceCounter[playerId] += 1;
        }
    }
    public void RemovePlayerFromDiceDict(uint playerId)
    {
        if (!isServer) return;
        if (!playerDiceCounter.ContainsKey(playerId)) return;
        playerDiceCounter[playerId] = 0;
    }
    #endregion

    #region Cmd's
    [Command(requiresAuthority = false)]
    public void RequestPayForQuicksand(NetworkConnectionToClient conn = null)
    {
        try
        {
            if (conn.identity == null) return;
            if (conn.identity.netId != QueueManager.instance.CurrentQueue) return;
            if (DiceManager.instance.currentQueueAlreadyUsedDice) return;
            if (!TrappedPlayers.Contains(conn.identity.netId)) return;
            NetworkPlayerCON netPc = conn.identity.GetComponent<NetworkPlayerCON>();
            if (netPc.PlayerMoney < getOutCost)
            {
                MainGameManager.instance.RpcSendTargetError(conn, "Error", "You don't have enough gold");
                return;
            }
            netPc.PlayerMoney -= getOutCost;

            TrappedPlayers.Remove(conn.identity.netId);
            closeToggleCallback(conn);
            AnimatedTextCreator.instance.CreateAnimatedText($"{netPc.PlayerName} paid {getOutCost} gold to get out.", Color.green);
        }
        catch
        {
            Debug.Log("Error Occured While Doing Quicksand Calculation");
        }
    }
    #endregion
    #region Target Rpc's
    [TargetRpc]
    private void closeToggleCallback(NetworkConnection conn)
    {
        ToggleQuicksandInterface(false);
    }
    #endregion
    public void ToggleQuicksandInterface(bool _toggle)
    {
        if (BoardRefrenceHolder.instance != null)
            BoardRefrenceHolder.instance.quicksandInterface.SetActive(_toggle);
    }
}
