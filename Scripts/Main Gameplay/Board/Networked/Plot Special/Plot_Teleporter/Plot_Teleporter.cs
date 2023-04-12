using System.Collections.Generic;
using UnityEngine;

public class Plot_Teleporter : MonoBehaviour
{
    [Header("Gameobjects")]
    [SerializeField] private GameObject dontTeleportButton;
    public static bool teleporterInterfaceOn;
    private void Start()
    {
        dontTeleportButton.SetActive(false);
    }
    private void OnDestroy()
    {
        Highlighter.HighlighterClickCallback -= requestTeleportLand;
    }
    public void QueueInitTeleportHighlighter(int[] plotIndexes)
    {
        //This function is here to prevent desync
        //We will wait until client player to arrive
        WaitUntilOfDmr.InvokeWithDelay(this, InitTeleportHighlighter, () => UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentAnimationPlotIndex == 30, plotIndexes);
    }
    public void InitTeleportHighlighter(int[] plotIndexes)
    {
        dontTeleportButton.SetActive(true);
        List<PlotClass> plotClasses = new List<PlotClass>();
        fillPlotClassWithIntArray(plotIndexes, plotClasses);
        Highlighter.instance.SwitchToHighlightMode(plotClasses.ToArray(), false);
        subscribeCallback();
        teleporterInterfaceOn = true;
    }
    public void CloseTeleportHighlight()
    {
        if (Highlighter.instance.IsInHighlightMode && teleporterInterfaceOn) Highlighter.instance.CloseHighlightMode();
        teleporterInterfaceOn = false;
        dontTeleportButton.SetActive(false);
        Highlighter.HighlighterClickCallback -= requestTeleportLand;
    }
    private void requestTeleportLand(PlotClass[] plotIndex)
    {
        if (plotIndex.Length >= 1)
        {
            TeleportManager.instance.RequestTeleport(plotIndex[0].landIndex);
        }
    }
    private void subscribeCallback()
    {
        Highlighter.HighlighterClickCallback += requestTeleportLand;
    }
    private void fillPlotClassWithIntArray(int[] intArray, List<PlotClass> listToFill)
    {
        foreach (int i in intArray)
        {
            listToFill.Add(BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>());
        }
    }
}
