using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoardInspector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI land_Name_Text;
    [SerializeField] private TextMeshProUGUI land_Description_Text;
    [SerializeField] private TextMeshProUGUI land_Biome_Description_Text;
    [SerializeField] private TextMeshProUGUI land_No_One_Owns_Text;
    [SerializeField] private TextMeshProUGUI land_Current_Owner_Text;
    [SerializeField] private TextMeshProUGUI land_Current_Monolith_Text;
    [SerializeField] private TextMeshProUGUI land_Total_Worth_Text;
    [SerializeField] private TextMeshProUGUI land_Wipe_Cost_Text;
    [SerializeField] private TextMeshProUGUI land_Base_Cost_Text;
    [SerializeField] private TextMeshProUGUI land_Monolith1_Cost_Text;
    [SerializeField] private TextMeshProUGUI land_Monolith2_Cost_Text;
    [SerializeField] private TextMeshProUGUI land_Monolith3_Cost_Text;
    [SerializeField] private TextMeshProUGUI land_Monolith4_Cost_Text;
    private TextMeshProUGUI land_Current_Monolith_Buy_Cost_Text;
    private TextMeshProUGUI land_Current_Monolith_Plot_Income_Text;
    [SerializeField] private GameObject[] monolithList;
    [SerializeField] private GameObject panelObject;
    private bool isPanelOpen;
    private void Start()
    {
        RaycastCenter.lookingObjectLeftClicked += onRaycastClick;
    }
    private void OnDestroy()
    {
        RaycastCenter.lookingObjectLeftClicked -= onRaycastClick;
    }
    private void onRaycastClick(GameObject go)
    {
        if (go.GetComponent<PlotClass>() == null) return;
        if (Highlighter.instance.IsInHighlightMode) return;
        if (!isPanelOpen)
        {
            isPanelOpen = true;
            panelObject.SetActive(true);
        }
        if (QueueManager.instance.CurrentQueue == NetworkPlayerCON.localPlayerCON.netId)
        {
            this.gameObject.GetComponent<LandBuyPanel>().openBuyPanelRightClick(go.GetComponent<PlotClass>(), UtulitiesOfDmr.ReturnBoardPlayerById(NetworkPlayerCON.localPlayerCON.netId));
        }
        PlotClass _plotClass = go.GetComponent<PlotClass>();
        if (_plotClass.landKind != LandKind.Buyable)
            setPlotNotBuyable();
        else
            setPlotBuyable();
        if (_plotClass.ownedBy == 0)
        {
            if (_plotClass.landKind == LandKind.Buyable)
                setPlotNoOwner(_plotClass);
        }
        else
            setPlotOwnerProperties(_plotClass);
        setPlotBasicProperties(_plotClass);
    }
    private void setPlotBasicProperties(PlotClass _plotClass)
    {
        land_Name_Text.text = _plotClass.landName;
        land_Description_Text.text = _plotClass.landDescription;
        land_Biome_Description_Text.text = _plotClass.landBiomeDesc;
        land_Name_Text.color = _plotClass.landColor;
        land_Description_Text.color = _plotClass.landColor;
        land_Description_Text.color = _plotClass.landColor;
    }
    private void setPlotOwnerProperties(PlotClass _plotClass)
    {
        int upgradeIndex = _plotClass.landCurrentUpgrade - 1;
        land_No_One_Owns_Text.enabled = false;
        land_Base_Cost_Text.enabled = false;
        land_Monolith1_Cost_Text.enabled = false;
        land_Monolith2_Cost_Text.enabled = false;
        land_Monolith3_Cost_Text.enabled = false;
        land_Monolith4_Cost_Text.enabled = false;
        if (upgradeIndex + 1 >= 0)
        {
            monolithList[upgradeIndex + 1].SetActive(true);
            land_Current_Monolith_Buy_Cost_Text = monolithList[upgradeIndex + 1].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            land_Current_Monolith_Plot_Income_Text = monolithList[upgradeIndex + 1].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            land_Current_Monolith_Buy_Cost_Text.text = $"Cost: {CalculatePlotMonolithCost(_plotClass)} Gold";
            land_Current_Monolith_Plot_Income_Text.text = $"Plot Income: {CalculatePlotIncome(_plotClass)} Gold";
        }
        string playerNameString = $"Owned By: {UtulitiesOfDmr.ReturnCorrespondPlayerById(_plotClass.ownedBy).GetComponent<NetworkPlayerCON>().PlayerName}";
        land_Current_Owner_Text.text = playerNameString;
        land_Total_Worth_Text.text = $"Plot Total Worth:\n {CalculateTotalWorth(_plotClass)} Gold";
        land_Wipe_Cost_Text.text = $"Wipe Cost:\n {CalculateTotalWorth(_plotClass) * BoardMoveManager.instance.LandInstance[_plotClass.landIndex].landPrices.landDestroyMultiplier} Gold";
    }

    private int CalculateTotalWorth(PlotClass _plotClass)
    {
        int totalWorth = 0;
        LandInstance landInstance = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        totalWorth += landInstance.landPrices.landBuyPrice;
        for (int i = 0; i <= _plotClass.landCurrentUpgrade - 1; i++)
        {
            totalWorth += landInstance.landPrices.landUpgradePrices[i];
        }
        return totalWorth;
    }
    private int CalculatePlotIncome(PlotClass _plotClass)
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
    private int CalculatePlotMonolithCost(PlotClass _plotClass)
    {
        int totalSalary = 0;
        LandInstance landInstance = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        if (_plotClass.landCurrentUpgrade - 1 == -1)
        {
            return landInstance.landPrices.landBuyPrice;
        }
        else
        {
            totalSalary += landInstance.landPrices.landUpgradePrices[_plotClass.landCurrentUpgrade - 1];
        }

        return totalSalary;
    }
    private void setPlotNotBuyable()
    {
        land_Current_Owner_Text.enabled = false;
        land_Total_Worth_Text.enabled = false;
        land_Wipe_Cost_Text.enabled = false;
        land_No_One_Owns_Text.enabled = false;
        land_Current_Monolith_Text.enabled = false;
        land_Base_Cost_Text.enabled = false;
        land_Monolith1_Cost_Text.enabled = false;
        land_Monolith2_Cost_Text.enabled = false;
        land_Monolith3_Cost_Text.enabled = false;
        land_Monolith4_Cost_Text.enabled = false;
        foreach (GameObject go in monolithList)
        {
            go.SetActive(false);
        }
    }
    private void setPlotBuyable()
    {
        land_Current_Owner_Text.enabled = true;
        land_Total_Worth_Text.enabled = true;
        land_Wipe_Cost_Text.enabled = true;
        land_No_One_Owns_Text.enabled = true;
        land_Current_Monolith_Text.enabled = true;
        foreach (GameObject go in monolithList)
        {
            go.SetActive(false);
        }
    }
    private void setPlotNoOwner(PlotClass _plotClass)
    {
        land_Current_Owner_Text.enabled = false;
        land_Total_Worth_Text.enabled = false;
        land_Wipe_Cost_Text.enabled = false;
        land_No_One_Owns_Text.enabled = true;
        land_Current_Monolith_Text.enabled = false;
        land_Base_Cost_Text.enabled = true;
        land_Monolith1_Cost_Text.enabled = true;
        land_Monolith2_Cost_Text.enabled = true;
        land_Monolith3_Cost_Text.enabled = true;
        land_Monolith4_Cost_Text.enabled = true;
        LandInstance instance = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        land_Base_Cost_Text.text = $"Land Base Buy Price: {instance.landPrices.landBuyPrice} Gold";
        land_Monolith1_Cost_Text.text = $"Single Monolith Price: {instance.landPrices.landUpgradePrices[0]} Gold";
        land_Monolith2_Cost_Text.text = $"Double Monolith Price: {instance.landPrices.landUpgradePrices[1]} Gold";
        land_Monolith3_Cost_Text.text = $"Rune Guided Monolith Price: {instance.landPrices.landUpgradePrices[2]} Gold";
        land_Monolith4_Cost_Text.text = $"The Great Monolith Price: {instance.landPrices.landUpgradePrices[3]} Gold";
        foreach (GameObject go in monolithList)
        {
            go.SetActive(false);
        }
    }
    public void closeInspectionPanel()
    {
        isPanelOpen = false;
        panelObject.SetActive(false);
    }
}
