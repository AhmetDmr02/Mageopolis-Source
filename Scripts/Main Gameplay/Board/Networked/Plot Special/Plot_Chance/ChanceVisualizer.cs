using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChanceVisualizer : MonoBehaviour
{
    [SerializeField] private GameObject chanceCard;

    public void VisualizeCard(int cardId)
    {
        if (cardId > ChanceManager.instance.ChanceInstances.Length) return;
        ChanceInstance selectedInstance = null;
        foreach (ChanceInstance chanceInstance in ChanceManager.instance.ChanceInstances)
        {
            if (chanceInstance.ChanceEffectId == cardId)
            {
                selectedInstance = chanceInstance;
                break;
            }
        }
        if (selectedInstance == null) return;
        string cardTitle = selectedInstance.ChanceTitle;
        string cardDescription = selectedInstance.ChanceDescription;
        Sprite cardThumbnail = selectedInstance.ChanceThumbnail;
        chanceCard.transform.GetChild(1).GetComponent<Image>().sprite = cardThumbnail;
        chanceCard.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = cardTitle;
        chanceCard.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = cardDescription;
        InvokerOfDmr.InvokeWithDelay(this, openCard, 1);
        InvokerOfDmr.InvokeWithDelay(this, closeCard, 6);
    }
    private void openCard()
    {
        chanceCard.SetActive(true);
    }
    private void closeCard()
    {
        chanceCard.SetActive(false);
    }
}
