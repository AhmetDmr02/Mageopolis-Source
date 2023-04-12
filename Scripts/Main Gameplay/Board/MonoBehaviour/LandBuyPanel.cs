using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class LandBuyPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI land_Name_Text;
    [SerializeField] private TextMeshProUGUI land_Description_Text;
    [SerializeField] private TextMeshProUGUI land_Biome_Description_Text;
    [SerializeField] private TextMeshProUGUI land_Buy_Button_Price_Text;
    [SerializeField] private GameObject buyLandShade;
    [SerializeField] private GameObject[] buyMonoliths;

    [SerializeField] private GameObject buyPanel;
    private void Start()
    {
        BoardMoveManager.onMoveDone += openBuyPanel;
        LandBuyManager.PlayerBoughtLand += eventListener;
        LandBuyManager.PlayerUpgradedLand += upgradeEventListener;
        QueueManager.queueChanged += closePanelAfter;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= openBuyPanel;
        LandBuyManager.PlayerBoughtLand -= eventListener;
        LandBuyManager.PlayerUpgradedLand -= upgradeEventListener;
        QueueManager.queueChanged -= closePanelAfter;
    }
    public void openBuyPanel(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        if (plotClass.landKind != LandKind.Buyable) return;
        if (QueueManager.instance.CurrentQueue != NetworkPlayerCON.localPlayerCON.netId) return;
        if (boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>() == null) return;
        if (boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId != NetworkPlayerCON.localPlayerCON.netId) return;
        if (plotClass.ownedBy != NetworkPlayerCON.localPlayerCON.netId && plotClass.ownedBy != 0) return;
        OpenPanel();
        fillUpPanelText(plotClass);
        setupMonolithCards(plotClass);
        refreshBuyPanel(plotClass);
        this.gameObject.GetComponent<BoardInspector>().closeInspectionPanel();
    }

    #region Low Level Calculations
    public void openBuyPanelRightClick(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        if (plotClass.landKind != LandKind.Buyable) return;
        if (QueueManager.instance.CurrentQueue != NetworkPlayerCON.localPlayerCON.netId) return;
        if (boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>() == null) return;
        if (boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId != NetworkPlayerCON.localPlayerCON.netId) return;
        if (plotClass.ownedBy != NetworkPlayerCON.localPlayerCON.netId && plotClass.ownedBy != 0) return;
        if (boardPlayer.currentAnimationPlotIndex != plotClass.landIndex) return;
        OpenPanel();
        fillUpPanelText(plotClass);
        setupMonolithCards(plotClass);
        refreshBuyPanel(plotClass);
        this.gameObject.GetComponent<BoardInspector>().closeInspectionPanel();
    }
    private void fillUpPanelText(PlotClass plotClass)
    {
        land_Name_Text.text = plotClass.landName;
        land_Description_Text.text = plotClass.landDescription;
        land_Biome_Description_Text.text = plotClass.landBiomeDesc;
        land_Name_Text.color = plotClass.landColor;
        land_Description_Text.color = plotClass.landColor;
        land_Buy_Button_Price_Text.text = $"Buy This Land For:\n {BoardMoveManager.instance.LandInstance[plotClass.landIndex].landPrices.landBuyPrice} Gold";
    }
    public void setupMonolithCards(PlotClass _plotClass)
    {
        string currentPlotIncome = $"Current Income: {getCurrentPlotIncome(_plotClass)}";
        int landCurrentUpgradeState = _plotClass.landCurrentUpgrade - 1;
        for (int i = 0; i < buyMonoliths.Length; i++)
        {
            if (landCurrentUpgradeState >= i)
            {
                //Buy Button
                buyMonoliths[i].transform.GetChild(1).gameObject.SetActive(false);
                buyMonoliths[i].transform.GetChild(5).gameObject.SetActive(false);
            }
            else
            {
                buyMonoliths[i].transform.GetChild(1).gameObject.SetActive(true);
                buyMonoliths[i].transform.GetChild(5).gameObject.SetActive(true);
                buyMonoliths[i].transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = $"After Upgrade Income: {getAfterUpgradePlotIncome(_plotClass, i)}";
            }
            //Plot Income Text
            buyMonoliths[i].transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = currentPlotIncome;
            buyMonoliths[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"Buy Cost: {getMonolithCosts(_plotClass, i)} Gold";
        }
    }
    private int getCurrentPlotIncome(PlotClass _plotClass)
    {
        int totalSalary = 0;
        LandInstance landInstance = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        totalSalary += landInstance.landPrices.landBaseSalary;
        for (int i = 0; i <= _plotClass.landCurrentUpgrade - 1; i++)
        {
            totalSalary += landInstance.landPrices.landUpgradeSalaries[i];
        }
        if (_plotClass.landPrices.isLandHit) totalSalary = (int)(totalSalary * _plotClass.landPrices.landHitPriceMultiplier);
        return totalSalary;
    }
    private int getMonolithCosts(PlotClass _plotClass, int priceForMonolith)
    {
        int total = 0;
        int currentUpgrade = _plotClass.landCurrentUpgrade - 1;
        LandInstance _land = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        for (int i = 0; i <= priceForMonolith; i++)
        {
            if (currentUpgrade >= i) continue;
            total += _land.landPrices.landUpgradePrices[i];
        }
        return total;
    }
    private int getAfterUpgradePlotIncome(PlotClass _plotClass, int upgradeIndex)
    {
        int total = 0;
        LandInstance _land = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        total += _land.landPrices.landBaseSalary;
        for (int i = 0; i <= upgradeIndex; i++)
        {
            total += _land.landPrices.landUpgradeSalaries[i];
        }
        if (_plotClass.landPrices.isLandHit) total = (int)(total * _plotClass.landPrices.landHitPriceMultiplier);
        return total;
    }
    private void refreshBuyPanel(PlotClass _plotClass)
    {
        if (_plotClass.ownedBy == NetworkPlayerCON.localPlayerCON.netId)
            buyLandShade.SetActive(false);
        else
            buyLandShade.SetActive(true);
    }
    private void eventListener(int plotIndex, uint id)
    {
        try
        {
            PlotClass _plotClass = BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
            if (_plotClass != null)
                refreshBuyPanel(_plotClass);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return;
        }
    }
    private void upgradeEventListener(int index, int upgradeIndex, uint id)
    {
        try
        {
            if (id != NetworkPlayerCON.localPlayerCON.netId) return;
            if (index != UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentLandIndex) return;
            if (buyPanel.activeInHierarchy)
            {
                setupMonolithCards(BoardMoveManager.instance.LandGameobject[index].GetComponent<PlotClass>());
            }
        }
        catch (Exception err)
        {

            Debug.Log(err);
        }

    }
    private void closePanelAfter(uint id)
    {
        ClosePanel();
    }

    #endregion

    #region Buttons
    public void ClosePanel()
    {
        buyPanel.SetActive(false);
    }
    public void OpenPanel()
    {
        buyPanel.SetActive(true);
    }
    public void RequestBuyLand()
    {
        try
        {
            LandBuyManager.instance.RequestBuyLand(UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentLandIndex);
            SoundEffectManager.instance.PlayClickSound();
            EventSystem.current.SetSelectedGameObject(null);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }
    }
    public void RequestBuyMonolith(int monolithIndex)
    {
        //0 is single monolith 3 is max level monolith
        try
        {
            LandBuyManager.instance.RequestUpgradeLand(UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId).currentLandIndex, monolithIndex);
            SoundEffectManager.instance.PlayClickSound();
            //Make Visual Effect
            EventSystem.current.SetSelectedGameObject(null);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }
    }
    #endregion
}
