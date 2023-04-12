using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GoldenHit : MonoBehaviour, IHitSpecial
{
    [SerializeField] private LandInstance[] hitDoublerGroup;
    public void WhenHit(uint playerId)
    {
        GoldenHitNetworked.instance.GoldenLandsHitOwner = playerId;
        GoldenHitNetworked.instance.subscribeForVoidHits();
        foreach (LandInstance landInstance in hitDoublerGroup)
        {
            for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
            {
                PlotClass plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (plotClass.landName == landInstance.landName)
                {
                    plotClass.landPrices.isLandHit = true;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    HitNetworkManager.instance.RpcHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, true);
                    plotClass.GetComponent<MainLandEffectPlayer>().enable = true;
                    HitNetworkManager.instance.RpcToggleLandEffect(true, plotClass.landIndex);
                }
            }
        }
    }
    public void WhenUnhit(uint playerId)
    {
        GoldenHitNetworked.instance.GoldenLandsHitOwner = 0;
        GoldenHitNetworked.instance.unSubscribeForVoidHits();
        foreach (LandInstance landInstance in hitDoublerGroup)
        {
            for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
            {
                PlotClass plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (plotClass.landName == landInstance.landName)
                {
                    plotClass.landPrices.isLandHit = false;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    HitNetworkManager.instance.RpcHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, false);
                    plotClass.GetComponent<MainLandEffectPlayer>().enable = false;
                    HitNetworkManager.instance.RpcToggleLandEffect(false, plotClass.landIndex);
                }
            }
        }
    }
}
