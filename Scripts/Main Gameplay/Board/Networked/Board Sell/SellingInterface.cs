using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class SellingInterface : MonoBehaviour
{
    public static SellingInterface instance;
    [SerializeField] private Image PlotInspectPanel;
    [SerializeField] private TextMeshProUGUI SellingTextInfo, PlotInspectPanelText;
    private int requiredPayment;
    public bool sellingIntefaceOn;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        PlotInspectPanel.gameObject.SetActive(false);
        SellingTextInfo.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        Highlighter.HighlighterHoverCallback -= listenForHighlightHovers;
        Highlighter.HighlighterClickCallback -= attemptToSellSelectedPlots;
        Highlighter.HighlighterMultipleSelectArrayChanged -= refreshTotalSelectedText;
    }
    public void InitSellingMode(int requiredPayment_, int[] plotIndexes)
    {
        toggleInspectionPanel(true);
        List<PlotClass> plotClasses = new List<PlotClass>();
        fillPlotClassWithIntArray(plotIndexes, plotClasses);
        Highlighter.instance.SwitchToHighlightMode(plotClasses.ToArray(), true);
        subscribeHighlighterCallbacks();
        requiredPayment = requiredPayment_;
        refreshTotalSelectedText();
        sellingIntefaceOn = true;
    }
    public void closeSellingMode()
    {
        if (Highlighter.instance.IsInHighlightMode && sellingIntefaceOn) Highlighter.instance.CloseHighlightMode();
        sellingIntefaceOn = false;
        toggleInspectionPanel(false);
        unSubscribeHighlighterCallbacks();
    }
    private void listenForHighlightHovers(PlotClass _plotClass)
    {
        if (_plotClass == null)
        {
            PlotInspectPanel.gameObject.SetActive(false);
            return;
        }
        if (!sellingIntefaceOn)
        {
            PlotInspectPanel.gameObject.SetActive(false);
            return;
        }
        PlotInspectPanel.gameObject.SetActive(true);
        Vector3 vec3 = _plotClass.slidePos;
        vec3.y += 30;
        PlotInspectPanel.transform.position = vec3;
        string infoString =
            $"Sell Price:\n {UtulitiesOfDmr.ReturnTotalWorthOfPlotClasses(new PlotClass[] { _plotClass })}";
        PlotInspectPanelText.text = infoString;
    }
    private void attemptToSellSelectedPlots(PlotClass[] _plotClass)
    {
        int getTotalWorth = UtulitiesOfDmr.ReturnTotalWorthOfPlotClasses(_plotClass.ToArray());
        if (getTotalWorth < requiredPayment)
        {
            NotificationCreator.instance.createNotification("Error", "You need to select more lands in order to pay.");
            return;
        }
        List<int> plotClassIndexes = new List<int>();
        foreach (PlotClass pc in _plotClass)
        {
            plotClassIndexes.Add(pc.landIndex);
        }
        NetworkSellingManager.instance.RequestSellPlots(plotClassIndexes.ToArray());
    }
    private void refreshTotalSelectedText()
    {
        int totalSelected = UtulitiesOfDmr.ReturnTotalWorthOfPlotClasses(Highlighter.instance.SelectedHighlihtObjects.Keys.ToArray());
        if (requiredPayment > totalSelected)
        {
            SellingTextInfo.color = Color.red;
        }
        else
        {
            SellingTextInfo.color = Color.green;
        }
        SellingTextInfo.text = $"Needed\n {requiredPayment} \n Total Selected \n {totalSelected}";
    }
    private void toggleInspectionPanel(bool toggle)
    {
        SellingTextInfo.gameObject.SetActive(toggle);
        PlotInspectPanel.gameObject.SetActive(false);
    }
    private void subscribeHighlighterCallbacks()
    {
        Highlighter.HighlighterHoverCallback += listenForHighlightHovers;
        Highlighter.HighlighterClickCallback += attemptToSellSelectedPlots;
        Highlighter.HighlighterMultipleSelectArrayChanged += refreshTotalSelectedText;
    }
    private void unSubscribeHighlighterCallbacks()
    {
        Highlighter.HighlighterHoverCallback -= listenForHighlightHovers;
        Highlighter.HighlighterClickCallback -= attemptToSellSelectedPlots;
        Highlighter.HighlighterMultipleSelectArrayChanged -= refreshTotalSelectedText;
    }
    private void fillPlotClassWithIntArray(int[] intArray, List<PlotClass> listToFill)
    {
        foreach (int i in intArray)
        {
            listToFill.Add(BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>());
        }
    }
    //public bcs i need to assign this to button
    public void RequestBankruptButton()
    {
        if (NetworkSellingManager.instance != null)
        {
            NetworkSellingManager.instance.RequestBankrupt();
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
