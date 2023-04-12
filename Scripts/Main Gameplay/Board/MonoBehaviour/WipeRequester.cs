using UnityEngine;
using TMPro;
using System.Linq;

public class WipeRequester : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;

    private Animator localAnimator;
    private bool isRequesterOpen;
    private bool isItHighlighted;

    private NetworkPlayerCON netPC;
    /// <summary>
    /// TODO: Check after money withrawal
    /// </summary>
    private void Start()
    {
        BoardMoveManager.onMoveDone += checkValidation;
        RaycastCenter.lookingObjectChanged += listenForRaycastHighlight;
        RaycastCenter.lookingObjectLeftClicked += listenForRaycastClicks;
        QueueManager.queueChanged += queueChanged;
        localAnimator = GetComponent<Animator>();
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= checkValidation;
        RaycastCenter.lookingObjectChanged -= listenForRaycastHighlight;
        RaycastCenter.lookingObjectLeftClicked -= listenForRaycastClicks;
        if (NetworkPlayerCON.localPlayerCON != null)
            NetworkPlayerCON.localPlayerCON.playerMoneyChanged -= playerMoneyChanged;
    }
    private void FixedUpdate()
    {
        if (isItHighlighted)
            infoText.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y - 15, Input.mousePosition.z);

        //Cannot assign on awake beacause of mirror 
        if (netPC == null)
        {
            if (NetworkPlayerCON.localPlayerCON != null)
            {
                netPC = NetworkPlayerCON.localPlayerCON;
                NetworkPlayerCON.localPlayerCON.playerMoneyChanged += playerMoneyChanged;
            }
        }
    }
    private void playerMoneyChanged(int oldMoney, int newMoney)
    {
        if (!isRequesterOpen) return;
        if (currentPlotClass == null) return;
        if (UtulitiesOfDmr.ReturnWipeCostOfPlot(currentPlotClass) > newMoney)
        {
            //After Player Money Withrawal
            switchOffWipeRequester();
        }
    }
    private void queueChanged(uint id)
    {
        if (id != NetworkPlayerCON.localPlayerCON.netId && isRequesterOpen)
            switchOffWipeRequester();
    }
    private PlotClass currentPlotClass;
    private void checkValidation(PlotClass plotClass_, BoardPlayer boardPlayer_)
    {
        if (boardPlayer_.networkPlayerObject != null)
        {
            if (boardPlayer_.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId != NetworkPlayerCON.localPlayerCON.netId)
            {
                if (isRequesterOpen)
                {
                    switchOffWipeRequester();
                }
                return;
            }
        }
        if (plotClass_.landKind != LandKind.Buyable)
        {
            if (isRequesterOpen)
            {
                switchOffWipeRequester();
            }
            return;
        }
        if (plotClass_.landCurrentUpgrade == 4)
        {
            if (isRequesterOpen)
            {
                switchOffWipeRequester();
            }
            return;
        }
        if (plotClass_.ownedBy == 0)
        {
            if (isRequesterOpen)
            {
                switchOffWipeRequester();
            }
            return;
        }
        if (plotClass_.ownedBy == NetworkPlayerCON.localPlayerCON.netId)
        {
            if (isRequesterOpen)
            {
                switchOffWipeRequester();
            }
            return;
        }
        int wipeCost = UtulitiesOfDmr.ReturnWipeCostOfPlot(plotClass_);
        if (NetworkPlayerCON.localPlayerCON.PlayerMoney < wipeCost)
        {
            if (isRequesterOpen)
            {
                switchOffWipeRequester();
            }
            return;
        }
        int[] playerOwnedPlots = UtulitiesOfDmr.ReturnOwnedLandsIndexesByPlayerID(NetworkPlayerCON.localPlayerCON.netId);
        //Academy Indexes
        if (!playerOwnedPlots.Contains(11) && !playerOwnedPlots.Contains(12) && !playerOwnedPlots.Contains(13))
        {
            if (isRequesterOpen)
            {
                switchOffWipeRequester();
            }
            return;
        }
        currentPlotClass = plotClass_;
        switchOnWipeRequester();
    }
    private void listenForRaycastHighlight(GameObject go)
    {
        if (!isRequesterOpen) return;
        if (go != this.gameObject)
        {
            infoText.enabled = false;
            isItHighlighted = false;
            return;
        }
        else
        {
            infoText.enabled = true;
            isItHighlighted = true;
            bool isItNull = UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId) == null ? true : false;
            int totalWipeCost = isItNull ? 0 : UtulitiesOfDmr.ReturnWipeCostOfPlot(UtulitiesOfDmr.ReturnPlotClassByIndex(UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).GetComponent<BoardPlayer>().currentLandIndex));
            infoText.text = $"Wipe Current Landed Plot:\n Price: {totalWipeCost} \n Click To Wipe!";
        }
    }
    private void listenForRaycastClicks(GameObject go)
    {
        if (go != this.gameObject) return;
        if (!isRequesterOpen) return;
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId);
        if (boardPlayer == null) return;
        SoundEffectManager.instance.PlayClickSound();
        BoardWipeManager.instance.RequestWipePlot(boardPlayer.currentLandIndex);
        switchOffWipeRequester();
    }
    private void switchOffWipeRequester()
    {
        SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeShort, this.transform.position, 1, 1, 0.1f, 0.8f);
        isRequesterOpen = false;
        infoText.enabled = false;
        isItHighlighted = false;
        currentPlotClass = null;
        localAnimator.Play("Wipe_Req_Off_Init");
    }
    private void switchOnWipeRequester()
    {
        Debug.Log("Dummy Audio Created.");
        SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeShort, this.transform.position, 1, 1, 0.1f, 0.8f);
        isRequesterOpen = true;
        localAnimator.Play("Wipe_Req_On_Init");
    }
}
