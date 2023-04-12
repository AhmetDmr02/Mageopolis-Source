using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class VolcanoHit : MonoBehaviour, IHitSpecial
{
    [SerializeField] private LandInstance[] hitDoublerGroup;
    [SerializeField] private TextMeshProUGUI inspectText;
    private bool isCannonActive = false;

    private void Start()
    {
        RaycastCenter.lookingObjectChanged += listenForText;
        RaycastCenter.lookingObjectLeftClicked += toggleHighlighterForCannon;
        QueueManager.queueChanged += resetCannonActive;
    }
    private void OnDestroy()
    {
        RaycastCenter.lookingObjectChanged -= listenForText;
        RaycastCenter.lookingObjectLeftClicked -= toggleHighlighterForCannon;
        QueueManager.queueChanged -= resetCannonActive;
    }

    #region Raycasting Section
    private void listenForText(GameObject go)
    {
        if (MainGameManager.instance.serverGameState != MainGameManager.ServerGameState.Ingame) { inspectText.text = ""; return; };
        if (go.gameObject.transform.tag != "HellBallCannon") { inspectText.text = ""; return; };
        if (isCannonActive) { inspectText.text = ""; return; };
        if (NetworkedVolcanoHit.instance.currentQueueUsedCannon) { inspectText.text = ""; return; };
        if (UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentLandIndex != 38) { inspectText.text = ""; return; };
        if (QueueManager.instance.CurrentQueue != NetworkPlayerCON.localPlayerCON.netId) { inspectText.text = ""; return; };
        if (BoardMoveManager.instance.LandGameobject[38].GetComponent<PlotClass>().ownedBy != NetworkPlayerCON.localPlayerCON.netId) { inspectText.text = ""; return; };
        if (!NetworkedVolcanoHit.instance.IsHit) { inspectText.text = ""; return; };
        inspectText.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y - 15, Input.mousePosition.z);
        inspectText.enabled = true;
        inspectText.text = "Click to use cannon";
    }
    private void resetCannonActive(uint playerId)
    {
        if (isCannonActive)
        {
            if (Highlighter.instance.IsInHighlightMode) Highlighter.instance.CloseHighlightMode();
            isCannonActive = false;
        };
    }
    #endregion
    private void toggleHighlighterForCannon(GameObject go)
    {
        if (go.gameObject.transform.tag != "HellBallCannon") return;
        if (isCannonActive) { Highlighter.instance.CloseHighlightMode(); isCannonActive = false; listenForText(go); return; }
        if (NetworkedVolcanoHit.instance.volcanoOwnerId == 0) return;
        if (NetworkedVolcanoHit.instance.volcanoOwnerId != NetworkPlayerCON.localPlayerCON.netId) return;
        if (QueueManager.instance.CurrentQueue != NetworkPlayerCON.localPlayerCON.netId) return;
        if (!DiceManager.instance.currentQueueAlreadyUsedDice) return;
        if (Highlighter.instance.IsInHighlightMode) return;
        if (NetworkedVolcanoHit.instance.currentQueueUsedCannon) return;
        if (UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentLandIndex != 38) return;
        listenForText(go);
        isCannonActive = true;
        List<PlotClass> plotClasses = new List<PlotClass>();
        foreach (GameObject gameObj in BoardMoveManager.instance.LandGameobject)
        {
            PlotClass plotClass = gameObj.GetComponent<PlotClass>();
            if (plotClass.landKind != LandKind.Buyable) continue;
            if (plotClass.ownedBy == 0) continue;
            if (plotClass.ownedBy == NetworkPlayerCON.localPlayerCON.netId) continue;
            if (plotClass.landCurrentUpgrade == 4) continue;
            plotClasses.Add(plotClass);
        }
        if (plotClasses.Count > 0)
        {
            Highlighter.instance.SwitchToHighlightMode(plotClasses.ToArray(), false);
            Highlighter.HighlighterClickCallback += requestLandWipe;
        }
        else
        {
            NotificationCreator.instance.createNotification("Notification", "Cannot find any targetable plots.");
        }
    }
    private void requestLandWipe(PlotClass[] plotClass)
    {
        isCannonActive = false;
        Highlighter.HighlighterClickCallback -= requestLandWipe;
        Highlighter.instance.CloseHighlightMode();
        NetworkedVolcanoHit.instance.RequestLandWipe(plotClass[0].landIndex);
    }

    public void WhenHit(uint playerId)
    {
        NetworkedVolcanoHit.instance.volcanoOwnerId = playerId;
        NetworkedVolcanoHit.instance.IsHit = true;
        foreach (LandInstance landInstance in hitDoublerGroup)
        {
            for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
            {
                PlotClass plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (plotClass.landName == landInstance.landName)
                {
                    plotClass.landPrices.isLandHit = true;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    HitNetworkManager.instance.RpcVolcanoAnimationToggle(true, gameObject.transform.position);
                    HitNetworkManager.instance.RpcHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, true);
                }
            }
        }
    }
    public void WhenUnhit(uint playerId)
    {
        NetworkedVolcanoHit.instance.volcanoOwnerId = 0;
        NetworkedVolcanoHit.instance.IsHit = false;
        foreach (LandInstance landInstance in hitDoublerGroup)
        {
            for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
            {
                PlotClass plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (plotClass.landName == landInstance.landName)
                {
                    plotClass.landPrices.isLandHit = false;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    HitNetworkManager.instance.RpcVolcanoAnimationToggle(false, gameObject.transform.position);
                    HitNetworkManager.instance.RpcHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, false);
                }
            }
        }
    }
}
