using UnityEngine;

public class GeneralHitDoubler : MonoBehaviour, IHitSpecial
{
    //This scripts will only active isHit property on PlotClass.landPrices
    [SerializeField] private LandInstance[] hitDoublerGroup;
    public void WhenHit(uint playerId)
    {
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
                }
            }
        }
    }
    public void WhenUnhit(uint playerId)
    {
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
                }
            }
        }
    }
}
