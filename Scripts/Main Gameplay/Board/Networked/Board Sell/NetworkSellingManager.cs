using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class NetworkSellingManager : NetworkBehaviour
{
    public static NetworkSellingManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    [SyncVar(hook = nameof(hookSellModeChanged))]
    [HideInInspector] public bool sellModeOpen = false;
    bool spamBlocker = false;
    uint currentPlayerID = 0;
    int neededMoney = 0;
    int totalNeededMoney = 0;
    GameObject withrawer = null;
    GameObject depositer = null;
    public void OpenSellMode(NetworkPlayerCON playerCon, int totalDebt, GameObject whoToPay)
    {
        if (!isServer) return;
        if (playerCon == null) return;
        int playerMoney = playerCon.PlayerMoney;
        int requiredMoney = totalDebt - playerMoney;
        neededMoney = requiredMoney;
        totalNeededMoney = totalDebt;
        withrawer = UtulitiesOfDmr.ReturnBoardPlayerById(playerCon.netId).gameObject;
        depositer = whoToPay;
        int totalWorthOfPlayer = UtulitiesOfDmr.ReturnTotalWorthOfPlayerByID(playerCon.netId);
        Debug.Log("Total Worth Of Player Was " + totalWorthOfPlayer);
        int[] ownedLandsIndexes = UtulitiesOfDmr.ReturnOwnedLandsIndexesByPlayerID(playerCon.netId);
        currentPlayerID = playerCon.netId;
        if (totalWorthOfPlayer >= totalNeededMoney)
        {
            //Not eliminated
            AnimatedTextCreator.instance.CreateAnimatedText($"{playerCon.PlayerName} is now selling plots!", new Color32(255, 64, 0, 255));
            QueueTimer.instance.setDecreaseRate(0.5f);
            QueueTimer.instance.setTimeFull();
            toggleClientSellInterface(playerCon.netIdentity.connectionToClient, requiredMoney, ownedLandsIndexes);
            sellModeOpen = true;
        }
        else
        {
            //Eliminated
            playerCon.Eliminated = true;
            AnimatedTextCreator.instance.CreateAnimatedText($"{playerCon.PlayerName} is eliminated!", Color.red);
            //Number's doesn't matter unless they are different
            QueueManager.instance.GetServerNextQueue(1, 2);
            int totalCollected = 0;
            totalCollected += playerCon.PlayerMoney;
            List<PlotClass> plotClasses = UtulitiesOfDmr.ReturnOwnedByPlayerPlots(playerCon.netIdentity.netId);
            List<int> removeClasses = new List<int>();
            foreach (PlotClass pc in plotClasses)
            {
                totalCollected += UtulitiesOfDmr.ReturnTotalWorthOfPlotClasses(new PlotClass[] { pc });
                removeClasses.Add(pc.landIndex);
            }
            foreach (int i in removeClasses)
            {
                //For Recalculating Visuals
                LandBuyManager.instance.InvokeRecalculateAction(i, 0, 0);
                BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>().ResetLand(true);
            }
            resetThisLands(removeClasses.ToArray());
            LandWithrawManager.instance.WithrawAtoB(withrawer, depositer, totalCollected);
            resetSellingManager();
            QueueManager.instance.queueList.Remove(playerCon.netIdentity.netId);
            //TODO: Kick out from QUEUE MANAGER
            //TODO: Sell all the plots and give it to rival player
            //TODO: Remove all owned plots
            //This part is done next up make test with disconnected players
        }
    }
    public void QueueTimePassedBeforeRequest()
    {
        if (!isServer) return;
        Debug.Log("QueueTimePassedBeforeRequest()");
        if (!sellModeOpen)
        {
            //Incase of something is gonna break we will get the next queue just to be safe
            getNextQueueWithWarning();
            return;
        }
        else
        {
            //Automatically Sell
            spamBlocker = true;
            int collectedMoney = 0;
            if (withrawer.GetComponent<BoardPlayer>() == null)
            {
                getNextQueueWithWarning();
                return;
            }
            else
            {
                Debug.Log("Auto Sell Stage 1");
                List<PlotClass> playerOwnedLands = UtulitiesOfDmr.ReturnOwnedByPlayerPlots(withrawer.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().netId).ToList();
                List<int> clearLandsIndexes = new List<int>();
                foreach (PlotClass _plotClass in playerOwnedLands)
                {
                    Debug.Log("Auto Sell Stage Loop");
                    if (collectedMoney < totalNeededMoney)
                    {
                        collectedMoney += UtulitiesOfDmr.ReturnTotalWorthOfPlotClasses(new PlotClass[] { _plotClass });
                        clearLandsIndexes.Add(_plotClass.landIndex);
                        if (collectedMoney < totalNeededMoney)
                        {
                            continue;
                        }
                        else
                        {
                            Debug.Log("Withraw Time Called");
                            //Withraw Time
                            withrawer.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney += collectedMoney;
                            foreach (int i in clearLandsIndexes)
                            {
                                //For Recalculating Visuals
                                Debug.Log("clearLandsIndexes");
                                LandBuyManager.instance.InvokeRecalculateAction(i, 0, 0);
                                BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>().ResetLand(true);
                            }
                            resetThisLands(clearLandsIndexes.ToArray());
                            LandWithrawManager.instance.WithrawAtoB(withrawer, depositer, totalNeededMoney);
                            queueMissedSellRequest();
                            resetSellingManager();

                            //Variables Doesn't matter
                            QueueTimer.instance.setTimerForPlayer(null, null);
                            break;
                        }
                    }
                    else
                    {
                        Debug.Log("Withraw Time Called");
                        //Withraw Time
                        withrawer.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney += collectedMoney;
                        foreach (int i in clearLandsIndexes)
                        {
                            //For Recalculating Visuals
                            Debug.Log("clearLandsIndexes");
                            LandBuyManager.instance.InvokeRecalculateAction(i, 0, 0);
                            BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>().ResetLand(true);
                        }
                        resetThisLands(clearLandsIndexes.ToArray());
                        LandWithrawManager.instance.WithrawAtoB(withrawer, depositer, totalNeededMoney);
                        queueMissedSellRequest();
                        resetSellingManager();

                        //Variables Doesn't matter
                        QueueTimer.instance.setTimerForPlayer(null, null);
                        break;
                    }
                }
            }
        }
    }
    private void resetSellingManager()
    {
        sellModeOpen = false;
        spamBlocker = false;
        neededMoney = 0;
        withrawer = null;
        depositer = null;
        currentPlayerID = 0;
    }
    private void getNextQueueWithWarning()
    {
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(QueueManager.instance.CurrentQueue);
        if (boardPlayer != null)
        {
            QueueManager.instance.GetServerNextQueue(boardPlayer.previousDiceOne, boardPlayer.previousDiceTwo);
        }
        else
        {
            QueueManager.instance.GetServerNextQueue(1, 2);
            Debug.LogWarning("Board Player Result Turned Out Null");
        }
        Debug.LogWarning("QueueTimePassedBeforeRequest() Has been fired for next queue that is actually means something in the logic is broken.");
        QueueTimer.instance.closeTimer();
    }
    private void bankruptPlayer(NetworkPlayerCON playerCon, GameObject withrawer_, GameObject depositer_)
    {
        if (!isServer) return;
        playerCon.Eliminated = true;
        AnimatedTextCreator.instance.CreateAnimatedText($"{playerCon.PlayerName} is eliminated!", Color.red);
        //Number's doesn't matter unless they are different
        QueueManager.instance.GetServerNextQueue(1, 2);
        int totalCollected = 0;
        totalCollected += playerCon.PlayerMoney;
        List<PlotClass> plotClasses = UtulitiesOfDmr.ReturnOwnedByPlayerPlots(playerCon.netIdentity.netId);
        List<int> removeClasses = new List<int>();
        foreach (PlotClass pc in plotClasses)
        {
            totalCollected += UtulitiesOfDmr.ReturnTotalWorthOfPlotClasses(new PlotClass[] { pc });
            removeClasses.Add(pc.landIndex);
        }
        foreach (int i in removeClasses)
        {
            //For Recalculating Visuals
            LandBuyManager.instance.InvokeRecalculateAction(i, 0, 0);
            BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>().ResetLand(true);
        }
        resetThisLands(removeClasses.ToArray());
        playerCon.PlayerMoney += totalCollected;
        if (playerCon.PlayerMoney > neededMoney)
        {
            LandWithrawManager.instance.WithrawAtoB(withrawer_, depositer_, neededMoney);
            playerCon.PlayerMoney -= neededMoney;
        }
        else
        {
            LandWithrawManager.instance.WithrawAtoB(withrawer_, depositer_, playerCon.PlayerMoney);
            playerCon.PlayerMoney -= neededMoney;
        }
        resetSellingManager();
        QueueManager.instance.queueList.Remove(playerCon.netIdentity.netId);
        QueueManager.instance.SetLastQueueDisconnected();
    }

    #region CMDS
    [Command(requiresAuthority = false)]
    public void RequestSellPlots(int[] PlotIndexes, NetworkConnectionToClient conn = null)
    {
        if (!isServer) return;
        if (!sellModeOpen) return;
        if (spamBlocker) return;
        if (conn.identity.netId != currentPlayerID) return;
        spamBlocker = true;
        List<PlotClass> plotClasses = new List<PlotClass>();
        foreach (int i in PlotIndexes)
        {
            plotClasses.Add(BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>());
        }
        foreach (PlotClass plotClass_ in plotClasses)
        {
            if (plotClass_.ownedBy != conn.identity.netId)
            {
                denySellRequest(conn, "You selected a plot that you don't owned?");
                spamBlocker = false;
                return;
            }
        }
        int totalWorthOfPlots = UtulitiesOfDmr.ReturnTotalWorthOfPlotClasses(plotClasses.ToArray());
        if (totalWorthOfPlots < neededMoney)
        {
            denySellRequest(conn, "You need to select more locations to pay!");
            spamBlocker = false;
            return;
        }
        NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>();
        foreach (int i in PlotIndexes)
        {
            //For Recalculating Visuals
            LandBuyManager.instance.InvokeRecalculateAction(i, 0, 0);
            BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>().ResetLand(true);
        }
        resetThisLands(PlotIndexes);
        netPc.PlayerMoney += totalWorthOfPlots;
        LandWithrawManager.instance.WithrawAtoB(withrawer, depositer, totalNeededMoney);
        approveSellRequest(conn);
        resetSellingManager();
    }
    [Command(requiresAuthority = false)]
    public void RequestBankrupt(NetworkConnectionToClient conn = null)
    {
        if (!isServer) return;
        if (!sellModeOpen) return;
        if (spamBlocker) return;
        if (conn.identity.netId != currentPlayerID) return;
        spamBlocker = true;
        NetworkPlayerCON playerCon = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>() == null ? null : UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>();
        if (playerCon == null) return;
        bankruptPlayer(playerCon, withrawer, depositer);
        //This will close players selling interface
        approveSellRequest(conn);
        AnimatedTextCreator.instance.CreateAnimatedText($"{playerCon.PlayerName} went bankrupt!", Color.red);
    }
    #endregion

    #region Target RPCS
    [TargetRpc]
    private void toggleClientSellInterface(NetworkConnection conn, int requiredMoney, int[] indexes)
    {
        if (SellingInterface.instance.sellingIntefaceOn) return;
        SellingInterface.instance.InitSellingMode(requiredMoney, indexes);
    }
    [TargetRpc]
    private void approveSellRequest(NetworkConnection conn)
    {
        if (!SellingInterface.instance.sellingIntefaceOn) return;
        SellingInterface.instance.closeSellingMode();
    }
    [TargetRpc]
    private void denySellRequest(NetworkConnection conn, string denyMessage)
    {
        NotificationCreator.instance.createNotification("Error", denyMessage);
    }
    #endregion

    #region Client RPCS
    [ClientRpc]
    private void queueMissedSellRequest()
    {
        //CONT FROM HERE
        if (!SellingInterface.instance.sellingIntefaceOn) return;
        SellingInterface.instance.closeSellingMode();
    }
    [ClientRpc]
    private void resetThisLands(int[] landIndexes)
    {
        if (isServer) return;
        foreach (int i in landIndexes)
        {
            //For Recalculating Visuals
            LandBuyManager.instance.InvokeRecalculateAction(i, 0, 0);
            BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>().ResetLand(true);
        }
    }
    #endregion

    private void hookSellModeChanged(bool oldVal, bool newVal)
    {
        if (ConfigPiercer.instance.shouldPlayersEnd && AutomaticQueueEnder.instance.NotInSellMode)
        {
            if (!newVal)
            {
                InvokerOfDmr.InvokeWithDelay(AutomaticQueueEnder.instance, AutomaticQueueEnder.instance.SafeAutoend, 2f);
            }
        }
    }
}
