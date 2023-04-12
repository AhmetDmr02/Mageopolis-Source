using UnityEngine;
using Mirror;

public class ChanceManager : NetworkBehaviour
{
    [SerializeField] private ChanceInstance[] chanceInstances;
    public ChanceInstance[] ChanceInstances => chanceInstances;

    public static ChanceManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        BoardMoveManager.onMoveDone += checkPlayerLand;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= checkPlayerLand;
    }
    #region Auto Checks
    private void checkPlayerLand(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        if (!isServer) return;
        if (plotClass.landName != "Chance") return;
        if (QueueManager.instance.CurrentQueue != boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId) return;
        if (!DiceManager.instance.currentQueueAlreadyUsedDice) return;
        chanceInstances.Shuffle();
        chanceCardRecieved(chanceInstances[0].ChanceEffectId);
        QueueManager.instance.cannotEndTour = true;
        InvokerOfDmr.InvokeWithDelay(this, fixQueueManager, 7, chanceInstances[0].ChanceEffectId);
        InvokerOfDmr.InvokeWithDelay(ChanceEffects.instance, ChanceEffects.instance.PlayChanceEffect, 5, boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId, chanceInstances[0].ChanceEffectId);
        InvokerOfDmr.InvokeWithDelay(ChanceEffects.instance, () => { QueueManager.instance.cannotEndTour = false; }, 5.1f);
    }
    //Id is just for debugging
    private void fixQueueManager(int Id)
    {
        if (chanceInstances[0].shouldPlayerRecheckEndTourButton)
        {
            Debug.Log("Recheck Button Sent");
            checkButton(Id);
            if (ConfigPiercer.instance.shouldPlayersEnd)
            {
                checkAutoCheck();
            }
        }
    }
    #endregion
    [ClientRpc]
    private void chanceCardRecieved(int Id)
    {
        BoardRefrenceHolder.instance.chanceVisualiser.VisualizeCard(Id);
    }
    [ClientRpc]
    private void checkButton(int id)
    {
        Debug.Log("Check Button Recieved Of Id" + id);
        EndTourButton.CheckButton(null, null);
    }
    [ClientRpc]
    private void checkAutoCheck()
    {
        InvokerOfDmr.InvokeWithDelay(AutomaticQueueEnder.instance, AutomaticQueueEnder.instance.SafeAutoend, 2f);
    }
}
