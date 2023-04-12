using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerStatsGUI : MonoBehaviour
{
    public static PlayerStatsGUI instance;
    public bool recalculateWithColor;
    public GameObject startAnimation;
    [SerializeField] private GameObject waitingForPlayersMenu;
    private bool startAnimPlayed;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    [SerializeField] private GameObject[] playerStatObject;
    public GameObject[] PlayerStatObject => playerStatObject;

    public void RecalculateGUI(GameObject[] players)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i + 1 > players.Length)
            {
                playerStatObject[i].SetActive(false);
                continue;
            }
            else
            {
                NetworkPlayerCON NetPC = players[i].GetComponent<NetworkPlayerCON>();
                playerStatObject[i].GetComponent<Image>().enabled = true;
                TextMeshProUGUI money_Text = playerStatObject[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI playername_Text = playerStatObject[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                if (recalculateWithColor) playername_Text.color = NetPC.playerColor;
                if (NetPC.PlayerMoney < 0)
                {
                    money_Text.text = "0";
                }
                else
                {
                    string moneyString = NetPC.PlayerMoney.ToString("000,000,000");
                    string moneyString2 = moneyString.TrimStart('0', '.');
                    money_Text.text = moneyString2;
                }
                playername_Text.text = NetPC.PlayerName;
                money_Text.enabled = true;
                playername_Text.enabled = true;
            }
        }
    }
    public void initStartAnim(uint ID)
    {
        if (startAnimPlayed) return;
        GameObject gameObj = UtulitiesOfDmr.ReturnCorrespondPlayerById(ID);
        NetworkPlayerCON NetPC = gameObj.GetComponent<NetworkPlayerCON>();
        if (AnimatedTextCreator.instance != null)
        {
            AnimatedTextCreator.instance.CreateAnimatedText(NetPC.PlayerName + " Will make first move!", Color.white);
        }
    }
    public void closeWaitingForPlayers()
    {
        waitingForPlayersMenu.SetActive(false);
    }
}
