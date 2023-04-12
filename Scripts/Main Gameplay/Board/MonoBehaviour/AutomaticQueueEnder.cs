using System;
using System.Linq;
using UnityEngine;

public class AutomaticQueueEnder : MonoBehaviour
{
    //This script can be client sided server can understand if player is cheating very early
    //If player is not auto skipping
    [SerializeField] private bool automaticEnderOpen;

    public bool NotInSellMode { private set; get; }

    public static AutomaticQueueEnder instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        BoardMoveManager.onMoveDone += checkForAutoend;
        LandBuyManager.PlayerBoughtLand += checkForAutoendAfterBuy;
        LandBuyManager.PlayerUpgradedLand += checkForAutoendAfterUpgrade;
        automaticEnderOpen = ConfigPiercer.instance.shouldPlayersEnd;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= checkForAutoend;
        LandBuyManager.PlayerBoughtLand -= checkForAutoendAfterBuy;
        LandBuyManager.PlayerUpgradedLand -= checkForAutoendAfterUpgrade;
    }
    private void checkForAutoendAfterBuy(int plotIndex, uint playerId)
    {
        if (playerId == 0) return;
        PlotClass plotClass = UtulitiesOfDmr.ReturnPlotClassByIndex(plotIndex);
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(playerId);
        if (boardPlayer != null)
            checkForAutoend(plotClass, boardPlayer);
    }
    private void checkForAutoendAfterUpgrade(int plotIndex, int upgradeIndex, uint playerId)
    {
        if (playerId == 0) return;
        PlotClass plotClass = UtulitiesOfDmr.ReturnPlotClassByIndex(plotIndex);
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(playerId);
        if (boardPlayer != null)
            checkForAutoend(plotClass, boardPlayer);
    }
    private void checkForAutoend(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        if (!automaticEnderOpen) return;
        if (boardPlayer.representedPlayerId != NetworkPlayerCON.localPlayerCON.netId) return;
        NotInSellMode = false;
        switch (plotClass.landKind)
        {
            case LandKind.Buyable:
                if (plotClass.ownedBy == 0)
                {
                    if (NetworkPlayerCON.localPlayerCON.PlayerMoney < plotClass.landPrices.landBuyPrice)
                    {
                        InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                        return;
                    }
                }
                else if (plotClass.ownedBy == NetworkPlayerCON.localPlayerCON.netId)
                {
                    if (plotClass.landName == "Windview Volcano")
                    {
                        //Check for hit
                        if (plotClass.landPrices.isLandHit)
                        {
                            return;
                        }
                    }
                    if (plotClass.landCurrentUpgrade == 4)
                    {
                        InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                        return;
                    }
                    else
                    {
                        if (NetworkPlayerCON.localPlayerCON.PlayerMoney < plotClass.landPrices.landUpgradePrices[plotClass.landCurrentUpgrade])
                        {
                            InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                            return;
                        }
                        else
                            return;

                    }
                }
                else
                {

                    int[] playerOwnedPlots = UtulitiesOfDmr.ReturnOwnedLandsIndexesByPlayerID(NetworkPlayerCON.localPlayerCON.netId);
                    //Academy Indexes
                    if (!playerOwnedPlots.Contains(11) && !playerOwnedPlots.Contains(12) && !playerOwnedPlots.Contains(13))
                    {
                        if (!NetworkSellingManager.instance.sellModeOpen)
                        {
                            InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                            NotInSellMode = true;
                        }
                        else
                        {
                            WaitUntilOfDmr.InvokeWithDelay(this, delayedSafeAutoend, () => !NetworkSellingManager.instance.sellModeOpen);
                        }
                    }
                    else
                    {
                        //Check if player can wipe
                        if (!NetworkSellingManager.instance.sellModeOpen)
                        {
                            int price = UtulitiesOfDmr.ReturnWipeCostOfPlot(plotClass);
                            if (NetworkPlayerCON.localPlayerCON.PlayerMoney < price)
                            {
                                InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                                NotInSellMode = true;
                            }
                            else if (plotClass.landCurrentUpgrade == 4)
                            {
                                InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                                NotInSellMode = true;
                            }
                            else
                                return;
                        }
                        else
                        {
                            int price = UtulitiesOfDmr.ReturnWipeCostOfPlot(plotClass);
                            if (NetworkPlayerCON.localPlayerCON.PlayerMoney < price)
                            {
                                WaitUntilOfDmr.InvokeWithDelay(this, delayedSafeAutoend, () => !NetworkSellingManager.instance.sellModeOpen);
                            }
                            else if (plotClass.landCurrentUpgrade == 4)
                            {
                                InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                            }
                            else
                                return;
                        }
                    }
                }
                break;

            case LandKind.EventLand:
                if (plotClass.landName == "Start")
                {
                    InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                }
                else if (plotClass.landName == "Teleporter")
                {
                    InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                }
                else if (plotClass.landName == "Mid Charger")
                {
                    InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                }
                else if (plotClass.landName == "Quicksand")
                {
                    InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                }
                else if (plotClass.landName == "Tree Praying Place")
                {
                    if (NetworkPlayerCON.localPlayerCON.PlayerMoney < 100000)
                    {
                        InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                        return;
                    }
                    int[] ownedPlaces = UtulitiesOfDmr.ReturnOwnedLandsIndexesByPlayerID(NetworkPlayerCON.localPlayerCON.netId).ToArray();
                    if (ownedPlaces.Length == 0)
                    {
                        InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                        return;
                    }
                }
                else if (plotClass.landName == "Unstable Hole")
                {
                    if (!NetworkSellingManager.instance.sellModeOpen)
                    {
                        InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                    }
                    else
                    {
                        WaitUntilOfDmr.InvokeWithDelay(this, delayedSafeAutoend, () => !NetworkSellingManager.instance.sellModeOpen);
                    }
                }
                break;

            case LandKind.Empty:
                InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
                break;
        }
    }
    public void SafeAutoend()
    {
        if (!automaticEnderOpen) return;
        if (QueueManager.instance.CurrentQueue != NetworkPlayerCON.localPlayerCON.netId) return;
        if (!DiceManager.instance.currentQueueAlreadyUsedDice) return;
        QueueManager.instance.requestEndTour();
    }
    private void delayedSafeAutoend()
    {
        InvokerOfDmr.InvokeWithDelay(this, SafeAutoend, 2f);
    }
}
