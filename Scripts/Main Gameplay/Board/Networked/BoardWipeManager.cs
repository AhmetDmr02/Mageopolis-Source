using UnityEngine;
using Mirror;
using EZCameraShake;
using System;

public class BoardWipeManager : NetworkBehaviour
{
    public static BoardWipeManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    public void ServerWipeLand(int plotIndex)
    {
        if (!isServer) return;
        if (plotIndex > 39 || plotIndex < 0) return;
        PlotClass plotClass_ = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        Vector3 vec3 = plotClass_.slidePos;
        InvokerOfDmr.InvokeWithDelay(plotClass_, plotClass_.WipeLand, 1.7f);
        initWipeEffects(vec3);
        clientLandWipedRpc(plotIndex, vec3);
    }

    #region CMDS
    [Command(requiresAuthority = false)]
    public void RequestWipePlot(int plotIndex, NetworkConnectionToClient conn = null)
    {
        bool isEligible = checkEligibility(plotIndex, conn);
        if (!isEligible)
        {
            MainGameManager.instance.RpcSendTargetError(conn, "Error", "You are not eligible for this action.");
            return;
        }
        NetworkPlayerCON netPC = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>();
        int requiredMoney = UtulitiesOfDmr.ReturnWipeCostOfPlot(UtulitiesOfDmr.ReturnPlotClassByIndex(plotIndex));
        netPC.PlayerMoney -= requiredMoney;
        PlotClass plotClass_ = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        InvokerOfDmr.InvokeWithDelay(plotClass_, plotClass_.WipeLand, 1.5f);
        Vector3 vec3 = plotClass_.slidePos;
        //Sound effects
        initWipeEffects(vec3);
        clientLandWipedRpc(plotIndex, vec3);
    }
    #endregion
    #region Client RPCS
    [ClientRpc]
    private void clientLandWipedRpc(int plotIndex, Vector3 spawnPos)
    {
        if (isServer) return;
        PlotClass plotClass_ = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
        InvokerOfDmr.InvokeWithDelay(plotClass_, plotClass_.WipeLand, 1.5f);
        initWipeEffects(spawnPos);
    }
    #endregion
    private bool checkEligibility(int plotIndex, NetworkConnectionToClient con)
    {
        if (!isServer) return false;

        if (con == null) return false;

        if (!DiceManager.instance.currentQueueAlreadyUsedDice) return false;

        if (plotIndex > 39 || plotIndex < 0) return false;

        if (BoardMoveManager.instance.LandGameobject[plotIndex] == null) return false;

        PlotClass plotClass_ = BoardMoveManager.instance.LandGameobject[plotIndex].gameObject.GetComponent<PlotClass>();

        if (plotClass_.landKind != LandKind.Buyable) return false;

        if (plotClass_.ownedBy == 0) return false;

        if (plotClass_.ownedBy == con.identity.netId) return false;

        if (plotClass_.landCurrentUpgrade == 4) return false;

        int requiredMoney = UtulitiesOfDmr.ReturnWipeCostOfPlot(UtulitiesOfDmr.ReturnPlotClassByIndex(plotIndex));

        GameObject netPCObject = UtulitiesOfDmr.ReturnCorrespondPlayerById(con.identity.netId);
        if (netPCObject == null) return false;

        if (netPCObject.GetComponent<NetworkPlayerCON>() == null) return false;

        NetworkPlayerCON netPC = netPCObject.GetComponent<NetworkPlayerCON>();
        if (netPC.PlayerMoney < requiredMoney) return false;

        if (QueueManager.instance.CurrentQueue != netPC.netId) return false;

        //Players have ability to wipe if they have plots on Library location
        if (!UtulitiesOfDmr.ReturnTrueIfCanPlayerWipeLands(netPC)) return false;

        BoardPlayer boardPlayerOfConnection = UtulitiesOfDmr.ReturnBoardPlayerById(con.identity.netId);
        if (boardPlayerOfConnection == null) return false;

        if (boardPlayerOfConnection.currentLandIndex != plotIndex) return false;
        return true;
    }
    private void shakeCam(float f, float f2, float f3, float f4)
    {
        CameraShaker.Instance.ShakeOnce(f, f2, f3, f4);
    }
    private void initWipeEffects(Vector3 spawnPos)
    {
        SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.FireballSound, spawnPos, 1, 1, 1, 0.5f);
        InvokerOfDmr.InvokeWithDelay(EffectManager.instance, EffectManager.instance.createWipeEffect, 0.6f, spawnPos);
        InvokerOfDmr.InvokeWithDelay(SoundEffectManager.instance, SoundEffectManager.instance.CreateDummyAudioAt, 1.5f, SoundEffectManager.instance.FireballImpact, spawnPos, 1f, 1f, 0.5f, 1f);
        InvokerOfDmr.InvokeWithDelay(SoundEffectManager.instance, SoundEffectManager.instance.CreateDummyAudioAt, 1.5f, SoundEffectManager.instance.FireballExplosion, spawnPos, 1f, 1f, 1f, 0.7f);
        InvokerOfDmr.InvokeWithDelay(this, shakeCam, 1.5f, 2f, 1f, 0.1f, 3f);
    }
}
