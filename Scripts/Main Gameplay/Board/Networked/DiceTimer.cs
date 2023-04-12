using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class DiceTimer : NetworkBehaviour
{
    private GameObject _timerObject;
    private Image _timerBar;
    [SerializeField] private float maxTimePerPlayer;
    [SerializeField] private Color32 barColor;
    private bool alreadySent;
    private float decreaseRate = 1;

    [SyncVar]
    public float SyncedTime;

    public static DiceTimer instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        QueueManager.queueChanged += setTimerForPlayer;
        QueueManager.currentQueueDisconnected += currentQueuePlayerDisconnected;
        DiceManager.diceFired += DiceFiredByPlayer;
    }
    private void OnDestroy()
    {
        QueueManager.queueChanged -= setTimerForPlayer;
        DiceManager.diceFired -= DiceFiredByPlayer;
        QueueManager.currentQueueDisconnected -= currentQueuePlayerDisconnected;
    }

    void Update()
    {
        if (BoardRefrenceHolder.instance == null) return;
        if (_timerObject == null || _timerBar == null)
        {
            _timerObject = BoardRefrenceHolder.instance.TimerObject;
            _timerBar = BoardRefrenceHolder.instance.TimerBar;
            return;
        }
        adjustBar();
        if (!isServer) return;
        serverCheckAndTickForTime();
    }
    private float lerpedSyncTime;
    private void adjustBar()
    {
        if (lerpedSyncTime < SyncedTime) lerpedSyncTime = SyncedTime;
        if (SyncedTime > 0)
        {
            lerpedSyncTime = Mathf.Lerp(lerpedSyncTime, SyncedTime, 4 * Time.deltaTime);
            _timerBar.fillAmount = lerpedSyncTime / maxTimePerPlayer;
        }
    }
    private void serverCheckAndTickForTime()
    {
        if (!isServer) return;
        if (SyncedTime > 0)
        {
            SyncedTime -= decreaseRate * Time.deltaTime;
        }
        else if (SyncedTime < 0 && !alreadySent)
        {
            //Dice Numbers Doesn't Matter
            alreadySent = true;
            SyncedTime = 0;
            BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(QueueManager.instance.CurrentQueue);
            if (DiceManager.instance.spamPreventer)
            {
                NotificationCreator.instance.createNotification("Error", "Player already used dice");
                return;
            }
            if (TeleportManager.instance.TeleporterOpen)
            {
                TeleportManager.instance.QueueTimePassed();
            }
            DiceManager.instance.currentQueueAlreadyUsedDice = true;
            DiceManager.instance.spamPreventer = true;
            //Make Logic And Skip Next Queue
            int diceOne = Random.Range(1, 7);
            int diceTwo = Random.Range(1, 7);
            if (boardPlayer == null)
            {
                Debug.LogWarning("Board Player Result Turned Out Null");
                QueueManager.instance.GetServerNextQueue(1, 2);
            }
            else
            {
                boardPlayer.previousDiceOne = diceOne;
                boardPlayer.previousDiceTwo = diceTwo;
                DiceManager.instance.RpcDiceFired(diceOne, diceTwo, boardPlayer, boardPlayer.currentAnimationPlotIndex);
                DiceManager.instance.StartCoroutine(DiceManager.instance.waitForSecondSignal(diceOne, diceTwo, boardPlayer));
                //Delete This And Call This Function After Player Finishes His Action
                closeTimer();
            }
        }
    }
    private void currentQueuePlayerDisconnected()
    {
        //Skipping disconnected queue can cause problems so we will wait for queuemanager instead

        //if (!alreadySent && SyncedTime > 0)
        //{
        //    alreadySent = true;
        //    SyncedTime = 0;
        //    QueueManager.instance.GetServerNextQueue(1, 2);
        //}
    }
    public void setTimerForPlayer(uint queue)
    {
        if (!isServer) return;
        SyncedTime = maxTimePerPlayer;
        alreadySent = false;
        openTimer();
    }
    public void DiceFiredByPlayer(int i, int x, BoardPlayer bp)
    {
        if (!isServer) return;
        alreadySent = true;
        SyncedTime = 0;
        closeTimer();
    }
    public void setDecreaseRate(float decreaseRate_)
    {
        decreaseRate = decreaseRate_;
    }
    public void setTimeFull()
    {
        SyncedTime = maxTimePerPlayer;
    }
    [ClientRpc]
    public void closeTimer()
    {
        _timerObject.SetActive(false);
    }
    [ClientRpc]
    public void openTimer()
    {
        _timerObject.SetActive(true);
        _timerBar.color = barColor;
    }
}
