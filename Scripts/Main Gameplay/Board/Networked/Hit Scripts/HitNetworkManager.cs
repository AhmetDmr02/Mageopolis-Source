using EZCameraShake;
using Mirror;
using UnityEngine;

public class HitNetworkManager : NetworkBehaviour
{
    public static HitNetworkManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    [ClientRpc]
    public void RpcHitChanged(int plotIndex, int upgradeIndex, uint playerId, bool isHit)
    {
        if (isServer) return;
        if (plotIndex > 39 || plotIndex < 0) return;
        PlotClass plotClass = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        plotClass.landPrices.isLandHit = isHit;
        LandBuyManager.instance.InvokeRecalculateAction(plotIndex, upgradeIndex, playerId);
    }
    [ClientRpc]
    public void RpcTreeHitChanged(int plotIndex, int upgradeIndex, uint playerId, bool isHit)
    {
        if (isServer) return;
        if (plotIndex > 39 || plotIndex < 0) return;
        PlotClass plotClass = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        if (isHit)
        {
            plotClass.landPrices.isLandHit = true;
            plotClass.isItProtected = true;
            LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
            BoardRefrenceHolder.instance.treeAnimator.Play("Main_Tree_Growing");
            CameraShaker.Instance.ShakeOnce(0.4f, 0.3f, 5, 15);
            //Dome SFX & VFX
            EffectManager.instance.createBlossomEffect(plotClass.slidePos);
            SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeLong, plotClass.slidePos, 1, 1, 0.5f, 0.7f);
            SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.BlobBlossom, plotClass.slidePos, 1, 1, 0.3f, 0.7f);
            InvokerOfDmr.InvokeWithDelay(EffectManager.instance, EffectManager.instance.createProtectionBlob, 1, plotClass.slidePos, plotClass.landIndex);
        }
        else
        {
            plotClass.landPrices.isLandHit = false;
            plotClass.isItProtected = false;
            LandBuyManager.instance.InvokeRecalculateAction(plotClass.landIndex, plotClass.landCurrentUpgrade, plotClass.ownedBy);
            BoardRefrenceHolder.instance.treeAnimator.Play("Main_Tree_Dying");
            //Earthquake Effects Here
            SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeLong, plotClass.slidePos, 1, 1, 0.5f, 0.7f);
            CameraShaker.Instance.ShakeOnce(0.4f, 0.3f, 5, 15);
            //Remove Dome
            plotClass.RemoveBarrier();
        }
    }
    [ClientRpc]
    public void RpcToggleVoidObject(bool value)
    {
        if (isServer) return;
        BoardRefrenceHolder.instance.vacuumEffect.SetActive(value);
        if (value) BoardRefrenceHolder.instance.vacuumEffect.GetComponent<ParticleSystem>().Play();
    }
    [ClientRpc]
    public void RpcToggleLandEffect(bool value, int plotIndex)
    {
        if (isServer) return;
        if (plotIndex > 39 || plotIndex < 0) return;
        GameObject go = BoardMoveManager.instance.LandGameobject[plotIndex];
        if (go.GetComponent<MainLandEffectPlayer>() == null) return;
        go.GetComponent<MainLandEffectPlayer>().enable = value;
    }
    [ClientRpc]
    public void RpcVolcanoAnimationToggle(bool toggle, Vector3 soundLocation)
    {
        if (toggle)
        {
            BoardRefrenceHolder.instance.volcanoAnimator.Play("Hell_Ball_Raising");
            BoardRefrenceHolder.instance.volcanoBallEffect.Play();
            SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeMiddleCharger, soundLocation, 1, 1, 1, 0.85f);
            CameraShaker.Instance.ShakeOnce(0.4f, 0.5f, 4f, 4f);
        }
        else
        {
            BoardRefrenceHolder.instance.volcanoAnimator.Play("Hell_Ball_Going_Down");
            BoardRefrenceHolder.instance.volcanoBallEffect.Play();
            SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeMiddleCharger, soundLocation, 1, 1, 1, 0.85f);
            CameraShaker.Instance.ShakeOnce(0.4f, 0.5f, 4f, 4f);
        }
    }
}
