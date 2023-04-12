using UnityEngine;
using Mirror;
public class GoldenHitNetworked : NetworkBehaviour
{
    [SerializeField] private int hitPrizeGold;
    public uint GoldenLandsHitOwner { get; set; }
    public static GoldenHitNetworked instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMove -= listenForOnLands;
        BoardMoveManager.onMoveDone -= listenForOnLands;
    }
    public void subscribeForVoidHits()
    {
        BoardMoveManager.onMove += listenForOnLands;
        BoardMoveManager.onMoveDone += listenForOnLands;
    }
    public void unSubscribeForVoidHits()
    {
        BoardMoveManager.onMove -= listenForOnLands;
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
        if (netPc.netId != GoldenLandsHitOwner) return;
        if (plotClass.landPrices.landBiome != Biome.GoldenLand) return;
        netPc.PlayerMoney += hitPrizeGold;
    }
}
