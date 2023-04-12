using UnityEngine;
using EZCameraShake;
public class TreeHitSpecial : MonoBehaviour, IHitSpecial
{
    [SerializeField] private LandInstance[] hitDoublerGroup;
    [SerializeField] private Animator treeAnimator;
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
                    plotClass.isItProtected = true;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    treeAnimator.Play("Main_Tree_Growing");
                    CameraShaker.Instance.ShakeOnce(0.4f, 0.3f, 5, 15);
                    //Dome SFX & VFX
                    EffectManager.instance.createBlossomEffect(plotClass.slidePos);
                    SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeLong, plotClass.slidePos, 1, 1, 0.5f, 0.7f);
                    SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.BlobBlossom, plotClass.slidePos, 1, 1, 0.3f, 0.7f);
                    InvokerOfDmr.InvokeWithDelay(EffectManager.instance, EffectManager.instance.createProtectionBlob, 1, plotClass.slidePos, plotClass.landIndex);
                    //Rpc call for client side calculations
                    HitNetworkManager.instance.RpcTreeHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, true);
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
                    plotClass.isItProtected = false;
                    LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
                    treeAnimator.Play("Main_Tree_Dying");
                    //Earthquake Effects Here
                    SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeLong, plotClass.slidePos, 1, 1, 0.5f, 0.7f);
                    CameraShaker.Instance.ShakeOnce(0.4f, 0.3f, 5, 15);
                    //Remove Dome
                    plotClass.RemoveBarrier();
                    HitNetworkManager.instance.RpcTreeHitChanged(plotClass.landIndex, plotClass.landCurrentUpgrade, playerId, false);
                }
            }
        }
    }
}
