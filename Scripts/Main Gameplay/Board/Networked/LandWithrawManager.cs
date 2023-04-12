using UnityEngine;
using System.Collections.Generic;
using Mirror;
using System;

public class LandWithrawManager : NetworkBehaviour
{
    //This script is responsible for withrawing money from players

    public static LandWithrawManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        BoardMoveManager.onMoveDone += onPlayerLanded;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= onPlayerLanded;
    }
    private void onPlayerLanded(PlotClass landedPlot, BoardPlayer landedPlayer)
    {
        if (!isServer) return;
        plotFilter pFilter = calculateFilterOfPlot(landedPlot);
        if (pFilter == plotFilter.Empty) return;
        if (pFilter == plotFilter.Vacuum)
        {
            //Check For Player Total Worth Then Withraw
            int totalDebt = UtulitiesOfDmr.GetVacuumPriceForPlayer(landedPlayer);
            int playerMoney = landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney;
            if (playerMoney < totalDebt)
            {
                AnimatedTextCreator.instance.CreateAnimatedText($"{landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerName} doesn't have enough money to pay", Color.red);
                NetworkSellingManager.instance.sellModeOpen = true; //To prevent players skipping queue
                InvokerOfDmr.InvokeWithDelay(NetworkSellingManager.instance, NetworkSellingManager.instance.OpenSellMode, 4f, landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>(), totalDebt, VacuumSystem.instance.gameObject);
                //Toggle sell mode in the future
            }
            else
            {
                if (totalDebt == 0)
                {
                    AnimatedTextCreator.instance.CreateAnimatedText($"{landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerName} doesn't have to pay anything!", Color.green);
                    return;
                }
                landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney -= totalDebt;
                VacuumSystem.instance.adjustMoney(totalDebt);
                moneyTraceEffectRpc(totalDebt, landedPlayer.transform.position, BoardRefrenceHolder.instance.vacuumPlace.transform.position, landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().playerColor, landedPlayer.representedPlayerId, landedPlot.landIndex);
            }
            return;
        }
        if (pFilter == plotFilter.OwnedByPlayer)
        {
            //TODO: Check If Player Have Enough Money To Buy If Not Open Sell Menu
            int totalDebt = UtulitiesOfDmr.GetCurrentPlotIncome(landedPlot);
            BoardPlayer boardPlayer_ = UtulitiesOfDmr.ReturnBoardPlayerById(landedPlot.ownedBy);
            int playerMoney = landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney;
            if (boardPlayer_ == landedPlayer) return; //Owner is same with the landed player
            if (playerMoney < totalDebt)
            {
                AnimatedTextCreator.instance.CreateAnimatedText($"{landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerName} doesn't have enough money to pay", Color.red);
                NetworkSellingManager.instance.sellModeOpen = true; //To prevent players skipping queue
                InvokerOfDmr.InvokeWithDelay(NetworkSellingManager.instance, NetworkSellingManager.instance.OpenSellMode, 4f, landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>(), totalDebt, boardPlayer_.gameObject);
                //Add Logic
                //Toggle sell mode in the future
            }
            else
            {
                landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney -= totalDebt;
                boardPlayer_.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney += totalDebt;
                Vector3 whereToSpawnOrigin = landedPlayer.GetComponentInChildren<Renderer>().bounds.center;
                Vector3 targetOrigin = boardPlayer_.GetComponentInChildren<Renderer>().bounds.center;
                moneyTraceEffectRpc(totalDebt, whereToSpawnOrigin, targetOrigin, landedPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().playerColor, landedPlayer.representedPlayerId, landedPlot.landIndex);
            }
            return;
        }
    }
    private void createEffects(int totalMoney, Vector3 whereToSpawn, Vector3 Target, Color32 traceColor)
    {
        int totalMoneyTraceCount = (int)Math.Ceiling(totalMoney / 100000f);
        if (totalMoneyTraceCount < 1) totalMoneyTraceCount += 1;
        EffectManager.instance.createMoneyTraces(whereToSpawn, Target, totalMoneyTraceCount, traceColor);
    }
    private plotFilter calculateFilterOfPlot(PlotClass landedPlot)
    {
        //if owned by is zero that means there is no owner for that plot
        if (landedPlot.landKind == LandKind.Buyable && landedPlot.ownedBy != 0)
        {
            return plotFilter.OwnedByPlayer;
        }
        else if (landedPlot.landKind == LandKind.Buyable && landedPlot.ownedBy == 0)
        {
            return plotFilter.Empty;
        }
        else if (landedPlot.landKind == LandKind.EventLand && landedPlot.landName == "Unstable Hole")
        {
            return plotFilter.Vacuum;
        }
        return plotFilter.Empty;
    }
    private enum plotFilter
    {
        Empty,
        OwnedByPlayer,
        //Unstable Hole plot in the board
        Vacuum
    }
    public void WithrawAtoB(GameObject withrawTarget, GameObject depositTarget, int totalWithrawAmount)
    {
        //I Wish i had used interface
        if (!isServer) return;
        Debug.Log("Withraw manager called");
        if (withrawTarget.GetComponent<BoardPlayer>() == null && withrawTarget.GetComponent<VacuumSystem>() == null) return;
        if (depositTarget.GetComponent<BoardPlayer>() == null && depositTarget.GetComponent<VacuumSystem>() == null) return;
        if (depositTarget.GetComponent<BoardPlayer>() != null && withrawTarget.GetComponent<BoardPlayer>() != null)
        {
            withrawTarget.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney -= totalWithrawAmount;
            depositTarget.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney += totalWithrawAmount;
            Vector3 whereToSpawnOrigin = withrawTarget.GetComponentInChildren<Renderer>().bounds.center;
            Vector3 targetOrigin = depositTarget.GetComponentInChildren<Renderer>().bounds.center;
            moneyTraceEffectRpc(totalWithrawAmount, whereToSpawnOrigin, targetOrigin, withrawTarget.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().playerColor, 0, 0);
        }
        if (depositTarget.GetComponent<BoardPlayer>() != null && withrawTarget.GetComponent<VacuumSystem>() != null)
        {
            withrawTarget.GetComponent<VacuumSystem>().decreaseMoney(totalWithrawAmount);
            depositTarget.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney += totalWithrawAmount;
            Vector3 whereToSpawnOrigin = BoardRefrenceHolder.instance.vacuumPlace.transform.position;
            Vector3 targetOrigin = depositTarget.GetComponentInChildren<Renderer>().bounds.center;
            moneyTraceEffectRpc(totalWithrawAmount, whereToSpawnOrigin, targetOrigin, Color.blue, 0, 0);
        }
        if (depositTarget.GetComponent<VacuumSystem>() != null && withrawTarget.GetComponent<BoardPlayer>() != null)
        {
            withrawTarget.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney -= totalWithrawAmount;
            depositTarget.GetComponent<VacuumSystem>().adjustMoney(totalWithrawAmount);
            Vector3 whereToSpawnOrigin = withrawTarget.GetComponentInChildren<Renderer>().bounds.center;
            Vector3 targetOrigin = BoardRefrenceHolder.instance.vacuumPlace.transform.position;
            moneyTraceEffectRpc(totalWithrawAmount, whereToSpawnOrigin, targetOrigin, withrawTarget.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkPlayerCON>().playerColor, 0, 0);
        }
    }

    #region RPCS
    [ClientRpc]
    private void moneyTraceEffectRpc(int totalMoney, Vector3 whereToSpawn, Vector3 target, Color32 colorTrace, uint withrawBoardPlayerId, int intendedAnimationPlotIndex)
    {
        //We need to create this somewhat latency independent so we just cannot directly call effect manager
        if (withrawBoardPlayerId == 0)
        {
            createEffects(totalMoney, whereToSpawn, target, colorTrace);
            Debug.Log("Money Trace Effect " + totalMoney + " " + whereToSpawn + " vs " + target);
            return;
        }
        if (UtulitiesOfDmr.ReturnBoardPlayerById(withrawBoardPlayerId) != null)
        {
            Debug.Log("Money Trace Effect Invoker" + totalMoney + " " + whereToSpawn + " vs " + target);
            WaitUntilOfDmr.InvokeWithDelay(this, createEffects, () => UtulitiesOfDmr.ReturnBoardPlayerById(withrawBoardPlayerId).currentAnimationPlotIndex == intendedAnimationPlotIndex, totalMoney, whereToSpawn, target, colorTrace);
        }
        else
        {
            createEffects(totalMoney, whereToSpawn, target, colorTrace);
        };
    }

    #endregion
}
