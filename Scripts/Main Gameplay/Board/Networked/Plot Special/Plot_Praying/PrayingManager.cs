using Mirror;
using System.Collections.Generic;
using UnityEngine;
public class PrayingManager : NetworkBehaviour
{
    [SerializeField] private int protectionCost;

    private bool prayingManagerOpen;

    public static PrayingManager instance;

    private int previousProtectedPlotIndex;
    private uint currentPrayerPlayerId = 0;

    public Plot_Praying plotPraying;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        BoardMoveManager.onMoveDone += onLandCalled;
        QueueManager.queueChanged += onQueueCalled;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= onLandCalled;
        QueueManager.queueChanged -= onQueueCalled;
    }
    private void onQueueCalled(uint playerId)
    {
        if (!isServer) return;
        if (currentPrayerPlayerId == 0) return;
        prayingManagerOpen = false;
        closePrayingInterface(UtulitiesOfDmr.ReturnCorrespondPlayerById(currentPrayerPlayerId).GetComponent<NetworkPlayerCON>().netIdentity.connectionToClient);
    }
    private void onLandCalled(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        if (!isServer) return;
        try
        {
            if (plotClass.landIndex != 20) return;
            if (boardPlayer.currentLandIndex != 20) return;
            if (prayingManagerOpen) return;
            uint playerId = boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId;

            prayingManagerOpen = true;
            currentPrayerPlayerId = playerId;

            List<int> highlightIndexes = new List<int>();
            int[] playerOwnedLands = UtulitiesOfDmr.ReturnOwnedLandsIndexesByPlayerID(playerId);

            foreach (int i in playerOwnedLands)
            {
                PlotClass _plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (_plotClass.landKind != LandKind.Buyable) continue;
                if (_plotClass.isItProtected) continue;
                highlightIndexes.Add(i);
            }
            if (highlightIndexes.Count > 0)
            {
                Debug.Log("initializing praying interface...");
                initPrayingInterface(boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netIdentity.connectionToClient, highlightIndexes.ToArray());
            }
            else
            {
                Debug.Log("Result is lesser than 0");
                prayingManagerOpen = false;
                closePrayingInterface(boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netIdentity.connectionToClient);
                currentPrayerPlayerId = 0;
            }
        }
        catch (System.Exception err)
        {
            NotificationCreator.instance.createNotification("Error", "Server has encountered major error:\n" + err);
        }
    }
    #region Commands
    [Command(requiresAuthority = false)]
    public void RequestProtectionToLand(int plotIndex, NetworkConnectionToClient conn = null)
    {
        validationResponse response = checkValidation(conn, plotIndex);
        if (!response.IsItViable)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", response.StatusMessage);
            return;
        }
        if (previousProtectedPlotIndex >= 0 && previousProtectedPlotIndex < 40)
        {
            //There should be only one barrier at the time besides chance and tree biomes
            if (BoardMoveManager.instance.LandGameobject[previousProtectedPlotIndex].GetComponent<PlotClass>().isItProtected)
            {
                Debug.Log("Wiped previous barrier");
                BoardMoveManager.instance.LandGameobject[previousProtectedPlotIndex].GetComponent<PlotClass>().RemoveBarrier();
                purgeBarrier(previousProtectedPlotIndex);
            }
        }
        previousProtectedPlotIndex = plotIndex;
        PlotClass _plotClass = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        _plotClass.isItProtected = true;
        NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>();
        netPc.PlayerMoney -= protectionCost;
        prayingManagerOpen = false;
        closePrayingInterface(conn);
        somebodyPrayedForIndex(plotIndex);
        //Sound & Visual Effects
        EffectManager.instance.createBlossomEffect(_plotClass.slidePos);
        SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.BlobBlossom, _plotClass.slidePos, 1, 1, 0.3f, 0.7f);
        InvokerOfDmr.InvokeWithDelay(EffectManager.instance, EffectManager.instance.createProtectionBlob, 1, _plotClass.slidePos, plotIndex);
    }
    [Command(requiresAuthority = false)]
    public void RequestSkipPraying(NetworkConnectionToClient conn = null)
    {
        validationResponse response = checkValidationForSkip(conn);
        if (!response.IsItViable)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", response.StatusMessage);
            return;
        }
        prayingManagerOpen = false;
        currentPrayerPlayerId = 0;
        closePrayingInterface(conn);
    }
    #endregion

    #region Target Rpcs
    [TargetRpc]
    private void initPrayingInterface(NetworkConnection conn, int[] highlightIndexes)
    {
        if (plotPraying == null)
        {
            if (BoardMoveManager.instance.LandGameobject[20].GetComponent<Plot_Praying>() == null)
                return;
        }
        plotPraying = BoardMoveManager.instance.LandGameobject[20].GetComponent<Plot_Praying>();
        plotPraying.QueueInitPrayingSequence(highlightIndexes);
    }
    [TargetRpc]
    private void closePrayingInterface(NetworkConnection conn)
    {
        if (plotPraying == null)
        {
            if (BoardMoveManager.instance.LandGameobject[20].GetComponent<Plot_Praying>() == null)
                return;
        }
        plotPraying = BoardMoveManager.instance.LandGameobject[20].GetComponent<Plot_Praying>();
        plotPraying.ClosePrayingHighlight();
    }
    #endregion

    #region Client Rpcs
    [ClientRpc]
    private void somebodyPrayedForIndex(int plotIndex)
    {
        if (isServer) return;
        try
        {
            PlotClass _plotClass = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
            _plotClass.isItProtected = true;
            //Sound & Visual Effects
            EffectManager.instance.createBlossomEffect(_plotClass.slidePos);
            SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.BlobBlossom, _plotClass.slidePos, 1, 1, 0.3f, 0.7f);
            InvokerOfDmr.InvokeWithDelay(EffectManager.instance, EffectManager.instance.createProtectionBlob, 1, _plotClass.slidePos, plotIndex);
        }
        catch { Debug.Log("Error occured on praying rpc"); }
    }
    [ClientRpc]
    private void purgeBarrier(int plotIndex)
    {
        if (isServer) return;
        try
        {
            if (plotIndex >= 0 && plotIndex < 40)
            {
                //There should be only one barrier at the time besides chance and tree biomes
                if (BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>().isItProtected)
                {
                    BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>().RemoveBarrier();
                }
            }
        }
        catch { Debug.Log("Error occured on purge barrier."); }
    }
    #endregion
    private validationResponse checkValidation(NetworkConnectionToClient conn, int plotIndex)
    {
        validationResponse response = new validationResponse("Success", true);
        if (conn == null)
        {
            response.StatusMessage = "Connection was null";
            response.IsItViable = false;
            return response;
        }
        if (plotIndex > 39 || plotIndex < 0)
        {
            response.StatusMessage = "Index trying to going outside of bounds";
            response.IsItViable = false;
            return response;
        }
        GameObject playerObject = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId);
        if (playerObject == null)
        {
            response.StatusMessage = "Cannot find player object!";
            response.IsItViable = false;
            return response;
        }
        if (playerObject.GetComponent<NetworkPlayerCON>() == null)
        {
            response.StatusMessage = "Cannot find player class!";
            response.IsItViable = false;
            return response;
        }
        NetworkPlayerCON netPc = playerObject.GetComponent<NetworkPlayerCON>();
        if (netPc.PlayerMoney < protectionCost)
        {
            response.StatusMessage = "You don't have enough money to do this action";
            response.IsItViable = false;
            return response;
        }
        PlotClass _plotClass = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        if (_plotClass.ownedBy != conn.identity.netId)
        {
            response.StatusMessage = "This land is not yours!";
            response.IsItViable = false;
            return response;
        }
        if (_plotClass.landKind != LandKind.Buyable)
        {
            response.StatusMessage = "This land is not eligible!";
            response.IsItViable = false;
            return response;
        }
        if (_plotClass.isItProtected)
        {
            response.StatusMessage = "This land is already protected";
            response.IsItViable = false;
            return response;
        }
        if (QueueManager.instance.CurrentQueue != netPc.netId)
        {
            response.StatusMessage = "Wait for your queue!";
            response.IsItViable = false;
            return response;
        }
        if (!DiceManager.instance.currentQueueAlreadyUsedDice)
        {
            response.StatusMessage = "You need to roll your dice!";
            response.IsItViable = false;
            return response;
        }
        return response;
    }
    private validationResponse checkValidationForSkip(NetworkConnectionToClient conn)
    {
        validationResponse response = new validationResponse("Success", true);
        if (conn == null)
        {
            response.StatusMessage = "Connection was null";
            response.IsItViable = false;
            return response;
        }
        GameObject playerObject = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId);
        if (playerObject == null)
        {
            response.StatusMessage = "Cannot find player object!";
            response.IsItViable = false;
            return response;
        }
        if (playerObject.GetComponent<NetworkPlayerCON>() == null)
        {
            response.StatusMessage = "Cannot find player class!";
            response.IsItViable = false;
            return response;
        }
        NetworkPlayerCON netPc = playerObject.GetComponent<NetworkPlayerCON>(); ;
        if (QueueManager.instance.CurrentQueue != netPc.netId)
        {
            response.StatusMessage = "Wait for your queue!";
            response.IsItViable = false;
            return response;
        }
        if (!DiceManager.instance.currentQueueAlreadyUsedDice)
        {
            response.StatusMessage = "You need to roll your dice!";
            response.IsItViable = false;
            return response;
        }
        return response;
    }
    private struct validationResponse
    {
        public string StatusMessage;
        public bool IsItViable;
        public validationResponse(string statusMessage, bool isItViable)
        {
            StatusMessage = statusMessage;
            IsItViable = isItViable;
        }
    }
}
