using UnityEngine;
using Mirror;
using EZCameraShake;
using System;
using System.Collections.Generic;
using System.Linq;

public class Plot_Middlecharger : NetworkBehaviour
{
    [SyncVar]
    public int PlotChargerIndex;

    private void Start()
    {
        BoardMoveManager.onMoveDone += addCharge;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= addCharge;
    }
    private void addCharge(PlotClass _plotClass, BoardPlayer _boardPlayer)
    {
        if (!isServer) return;
        if (_plotClass.landName != "Mid Charger") return;
        //8 Is limit
        if (PlotChargerIndex < 7)
        {
            PlotChargerIndex += 1;
            plotChargerChanged(PlotChargerIndex);
            playerMiddleChargerSFX();
        }
        else
        {
            PlotChargerIndex = 0;
            plotChargerChanged(PlotChargerIndex);
            //Lets get all the plots besides start,teleporter,tree praying,quicksand and all middle chargers
            int[] forbiddenIndexes = { 0, 5, 10, 15, 20, 28, 30, 35 };
            List<int> possibleIndexes = new List<int>();
            foreach (GameObject go in BoardMoveManager.instance.LandGameobject)
            {
                if (go.GetComponent<PlotClass>() == null) return;
                if (go.GetComponent<PlotClass>().landKind != LandKind.Buyable) continue;
                if (go.GetComponent<PlotClass>().ownedBy == 0) continue;
                PlotClass plotClass_ = go.GetComponent<PlotClass>();
                if (forbiddenIndexes.Contains(plotClass_.landIndex)) continue;
                possibleIndexes.Add(plotClass_.landIndex);
            }
            BoardRefrenceHolder.instance.middleChargerAnimator.Play("MidCharger_Fire");
            if (possibleIndexes.Count < 1) return;
            possibleIndexes.Shuffle();
            int wipeIndex = possibleIndexes[0];
            middleChargerFired(wipeIndex);

            //Server Side Effects
            playVFX();
            InvokerOfDmr.InvokeWithDelay(BoardWipeManager.instance, BoardWipeManager.instance.ServerWipeLand, 3, wipeIndex);

        }
    }
    [ClientRpc]
    private void plotChargerChanged(int animIndex)
    {
        if (BoardRefrenceHolder.instance == null) return;
        BoardRefrenceHolder.instance.middleChargerAnimator.SetInteger("MidChargerIndex", animIndex);
    }
    [ClientRpc]
    private void middleChargerFired(int landIndex)
    {
        if (isServer) return;
        if (landIndex < 0 || landIndex > 39) return;
        BoardRefrenceHolder.instance.middleChargerAnimator.Play("MidCharger_Fire");
        playVFX();
    }
    private void playVFX()
    {
        EffectManager.instance.PlayMiddleChargerEffect();
        Action shakeCamera = () => { CameraShaker.Instance.ShakeOnce(1, 1, 0.1f, 1f); };
        InvokerOfDmr.InvokeWithDelay(this, shakeCamera, 1);
        InvokerOfDmr.InvokeWithDelay(SoundEffectManager.instance, SoundEffectManager.instance.PlayDiceLaunchSound, 0.2f);
    }
    [ClientRpc]
    private void playerMiddleChargerSFX()
    {
        Vector3 soundPosition = BoardRefrenceHolder.instance.middleChargerAnimator.transform.position;
        SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.EarthQuakeMiddleCharger, soundPosition, 1, 1, 1, 0.85f);
        CameraShaker.Instance.ShakeOnce(0.4f, 0.5f, 3f, 3f);
    }
}
