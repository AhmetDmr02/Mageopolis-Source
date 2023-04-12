using Mirror;
using System;
using UnityEngine;

public class QueueIndicator : MonoBehaviour
{
    private void Start()
    {
        BoardMoveManager.CloseAllQueueIndicators += closeAllQueueIndicators;
        QueueManager.queueChanged += checkQueueIndicators;
    }
    private void closeAllQueueIndicators()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
    public void checkQueueIndicators(uint CurrentQueue)
    {
        try
        {
            if (this.gameObject.transform.parent.GetComponent<BoardPlayer>() == null) return;
            if (this.gameObject.transform.parent.GetComponent<BoardPlayer>().networkPlayerObject == null) return;

            if (this.gameObject.transform.parent.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkIdentity>().netId == CurrentQueue)
                this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            else
                this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log($"Queue Indicator Failed {e}");
        }
    }
    private void OnDestroy()
    {
        BoardMoveManager.CloseAllQueueIndicators -= closeAllQueueIndicators;
        QueueManager.queueChanged -= checkQueueIndicators;
    }
}
