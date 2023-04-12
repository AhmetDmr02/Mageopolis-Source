using Mirror;
using UnityEngine;

public class NetworkedVoidHit : NetworkBehaviour
{
    public uint VoidHitOwner { get; set; }

    public static NetworkedVoidHit instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= listenForOnLands;
    }
    public void subscribeForVoidHits()
    {
        BoardMoveManager.onMoveDone += listenForOnLands;
    }
    public void unSubscribeForVoidHits()
    {
        BoardMoveManager.onMoveDone -= listenForOnLands;
    }
    private void listenForOnLands(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        if (!isServer) return;
        if (boardPlayer == null) return;
        if (plotClass == null) return;
        if (boardPlayer.networkPlayerObject == null) return;
        if (UtulitiesOfDmr.ReturnCorrespondPlayerById(boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId) == null) return;
        NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId).GetComponent<NetworkPlayerCON>();
        if (netPc.netId != VoidHitOwner) return;
        if (plotClass.landName != "The Deep Abyss") return;
        if (VacuumSystem.instance.VacuumMoney < 1) return;
        int howMuchToDraw = ((VacuumSystem.instance.VacuumMoney / 100) * 40);
        LandWithrawManager.instance.WithrawAtoB(VacuumSystem.instance.gameObject, boardPlayer.gameObject, howMuchToDraw);
        VacuumSystem.instance.adjustMoney(-howMuchToDraw);
    }
}
