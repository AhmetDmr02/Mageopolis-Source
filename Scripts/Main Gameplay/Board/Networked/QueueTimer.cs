using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class QueueTimer : NetworkBehaviour
{
    private GameObject _timerObject;
    private Image _timerBar;
    private float decreaseRate = 1;
    [SerializeField] private float maxTimePerPlayer;
    [SerializeField] private Color32 barColor;
    private bool alreadySent;

    [SyncVar]
    public float SyncedTime;


    public static QueueTimer instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        BoardMoveManager.onMoveDone += setTimerForPlayer;
        QueueManager.queueEndedByPlayer += queueEndedByPlayer;
        QueueManager.currentQueueDisconnected += currentQueuePlayerDisconnected;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= setTimerForPlayer;
        QueueManager.queueEndedByPlayer -= queueEndedByPlayer;
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
        //Lerping variable for incase of server is running like 5 fps it will be still smooth
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
            if (NetworkSellingManager.instance.sellModeOpen)
            {
                NetworkSellingManager.instance.QueueTimePassedBeforeRequest();
                return;
            }
            if (TeleportManager.instance.TeleporterOpen)
            {
                TeleportManager.instance.QueueTimePassed();
            }
            BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(QueueManager.instance.CurrentQueue);
            if (boardPlayer != null)
            {
                QueueManager.instance.GetServerNextQueue(boardPlayer.previousDiceOne, boardPlayer.previousDiceTwo);
            }
            else
            {
                QueueManager.instance.GetServerNextQueue(1, 2);
                Debug.LogWarning("Board Player Result Turned Out Null");
            }
            closeTimer();
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
    public void setTimerForPlayer(PlotClass pc, BoardPlayer bp)
    {
        if (!isServer) return;
        SyncedTime = maxTimePerPlayer;
        decreaseRate = 1;
        alreadySent = false;
        openTimer();
    }
    public void queueEndedByPlayer()
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
