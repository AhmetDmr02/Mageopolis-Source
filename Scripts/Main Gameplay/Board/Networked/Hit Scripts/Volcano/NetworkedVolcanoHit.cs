using UnityEngine;
using Mirror;

public class NetworkedVolcanoHit : NetworkBehaviour
{
    [SyncVar]
    public uint volcanoOwnerId;

    [SyncVar] public bool IsHit;

    [SyncVar] public bool currentQueueUsedCannon;

    #region Instance
    public static NetworkedVolcanoHit instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    #endregion

    private void Start()
    {
        QueueManager.queueChanged += resetCurrentQueue;
    }
    private void OnDestroy()
    {
        QueueManager.queueChanged -= resetCurrentQueue;
    }
    private void resetCurrentQueue(uint playerId)
    {
        currentQueueUsedCannon = false;
    }

    #region Request Wipe With Cannon
    [Command(requiresAuthority = false)]
    public void RequestLandWipe(int plotIndex, NetworkConnectionToClient conn = null)
    {
        if (conn == null) return;
        if (volcanoOwnerId == 0) return;
        if (currentQueueUsedCannon) return;
        if (conn.identity.netId != volcanoOwnerId) return;
        if (QueueManager.instance.CurrentQueue != conn.identity.netId) return;
        if (!DiceManager.instance.currentQueueAlreadyUsedDice) return;
        if (UtulitiesOfDmr.ReturnBoardPlayerById(conn.identity.netId).currentLandIndex != 38) return;
        if (plotIndex < 0 || plotIndex > 39) return;
        PlotClass plotClass = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        if (plotClass.landKind != LandKind.Buyable) return;
        if (plotClass.ownedBy == 0) return;
        if (plotClass.ownedBy == conn.identity.netId) return;
        if (plotClass.landCurrentUpgrade == 4) return;
        currentQueueUsedCannon = true;
        volcanoBallEffect();
        InvokerOfDmr.InvokeWithDelay(BoardWipeManager.instance, BoardWipeManager.instance.ServerWipeLand, 2, plotIndex);
    }
    [ClientRpc]
    private void volcanoBallEffect()
    {
        BoardRefrenceHolder.instance.volcanoBallMuzzle.Play();
        Vector3 soundPosition = BoardRefrenceHolder.instance.middleChargerAnimator.gameObject.transform.position;
        SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.FireballSound, soundPosition, 1, 1, 0.7f, 0.7f);
    }
    #endregion
}
