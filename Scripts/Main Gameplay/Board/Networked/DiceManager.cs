using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class DiceManager : NetworkBehaviour
{
    //public Dice diceObjectOne,diceObjectTwo;

    public static DiceManager instance;

    [SyncVar]
    public int FirstDice, SecondDice;

    public static event Action<int, int, BoardPlayer> diceFired;
    public static event Action<int, int, BoardPlayer> diceLanded;

    [SyncVar]
    public bool currentQueueAlreadyUsedDice;
    public bool cannotRollDice;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    [SyncVar] public bool spamPreventer;

    [Command(requiresAuthority = false)]
    public void RollDice(NetworkConnectionToClient sender = null)
    {
        if (cannotRollDice)
        {
            //Player Bugged Or Cheating
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "You cannot do this action now");
            return;
        }
        if (sender.identity.netId != QueueManager.instance.CurrentQueue)
        {
            //Player Bugged Or Cheating
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "Your queue is not ready yet!?");
            return;
        }
        if (currentQueueAlreadyUsedDice)
        {
            //Player Bugged Or Cheating
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "You already used your queue");
            return;
        }
        if (TeleportManager.instance.TeleporterOpen)
        {
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "You cannot roll while teleporter is active");
            return;
        }
        if (spamPreventer)
        {
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "You already rolled your dice");
            return;
        }
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(sender.identity.netId);
        if (boardPlayer == null)
        {
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "Player not found.");
            return;
        }
        if (!boardPlayer.playerFinishedTravel)
        {
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "Wait until player arrival.");
            return;
        }
        spamPreventer = true;
        //Make Logic And Skip Next Queue
        //NOTE: I Tried To Make This Clump High Level But Mirror Didn't liked that idea for some reason?
        int diceOne = getRandomDiceVariable();
        int diceTwo = getRandomDiceVariable();
        boardPlayer.previousDiceOne = diceOne;
        boardPlayer.previousDiceTwo = diceTwo;
        //boardPlayer.currentLandIndex = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentLandIndex, diceOne + diceTwo);
        RpcDiceFired(diceOne, diceTwo, boardPlayer, boardPlayer.currentAnimationPlotIndex);
        StartCoroutine(waitForSecondSignal(diceOne, diceTwo, boardPlayer));
        //Delete This And Call This Function After Player Finishes His Action
    }

    [Command(requiresAuthority = false)]
    public void RollDiceDebug(int diceNum, int diceNum2, NetworkConnectionToClient sender = null)
    {
        if (cannotRollDice)
        {
            //Player Bugged Or Cheating
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "You cannot do this action now");
            return;
        }
        if (sender.identity.netId != QueueManager.instance.CurrentQueue)
        {
            //Player Bugged Or Cheating
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "Your queue is not ready yet!?");
            return;
        }
        if (currentQueueAlreadyUsedDice)
        {
            //Player Bugged Or Cheating
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "You already used your queue");
            return;
        }
        spamPreventer = true;
        //Make Logic And Skip Next Queue
        int dice1 = diceNum > 6 ? 6 : diceNum;
        int dice2 = diceNum2 > 6 ? 6 : diceNum2;
        int diceOne = dice1;
        int diceTwo = dice2;
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(sender.identity.netId);
        boardPlayer.previousDiceOne = diceOne;
        boardPlayer.previousDiceTwo = diceTwo;
        RpcDiceFired(diceOne, diceTwo, boardPlayer, boardPlayer.currentAnimationPlotIndex);
        //MOVED TO BOARD MOVE MANAGER
        //boardPlayer.currentLandIndex = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentLandIndex, diceOne + diceTwo);
        StartCoroutine(waitForSecondSignal(diceOne, diceTwo, boardPlayer));
        //Delete This And Call This Function After Player Finishes His Action
    }
    public IEnumerator waitForSecondSignal(int diceOne, int diceTwo, BoardPlayer boardPlayer)
    {
        if (!isServer) yield break;
        yield return new WaitForSecondsRealtime(3f);
        //Server should calculate faster than any client
        diceLanded?.Invoke(diceOne, diceTwo, boardPlayer);
        RpcDiceLanded(diceOne, diceTwo, boardPlayer);
        yield break;
    }
    public int getRandomDiceVariable()
    {
        //I used Random.Range from unity and it was so predictable so this is the new system
        byte[] bytes = new byte[1];
        System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        int value = (bytes[0] % 6) + 1;
        return value;
    }

    [ClientRpc]
    public void RpcDiceFired(int First, int Second, BoardPlayer boardPlayer, int currentAnimIndex)
    {
        FirstDice = First;
        SecondDice = Second;
        boardPlayer.currentAnimationPlotIndex = currentAnimIndex;
        diceFired?.Invoke(First, Second, boardPlayer);
        Debug.Log("Rpc Dice Fired");
    }
    [ClientRpc]
    public void RpcDiceLanded(int First, int Second, BoardPlayer boardPlayer)
    {
        if (isServer) return;
        diceLanded?.Invoke(First, Second, boardPlayer);
        Debug.Log("Rpc Dice Landed");
    }
}