using UnityEngine;

public class VoidHit : MonoBehaviour, IHitSpecial
{
    [SerializeField] private LandInstance[] hitDoublerGroup;

    public void WhenHit(uint playerId)
    {
        NetworkedVoidHit.instance.VoidHitOwner = playerId;
        NetworkedVoidHit.instance.subscribeForVoidHits();
        foreach (LandInstance landInstance in hitDoublerGroup)
        {
            for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
            {
                PlotClass plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (plotClass.landName == landInstance.landName)
                {
                    plotClass.landPrices.isLandHit = true;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    BoardRefrenceHolder.instance.vacuumEffect.SetActive(true);
                    BoardRefrenceHolder.instance.vacuumEffect.GetComponent<ParticleSystem>().Play();
                    HitNetworkManager.instance.RpcToggleVoidObject(true);
                    HitNetworkManager.instance.RpcHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, true);
                }
            }
        }
    }
    public void WhenUnhit(uint playerId)
    {
        NetworkedVoidHit.instance.VoidHitOwner = 0;
        NetworkedVoidHit.instance.unSubscribeForVoidHits();
        foreach (LandInstance landInstance in hitDoublerGroup)
        {
            for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
            {
                PlotClass plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (plotClass.landName == landInstance.landName)
                {
                    plotClass.landPrices.isLandHit = false;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    BoardRefrenceHolder.instance.vacuumEffect.SetActive(false);
                    HitNetworkManager.instance.RpcToggleVoidObject(false);
                    HitNetworkManager.instance.RpcHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, false);
                }
            }
        }
    }
}
