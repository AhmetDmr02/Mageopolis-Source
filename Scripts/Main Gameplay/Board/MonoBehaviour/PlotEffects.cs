using System;
using UnityEngine;

public class PlotEffects : MonoBehaviour
{
    private void Start()
    {
        LandBuyManager.PlayerBoughtLand += (int landIndex, uint playerId) => recalculateVisuals(landIndex, 0, playerId);
        LandBuyManager.PlayerUpgradedLand += recalculateVisuals;
    }
    private void OnDestroy()
    {
        LandBuyManager.PlayerBoughtLand -= (int landIndex, uint playerId) => recalculateVisuals(landIndex, 0, playerId);
        LandBuyManager.PlayerUpgradedLand -= recalculateVisuals;
    }
    private void recalculateVisuals(int plotIndex, int upgradeIndex, uint playerID)
    {
        try
        {
            PlotClass _plotClass = UtulitiesOfDmr.ReturnPlotClassByIndex(plotIndex);
            for (int i = 0; i < _plotClass.monolithRuneParent.transform.childCount; i++)
            {
                _plotClass.monolithRuneParent.transform.GetChild(i).gameObject.SetActive(false);
            }
            //if (upgradeIndex == 0) return;
            if (upgradeIndex - 1 >= 0)
            {
                _plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).gameObject.SetActive(true);
                if (_plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).GetComponent<Animation>() != null)
                    _plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).GetComponent<Animation>().Play();
                if (_plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).GetComponent<MeshRenderer>() != null)
                {
                    //Assigning Monoliths to Correct Player Color
                    Color32 playerColor = UtulitiesOfDmr.ReturnPlayerColor(playerID);
                    setMonolithColor(playerColor, _plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).gameObject);
                }
                else
                {
                    //Our second monoliths contains 2 different objects
                    Color32 playerColor = UtulitiesOfDmr.ReturnPlayerColor(playerID);
                    if (_plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).GetChild(0).GetComponent<MeshRenderer>() != null)
                        setMonolithColor(playerColor, _plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).GetChild(0).gameObject);
                    if (_plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).GetChild(1).GetComponent<MeshRenderer>() != null)
                        setMonolithColor(playerColor, _plotClass.monolithRuneParent.transform.GetChild(upgradeIndex - 1).GetChild(1).gameObject);
                }
            }
            string priceString = formatNumber(UtulitiesOfDmr.GetCurrentPlotIncome(_plotClass));
            _plotClass.plotText.text = playerID == 0 ? _plotClass.defaultString : $"{priceString}";
        }
        catch (System.Exception err)
        {
            Debug.LogWarning("Plot visualiser error " + err);
        }
    }
    private string formatNumber(int number)
    {
        if (number >= 1000000)
        {
            return (number / 1000000f).ToString("0.#") + "m";
        }
        else if (number >= 1000)
        {
            return (number / 1000f).ToString("0.#") + "k";
        }
        else
        {
            return number.ToString();
        }
    }
    private void setMonolithColor(Color32 color, GameObject go)
    {
        Material wantedMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        //Setting up colors
        wantedMat.SetColor("_BaseColor", color);
        wantedMat.SetFloat("_Metallic", 1);
        wantedMat.SetFloat("_Smoothness", 0);
        wantedMat.EnableKeyword("_EMISSION");
        //-1.5f intensify is more accurate
        wantedMat.SetColor("_EmissionColor", (Color)color * Mathf.Pow(2, -1.5f));
        go.GetComponent<MeshRenderer>().material = wantedMat;
    }
}
