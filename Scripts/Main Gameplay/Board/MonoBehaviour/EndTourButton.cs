using UnityEngine;
using UnityEngine.EventSystems;

public class EndTourButton : MonoBehaviour, IPointerClickHandler
{
    private void Start()
    {
        BoardMoveManager.onMoveDone += CheckButton;
        QueueManager.queueChanged += closeAll;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (QueueManager.instance == null)
        {
            NotificationCreator.instance.createNotification("Critical Error Occured", "End Tour Button Cannot Find Queue Manager Reference!!, This Is Critical Error And Means Your Client Is Basically Doesn't Working. Probably Developers Made Some Bad Code UFF!");
            return;
        }
        EventSystem.current.SetSelectedGameObject(null);
        if (Highlighter.instance.IsInHighlightMode) return;
        QueueManager.instance.requestEndTour();
        this.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= CheckButton;
        QueueManager.queueChanged -= closeAll;
    }
    private void closeAll(uint currentQueue)
    {
        this.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
    }
    public static void CheckButton(PlotClass pc, BoardPlayer bp)
    {
        if (bp != null)
        {
            if (bp.representedPlayerId != NetworkPlayerCON.localPlayerCON.netId) return;
        }
        QueueManager.instance.shouldClientOpenEndTour();
    }
    public void ToggleButton(bool toggleBool)
    {
        this.transform.parent.GetComponentInChildren<Canvas>().enabled = toggleBool;
    }
}
