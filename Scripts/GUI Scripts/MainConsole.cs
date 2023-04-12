using EpicTransport;
using EZCameraShake;
using SmartConsole;
using System.Collections.Generic;
using UnityEngine;

public class MainConsole : CommandBehaviour
{
    [Command]
    public void Debug_Launch_Dice_Test(int diceOne, int diceTwo)
    {
        if (MainGameManager.instance == null) return;
        if (MainGameManager.instance.serverGameState != MainGameManager.ServerGameState.Ingame) return;
        diceOne = diceOne <= 0 ? 1 : diceOne;
        diceTwo = diceTwo <= 0 ? 1 : diceTwo;
        DiceManager.instance.RollDiceDebug(diceOne, diceTwo);
        Debug.Log("Dice Roll Command Called");
    }

    [Command]
    public void DEVELOPER_MoveMeTo(int landIndex)
    {
        BoardMoveManager.instance.MovePlayerTo(landIndex, UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId));
    }

    [Command]
    public void Debug_Launch_Dice()
    {
        if (MainGameManager.instance == null) return;
        if (MainGameManager.instance.serverGameState != MainGameManager.ServerGameState.Ingame) return;
        DiceManager.instance.RollDice();
        Debug.Log("Dice Roll Command Called");
    }
    [Command]
    public void Debug_End_Tour()
    {
        if (QueueManager.instance != null)
        {
            QueueManager.instance.requestEndTour();
        }
    }
    [Command]
    public void Debug_Toggle_Player_Names(bool toggle)
    {
        if (BoardMoveManager.instance == null) return;
        foreach (GameObject bp in BoardMoveManager.instance.boardPlayers)
        {
            BoardPlayer boardPlayer = bp.GetComponent<BoardPlayer>();
            boardPlayer.togglePlayerNames = toggle;
            //GameObject Doesn't Matter
            boardPlayer.checkTogglePlayerNames(this.gameObject);
        }
    }
    [Command]
    public void Debug_Set_Player_Name_Scale(float scale)
    {
        if (BoardMoveManager.instance == null) return;
        foreach (GameObject bp in BoardMoveManager.instance.boardPlayers)
        {
            bp.transform.GetChild(3).GetChild(0).transform.localScale = new Vector3(scale, scale, scale);
            Vector3 posBP = bp.transform.GetChild(3).GetChild(0).transform.localPosition;
            bp.transform.GetChild(3).GetChild(0).transform.localPosition = new Vector3(posBP.x, 0, posBP.z);
            bp.transform.GetChild(3).GetChild(0).transform.localPosition = new Vector3(posBP.x, 10f + scale, posBP.z);
        }
    }
    [Command]
    public void Land_Debug_Try_Buy_Current_Land()
    {
        LandBuyManager.instance.RequestBuyLand(UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentLandIndex);
    }
    [Command]
    public void Epic_Get_ID()
    {
        if (MainMenuManager.instance.isEpicActive)
        {
            Debug.Log(MainMenuManager.instance.GetCurrentLobbyId());
        }
    }
    [Command]
    public void Epic_Force_Leave()
    {
        if (MainMenuManager.instance.isEpicActive)
        {
            MainMenuManager.instance.LeaveLobby();
        }
    }
    [Command]
    public void Land_Debug_Try_Buy_Land(int landIndex)
    {
        LandBuyManager.instance.RequestBuyLand(landIndex);
    }
    [Command]
    public void Server_ReSyncEveryone()
    {
        BoardMoveManager.instance.ReSync();
    }

    [Command]
    public void Application_Quitgame()
    {
        Application.Quit();
    }
    [Command]
    public void Application_Toggle_Vsync(bool toggle)
    {
        QualitySettings.vSyncCount = toggle ? 1 : 0;
    }
    [Command]
    public void Application_Set_Target_FPS(int fps)
    {
        if (fps < 8) fps = 8;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }
    [Command]
    public void Debug_Check_End_Button()
    {
        EndTourButton.CheckButton(null, null);
    }
    [Command]
    public void Debug_Set_Volume(float volumeZeroToOne)
    {
        volumeZeroToOne = volumeZeroToOne < 0 ? 0 : volumeZeroToOne;
        volumeZeroToOne = volumeZeroToOne > 1 ? 1 : volumeZeroToOne;
        AudioListener.volume = volumeZeroToOne;
    }
    [Command]
    public void Info_Current_Rendering_Api()
    {
        UnityEngine.Rendering.GraphicsDeviceType currentAPI = UnityEngine.SystemInfo.graphicsDeviceType;
        Debug.Log($"Currently {currentAPI.ToString()} is active");
    }
    [Command]
    public void Camera_Set_Mode(bool isItOrthographic)
    {
        if (BoardRefrenceHolder.instance == null) return;
        BoardRefrenceHolder.instance.cameraController.switchPerspectiveMode(isItOrthographic);
    }
    [Command]
    public void Debug_Close_Highlighter()
    {
        if (Highlighter.instance != null)
            Highlighter.instance.CloseHighlightMode();
    }
    [Command]
    public void Info_Get_Player_Owned_Lands(int playerID)
    {
        List<PlotClass> ListOfPlots = UtulitiesOfDmr.ReturnOwnedByPlayerPlots((uint)playerID);
        foreach (PlotClass pc in ListOfPlots)
        {
            Debug.Log(pc.landName);
        }
    }

    [Command]
    public void Visual_Debug_Create_Meteor_At_Plot(int landIndex)
    {
        if (landIndex < 0) return;
        if (landIndex > 39) return;
        Vector3 spawnPos = BoardMoveManager.instance.LandGameobject[landIndex].GetComponent<PlotClass>().slidePos;
        SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.FireballSound, spawnPos, 1, 1, 1, 0.5f);
        InvokerOfDmr.InvokeWithDelay(EffectManager.instance, EffectManager.instance.createWipeEffect, 0.6f, spawnPos);
        InvokerOfDmr.InvokeWithDelay(SoundEffectManager.instance, SoundEffectManager.instance.CreateDummyAudioAt, 1.5f, SoundEffectManager.instance.FireballImpact, spawnPos, 1f, 1f, 0.5f, 1f);
        InvokerOfDmr.InvokeWithDelay(SoundEffectManager.instance, SoundEffectManager.instance.CreateDummyAudioAt, 1.5f, SoundEffectManager.instance.FireballExplosion, spawnPos, 1f, 1f, 1f, 0.7f);
        InvokerOfDmr.InvokeWithDelay(this, shakeCam, 1.5f, 2f, 1f, 0.1f, 3f);

    }
    [Command]
    public void Server_Recheck_Queue()
    {
        QueueManager.instance.RefreshList();
    }
    [Command]
    public void Client_IsConnectingEOS()
    {
        Debug.Log(EOSSDKComponent.IsConnecting);
    }
    [Command]
    public void Visual_Hide_Tree()
    {
        BoardRefrenceHolder.instance.treeAnimator.gameObject.SetActive(false);
    }
    private void shakeCam(float f, float f2, float f3, float f4)
    {
        CameraShaker.Instance.ShakeOnce(f, f2, f3, f4);
    }
}
