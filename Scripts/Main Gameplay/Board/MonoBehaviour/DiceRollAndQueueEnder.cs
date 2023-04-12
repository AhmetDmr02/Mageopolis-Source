using UnityEngine;
using TMPro;
public class DiceRollAndQueueEnder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rollInfoText;
    private void Start()
    {
        RaycastCenter.lookingObjectChanged += listenForHovers;
        RaycastCenter.lookingObjectLeftClicked += listenClicks;
        rollInfoText.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        RaycastCenter.lookingObjectChanged -= listenForHovers;
        RaycastCenter.lookingObjectLeftClicked -= listenClicks;
    }
    private void FixedUpdate()
    {
        if (rollInfoText.gameObject.activeInHierarchy)
        {
            rollInfoText.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);
        }
    }
    private void listenForHovers(GameObject hoveredObject)
    {
        if (MainGameManager.instance.serverGameState != MainGameManager.ServerGameState.Ingame) { rollInfoText.gameObject.SetActive(false); return; }
        if (hoveredObject.gameObject.tag != "Dice") { rollInfoText.gameObject.SetActive(false); return; }
        rollInfoText.gameObject.SetActive(true);
        if (QueueManager.instance.CurrentQueue != NetworkPlayerCON.localPlayerCON.netId) { rollInfoText.gameObject.SetActive(false); return; }
        if (DiceManager.instance.currentQueueAlreadyUsedDice)
        {
            bool playerFinishedTravel = true;
            BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId);
            playerFinishedTravel = boardPlayer == null ? true : playerFinishedTravel = boardPlayer.playerFinishedTravel;
            if (DiceParticlesOpener.highlighted && playerFinishedTravel)
            {
                rollInfoText.text = "End Tour";
            }
            else
            {
                rollInfoText.text = "";
            }
        }
        else
        {
            if (DiceParticlesOpener.highlighted)
            {
                rollInfoText.text = "Roll Dice";
            }
            else
            {
                rollInfoText.text = "";
            }
        }
    }
    private void listenClicks(GameObject hoveredObject)
    {
        if (hoveredObject.gameObject.tag != "Dice") return;
        if (QueueManager.instance.CurrentQueue != NetworkPlayerCON.localPlayerCON.netId) return;
        if (DiceManager.instance.currentQueueAlreadyUsedDice)
        {
            bool playerFinishedTravel = true;
            BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId);
            playerFinishedTravel = boardPlayer == null ? true : playerFinishedTravel = boardPlayer.playerFinishedTravel;
            if (DiceParticlesOpener.highlighted && playerFinishedTravel)
            {
                QueueManager.instance.requestEndTour();
                rollInfoText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (DiceParticlesOpener.highlighted)
            {
                DiceManager.instance.RollDice();
                rollInfoText.gameObject.SetActive(false);
            }
        }
    }
}
