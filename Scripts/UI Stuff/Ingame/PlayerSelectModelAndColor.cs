using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
public class PlayerSelectModelAndColor : MonoBehaviour
{
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private playerSlotVariables[] playerSlots;
    public void recalculateAll()
    {
        GameObject[] players = MainPlayerRefrences.instance.PlayerObjects.ToArray();
        foreach (playerSlotVariables ps in playerSlots)
        {
            ps.playerObject = null;
        }
        for (int i = 0; i < players.Length; i++)
        {
            playerSlots[i].playerObject = players[i] == null ? null : players[i];
        }
        foreach (playerSlotVariables psv in playerSlots)
        {
            if (psv.playerObject != null)
            {
                if (psv.playerObject.GetComponent<NetworkPlayerCON>() != null)
                {
                    if (psv.playerObject.GetComponent<NetworkPlayerCON>().Disconnected)
                    {
                        for (int i = 0; i < psv.slotObject.transform.childCount; i++)
                        {
                            Transform childObject = psv.slotObject.transform.GetChild(i);
                            if (i == 4)
                            {
                                //Middle Name
                                childObject.gameObject.SetActive(true);
                                childObject.GetComponent<Text>().text = "Empty";
                                psv.slotObject.GetComponent<Image>().color = new Color32(168, 98, 98, 255);
                            }
                            else
                                childObject.gameObject.SetActive(false);
                        }
                        continue;
                    }
                }
            }
            if (psv.playerObject == null)
            {
                for (int i = 0; i < psv.slotObject.transform.childCount; i++)
                {
                    Transform childObject = psv.slotObject.transform.GetChild(i);
                    if (i == 4)
                    {
                        //Middle Name
                        childObject.gameObject.SetActive(true);
                        childObject.GetComponent<Text>().text = "Empty";
                        psv.slotObject.GetComponent<Image>().color = new Color32(168, 98, 98, 255);
                    }
                    else
                        childObject.gameObject.SetActive(false);

                }
            }
            else
            {
                for (int i = 0; i < psv.slotObject.transform.childCount; i++)
                {
                    Transform childObject = psv.slotObject.transform.GetChild(i);
                    if (psv.playerObject.GetComponent<NetworkPlayerCON>().isLocalPlayer)
                    {
                        if (i == 4)
                            childObject.gameObject.SetActive(false);
                        else
                        {
                            childObject.gameObject.SetActive(true);
                            switch (i)
                            {
                                case 0:
                                    childObject.GetComponent<Text>().text = psv.playerObject.GetComponent<NetworkPlayerCON>().PlayerName;
                                    break;
                                case 1:
                                    childObject.GetComponent<Text>().text = psv.playerObject.GetComponent<NetworkPlayerCON>().playerModel.ToString();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (i == 4 || i == 3 || i == 2)
                        {
                            childObject.gameObject.SetActive(false);
                        }
                        else
                        {
                            childObject.gameObject.SetActive(true);
                            switch (i)
                            {
                                case 0:
                                    childObject.GetComponent<Text>().text = psv.playerObject.GetComponent<NetworkPlayerCON>().PlayerName;
                                    break;
                                case 1:
                                    childObject.GetComponent<Text>().text = psv.playerObject.GetComponent<NetworkPlayerCON>().playerModel.ToString();
                                    break;
                            }
                        }
                    }
                }
                if (psv.playerObject.GetComponent<NetworkPlayerCON>().playerColor == Color.white)
                {
                    psv.slotObject.GetComponent<Image>().color = new Color32(168, 98, 98, 255);
                }
                else
                {
                    psv.slotObject.GetComponent<Image>().color = psv.playerObject.GetComponent<NetworkPlayerCON>().playerColor;
                }
            }
        }
        if (NetworkPlayerCON.localPlayerCON != null)
        {
            if (NetworkPlayerCON.localPlayerCON.isServer)
                startGameButton.SetActive(true);
            else
                startGameButton.SetActive(false);
        }
    }
    public void requestColor(Image sendObject)
    {
        if (NetworkPlayerCON.localPlayerCON == null) { Debug.Log("its null"); return; }
        MainGameManager.instance.CmdChangeUsedColors(NetworkPlayerCON.localPlayerCON.playerColor, sendObject.color, NetworkPlayerCON.localPlayerCON.netIdentity);
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void requestChangeModel(bool isItNext)
    {
        if (NetworkPlayerCON.localPlayerCON == null) return;
        NetworkPlayerCON.localPlayerCON.CmdUpdatePlayerModel(isItNext);
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void startGame()
    {
        MainGameManager.instance.startGameFromSelecting();
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void ExitRoom()
    {
        if (NetworkClient.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();

        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void closeSelectPanel()
    {
        selectPanel.SetActive(false);
    }
}

[System.Serializable]
public class playerSlotVariables
{
    public GameObject playerObject;
    public GameObject slotObject;
}
