using UnityEngine;

public class Plot_Praying : MonoBehaviour
{
    //Highlighter Stuff goes here

    public static bool IsPrayingInterfaceOn;
    [SerializeField] private GameObject highlightObjects;

    private void Start()
    {
        if (PrayingManager.instance.plotPraying == null)
            PrayingManager.instance.plotPraying = this;
        highlightObjects.SetActive(false);
    }
    private void OnDestroy()
    {
        Highlighter.HighlighterClickCallback -= requestPrayLocation;
    }
    public void QueueInitPrayingSequence(int[] highlightIndexes)
    {
        //Wait till local board player to reach its location,
        //With this player will not notice any lagging even if we have 
        if (IsPrayingInterfaceOn) return;
        WaitUntilOfDmr.InvokeWithDelay(this, initPrayingSequence, () => UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentAnimationPlotIndex == 20, highlightIndexes);
    }
    private void initPrayingSequence(int[] highlightIndexes)
    {
        if (Highlighter.instance.IsInHighlightMode) return;
        PlotClass[] classes = (UtulitiesOfDmr.ReturnPlotClassesWithIntArray(highlightIndexes).ToArray());
        Highlighter.instance.SwitchToHighlightMode(classes, false);
        Highlighter.HighlighterClickCallback += requestPrayLocation;
        IsPrayingInterfaceOn = true;
        highlightObjects.SetActive(true);
    }
    public void ClosePrayingHighlight()
    {
        if (Highlighter.instance.IsInHighlightMode && IsPrayingInterfaceOn) Highlighter.instance.CloseHighlightMode();
        IsPrayingInterfaceOn = false;
        Highlighter.HighlighterClickCallback -= requestPrayLocation;
        highlightObjects.SetActive(false);
    }
    private void requestPrayLocation(PlotClass[] plotClasses)
    {
        if (plotClasses.Length >= 1)
        {
            PrayingManager.instance.RequestProtectionToLand(plotClasses[0].landIndex);
        }
    }
    public void RequestSkipPraying()
    {
        if (!Highlighter.instance.IsInHighlightMode) return;
        PrayingManager.instance.RequestSkipPraying();
    }
}
