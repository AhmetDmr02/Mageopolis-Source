using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;
using System;
using System.Linq;

public class EndGameStats : MonoBehaviour
{
    public GameObject endPanel;
    [SerializeField] private GameObject playerStatInstance;
    [SerializeField] private GameObject playerStats;

    int ComparePlayerEndStatsByMoney(PlayerEndStats a, PlayerEndStats b)
    {
        return b.PlayerMoney.CompareTo(a.PlayerMoney);
    }

    public void InitEndState(PlayerEndStats[] statsOfAllPlayers)
    {
        endPanel.SetActive(true);
        PlayerEndStats[] sortedStatsOfAllPlayers = statsOfAllPlayers;
        Array.Sort(sortedStatsOfAllPlayers, ComparePlayerEndStatsByMoney);
        Array.Reverse(sortedStatsOfAllPlayers);
        for (int i = 0; i < sortedStatsOfAllPlayers.Length; i++)
        {
            foreach (GameObject Players in MainPlayerRefrences.instance.PlayerObjects)
            {
                NetworkPlayerCON netPc = Players.GetComponent<NetworkPlayerCON>();

                if (netPc.PlayerName == sortedStatsOfAllPlayers[i].PlayerName)
                {
                    GameObject go = Instantiate(playerStatInstance, playerStats.transform);
                    go.GetComponent<Image>().color = sortedStatsOfAllPlayers[i].PlayerColor;
                    go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = sortedStatsOfAllPlayers[i].PlayerName;
                    go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Total Worth: {sortedStatsOfAllPlayers[i].PlayerMoney}";
                }
            }
        }
    }
    public void purgeLobby()
    {
        if (NetworkClient.isConnected && NetworkClient.active)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
    }

    public struct PlayerEndStats
    {
        public string PlayerName;
        public int PlayerMoney;
        public Color PlayerColor;
    }
}
