using Mirror;
using System;
using System.Collections;
using System.Drawing;
using UnityEngine;

public class QueueManager : NetworkBehaviour
{
    [SyncVar]
    public uint CurrentQueue;

    [SyncVar]
    public SyncList<uint> queueList = new SyncList<uint>();

    [SerializeField] private int currentIndex;

    //I have no idea why i named them small case
    public static event Action<uint> queueChanged;
    public static event Action queueEndedByPlayer;
    public static event Action currentQueueDisconnected;


    public static QueueManager instance;
    private bool lastQueueDisconnected = false;
    public bool cannotEndTour;
    //If Player Roll Dice 3 Times Double Teleport Them To Quicksand
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    private void Start()
    {
        MainPlayerRefrences.playerListChanged += RefreshList;
    }
    private void OnDestroy()
    {
        MainPlayerRefrences.playerListChanged -= RefreshList;
    }
    private void Update()
    {
        if (!isServer) return;
        if (queueList.Count <= 0) RefreshList();
    }
    public void RefreshList()
    {
        if (!isServer) return;
        queueList.Clear();
        foreach (uint playerIds in MainPlayerRefrences.instance.mainPlayerIds)
        {
            if (UtulitiesOfDmr.ReturnCorrespondPlayerById(playerIds) != null)
            {
                if (UtulitiesOfDmr.ReturnCorrespondPlayerById(playerIds).GetComponent<NetworkPlayerCON>() != null)
                {
                    if (UtulitiesOfDmr.ReturnCorrespondPlayerById(playerIds).GetComponent<NetworkPlayerCON>().Eliminated) continue;
                    if (UtulitiesOfDmr.ReturnCorrespondPlayerById(playerIds).GetComponent<NetworkPlayerCON>().Disconnected) continue;
                }
            }
            queueList.Add(playerIds);
        }
    }
    public void SetLastQueueDisconnected()
    {
        lastQueueDisconnected = true;
    }
    public void ReCheckQueue(NetworkConnectionToClient conn)
    {
        //When Player Disconnects Server Should Get Next Queue
        if (!isServer) return;

        //Solution for disconnected players
        if (Application.loadedLevel != 1) return;
        NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(conn.identity.netId).GetComponent<NetworkPlayerCON>();
        Func<bool> someFunc = () => netPc.Disconnected == true; //Checks if player is disconnected or not if it is then returns false
        WaitUntilOfDmr.InvokeWithDelay(this, this.RefreshList, someFunc);

        if (conn.identity.netId == CurrentQueue)
        {
            currentQueueDisconnected?.Invoke();
            lastQueueDisconnected = true;
            Debug.Log("Player Disconnected Manager, Will Skip To Next Queue");
        }
        else
        {
            Debug.Log("Player Disconnected But Current Queue ID Was Not The Same With Player ID");
        }
    }

    public IEnumerator InitFirstQueue(float delayTime)
    {
        if (!isServer) yield break;
        currentIndex = UnityEngine.Random.Range(0, queueList.Count);
        PlayerStatsGUI.instance.initStartAnim(queueList[currentIndex]);
        yield return new WaitForSecondsRealtime(delayTime);
        if (currentIndex > queueList.Count)
        {
            //This means someone just quit on delay
            CurrentQueue = queueList[0];
            RpcFireQueueChanged(CurrentQueue);
            DiceManager.instance.currentQueueAlreadyUsedDice = false;
            DiceManager.instance.spamPreventer = false;
        }
        else
        {
            try
            {
                CurrentQueue = queueList[currentIndex];
                RpcFireQueueChanged(CurrentQueue);
                DiceManager.instance.currentQueueAlreadyUsedDice = false;
                DiceManager.instance.spamPreventer = false;
            }
            catch
            {
                CurrentQueue = queueList[0];
                RpcFireQueueChanged(CurrentQueue);
                DiceManager.instance.currentQueueAlreadyUsedDice = false;
                DiceManager.instance.spamPreventer = false;
            }
        }
    }
    ///<summary>
    ///This function is obsolete since i want bit delay before starting queue use InitFirstQueue Instead
    ///</summary>
    [Command(requiresAuthority = true)]
    public void CmdInitFirstQueue()
    {
        if (!isServer) return;
        currentIndex = UnityEngine.Random.Range(0, queueList.Count);
        PlayerStatsGUI.instance.initStartAnim(queueList[currentIndex]);
        CurrentQueue = queueList[currentIndex];
        RpcFireQueueChanged(CurrentQueue);
        DiceManager.instance.currentQueueAlreadyUsedDice = false;
        DiceManager.instance.spamPreventer = false;
    }
    [Command(requiresAuthority = false)]
    public void requestEndTour(NetworkConnectionToClient sender = null)
    {
        if (!isServer) return;
        if (cannotEndTour) return;
        if (NetworkSellingManager.instance.sellModeOpen) return;
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
        if (!DiceManager.instance.currentQueueAlreadyUsedDice)
        {
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "Use your dice first!");
            return;
        }
        if (sender.identity.netId == CurrentQueue)
        {
            queueEndedByPlayer?.Invoke();
            GetServerNextQueue(boardPlayer.previousDiceOne, boardPlayer.previousDiceTwo);
        }
        else
        {
            MainGameManager.instance.RpcSendTargetError(sender, "Error", "You cannot end someone's queue");
            Debug.Log("You cannot end someone's queue");
        }
    }

    #region Opening And Closing End Round Button
    //This is player ui part 
    [Command(requiresAuthority = false)]
    public void shouldClientOpenEndTour(NetworkConnectionToClient sender = null)
    {
        //This function is crowding up a bit but this is the most reliable method for clients
        if (!isServer) return;
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(sender.identity.netId);
        if (sender.identity.netId == CurrentQueue && DiceManager.instance.currentQueueAlreadyUsedDice && boardPlayer.playerFinishedTravel)
        {
            RpcOpenEndTourPanel(sender, true);
        }
        else
        {
            RpcOpenEndTourPanel(sender, false);
        }
    }
    [TargetRpc]
    public void RpcOpenEndTourPanel(NetworkConnection conn, bool shouldI)
    {
        BoardRefrenceHolder.instance.endTourButton.ToggleButton(shouldI);
    }
    #endregion

    public bool GameDone;
    public void GetServerNextQueue(int firstDice, int secondDice)
    {
        if (!isServer) return;
        Debug.Log("Trying to get next queue...");
        if (queueList.Count == 1)
        {
            //Game ended
            //Numbers Doesn' Matter
            GameEnder.instance.checkForEnding(0, 0);
            return;
        }
        if (!lastQueueDisconnected)
        {
            if (firstDice == secondDice && !Quicksand.instance.TrappedPlayers.Contains(CurrentQueue))
            {
                RpcFireQueueChanged(CurrentQueue);
            }
            else
            {
                if (currentIndex + 1 >= queueList.Count) currentIndex = 0;
                else currentIndex += 1;
                CurrentQueue = queueList[currentIndex];
                RpcFireQueueChanged(CurrentQueue);
            }
        }
        else
        {
            lastQueueDisconnected = false;
            if (currentIndex > queueList.Count - 1)
            {
                if (currentIndex - 1 > queueList.Count)
                {
                    CurrentQueue = queueList[currentIndex - 1];
                    RpcFireQueueChanged(queueList[currentIndex - 1]);
                }
                else
                {
                    CurrentQueue = queueList[0];
                    RpcFireQueueChanged(queueList[0]);
                    Debug.Log("Bounds to 0");
                }
            }
            else
            {
                CurrentQueue = queueList[currentIndex];
                RpcFireQueueChanged(queueList[currentIndex]);
            }
        }
        DiceManager.instance.currentQueueAlreadyUsedDice = false;
        DiceManager.instance.spamPreventer = false;
    }
    [ClientRpc]
    public void RpcFireQueueChanged(uint changedId)
    {
        queueChanged?.Invoke(changedId);
    }
}
