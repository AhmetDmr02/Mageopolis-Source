using UnityEngine;
using UnityEngine.EventSystems;

public class DiceButton : MonoBehaviour, IPointerClickHandler
{
    public static DiceButton instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        QueueManager.queueChanged += CheckButton;
        DiceManager.diceFired += closeButton;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (DiceManager.instance == null)
        {
            NotificationCreator.instance.createNotification("Critical Error Occured", "Dice Button Cannot Find Dice Manager Reference!!, This Is Critical Error And Means Your Client Is Basically Doesn't Working Probably Developers Made Some Bad Code UFF!");
            return;
        }
        EventSystem.current.SetSelectedGameObject(null);
        if (Highlighter.instance.IsInHighlightMode) return;
        DiceManager.instance.RollDice();
        this.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
    }
    private void OnDestroy()
    {
        QueueManager.queueChanged -= CheckButton;
        DiceManager.diceFired -= closeButton;
    }
    public void CheckButton(uint newQueue)
    {
        if (NetworkPlayerCON.localPlayerCON.netIdentity.netId == newQueue)
            this.gameObject.transform.parent.GetComponent<Canvas>().enabled = true;
        else
            this.gameObject.transform.parent.GetComponent<Canvas>().enabled = false;
    }
    //i used i1 and i2 because i had to use it in order to subscribe to action
    public void closeButton(int i, int i2, BoardPlayer bp)
    {
        this.gameObject.transform.parent.GetComponent<Canvas>().enabled = false;
    }
}
