using Mirror;
using System;
using UnityEngine;

public class LandBuyManager : NetworkBehaviour
{
    public static LandBuyManager instance;
    public static event Action<int, uint> PlayerBoughtLand;
    public static event Action<int, int, uint> PlayerUpgradedLand;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private int getUpgradeCosts(PlotClass _plotClass, int priceForMonolith)
    {
        int total = 0;
        int currentUpgrade = _plotClass.landCurrentUpgrade - 1;
        LandInstance _land = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        for (int i = 0; i <= priceForMonolith; i++)
        {
            if (currentUpgrade >= i) continue;
            total += _land.landPrices.landUpgradePrices[i];
        }
        return total;
    }
    [Command(requiresAuthority = false)]
    public void RequestBuyLand(int landIndex, NetworkConnectionToClient conn = null)
    {
        #region Checking For Eligibility
        if (UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>() == null)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Server Cannot Find Corresponding Player!");
            return;
        }
        //There are total of 40 plots and beacause of we don't want to go out of bounds i limited this
        if (landIndex > 39 || landIndex < 0)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Don't try to cheat in my game, why you trying to break my game its already falling apart :(");
            return;
        }
        NetworkPlayerCON _networkPlayerCon = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>();
        BoardPlayer _boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(conn.identity.netId);
        PlotClass _plotClass = BoardMoveManager.instance.LandGameobject[landIndex].GetComponent<PlotClass>();
        if (_plotClass == null)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Plot Class Couldn't Found");
            return;
        }
        if (_boardPlayer == null)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Board Player Was Null.");
            return;
        }
        if (_plotClass.landKind != LandKind.Buyable)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "This Plot Is Not Buyable");
            return;
        }
        if (QueueManager.instance.CurrentQueue != conn.identity.netId)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You are not the current queue.");
            return;
        }
        if (!DiceManager.instance.currentQueueAlreadyUsedDice)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Use your dice.");
            return;
        }
        if (_boardPlayer.currentLandIndex != landIndex)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Your board player is not in that plot?");
            return;
        }
        if (_plotClass.ownedBy == conn.identity.netId)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You already own this place");
            return;
        }
        if (_plotClass.ownedBy != 0 && _plotClass.ownedBy != conn.identity.netId)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You don't own this place");
            return;
        }
        if (_boardPlayer.networkPlayerObject != _networkPlayerCon.gameObject)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Network PlayerCON Compare Failed!");
            return;
        }
        if (_networkPlayerCon.PlayerMoney < BoardMoveManager.instance.LandInstance[landIndex].landPrices.landBuyPrice)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You don't have enough gold to buy!");
            return;
        }
        #endregion

        _plotClass.ChangeOwnership(conn.identity.netId);
        _networkPlayerCon.PlayerMoney -= BoardMoveManager.instance.LandInstance[landIndex].landPrices.landBuyPrice;
        string boughtTextString = $"{_networkPlayerCon.PlayerName} Bought {_plotClass.landName} For {BoardMoveManager.instance.LandInstance[landIndex].landPrices.landBuyPrice} Gold!";
        playerBoughtLand(landIndex, conn.identity.netId);
        AnimatedTextCreator.instance.CreateAnimatedText(boughtTextString, UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>().playerColor);
    }
    [Command(requiresAuthority = false)]
    public void RequestUpgradeLand(int landIndex, int UpgradeIndex, NetworkConnectionToClient conn = null)
    {
        #region Checking For Eligibility
        if (UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>() == null)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Server Cannot Find Corresponding Player!");
            return;
        }
        if (landIndex > 39 || landIndex < 0)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Don't try to cheat in my game, why you trying to break my game its already falling apart :(");
            return;
        }
        if (UpgradeIndex > 3)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Don't try to cheat in my game, why you trying to break my game its already falling apart :(");
            return;
        }
        NetworkPlayerCON _networkPlayerCon = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>();
        BoardPlayer _boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(conn.identity.netId);
        PlotClass _plotClass = BoardMoveManager.instance.LandGameobject[landIndex].GetComponent<PlotClass>();
        int upgradeCost = getUpgradeCosts(_plotClass, UpgradeIndex);
        if (_plotClass == null)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Plot Class Couldn't Found");
            return;
        }
        if (_boardPlayer == null)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Board Player Was Null.");
            return;
        }
        if (_plotClass.landKind != LandKind.Buyable)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "This Plot Is Not Buyable");
            return;
        }
        if (QueueManager.instance.CurrentQueue != conn.identity.netId)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You are not the current queue.");
            return;
        }
        if (!DiceManager.instance.currentQueueAlreadyUsedDice)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Use your dice.");
            return;
        }
        if (_boardPlayer.currentLandIndex != landIndex)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Your board player is not in that plot?");
            return;
        }
        if (_plotClass.landCurrentUpgrade - 1 >= UpgradeIndex)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "This monolith is already upgraded.");
            return;
        }
        if (_plotClass.ownedBy != 0 && _plotClass.ownedBy != conn.identity.netId)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You don't own this place");
            return;
        }
        if (_boardPlayer.networkPlayerObject != _networkPlayerCon.gameObject)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "Network PlayerCON Compare Failed!");
            return;
        }
        if (_networkPlayerCon.PlayerMoney < upgradeCost)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You don't have enough gold to buy!");
            return;
        }
        #endregion

        _plotClass.landCurrentUpgrade = UpgradeIndex + 1;
        _networkPlayerCon.PlayerMoney -= upgradeCost;
        string boughtTextString = $"{_networkPlayerCon.PlayerName} Upgraded {_plotClass.landName} For {upgradeCost} Gold!";
        playerUpgradedLand(landIndex, UpgradeIndex + 1, conn.identity.netId);
        AnimatedTextCreator.instance.CreateAnimatedText(boughtTextString, UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>().playerColor);
    }
    [ClientRpc]
    private void playerUpgradedLand(int _landIndex, int UpgradeIndex, uint connectionId)
    {
        try
        {
            BoardMoveManager.instance.LandGameobject[_landIndex].GetComponent<PlotClass>().landCurrentUpgrade = UpgradeIndex;
            PlayerUpgradedLand?.Invoke(_landIndex, UpgradeIndex, connectionId);
            //Maybe Fire Some Effects Later;
        }
        catch (System.Exception err)
        {
            Debug.LogWarning(err);
            return;
        }
    }
    [ClientRpc]
    private void playerBoughtLand(int _landIndex, uint connectionId)
    {
        try
        {
            BoardMoveManager.instance.LandGameobject[_landIndex].GetComponent<PlotClass>().ownedBy = connectionId;
            PlayerBoughtLand?.Invoke(_landIndex, connectionId);
            //Maybe Fire Some Effects Later;
        }
        catch (System.Exception err)
        {
            Debug.LogWarning(err);
            return;
        }
    }

    public void InvokeRecalculateAction(int plotIndex, int upgradeIndex, uint playerId)
    {
        PlayerUpgradedLand?.Invoke(plotIndex, upgradeIndex, playerId);
    }
}
