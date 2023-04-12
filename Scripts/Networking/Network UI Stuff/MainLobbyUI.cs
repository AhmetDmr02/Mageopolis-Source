using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;
using EpicTransport;
using System.Collections.Generic;

public class MainLobbyUI : MonoBehaviour
{
    public static MainLobbyUI instance;
    [SerializeField] private TextMeshProUGUI mainTitle;
    [SerializeField] private GameObject[] mainSlots;
    [SerializeField] private AudioSource clickSound;
    [SerializeField] private Color emptyColor, ReadyColor, NotReadyColor;
    private NetworkRoomPlayerRev localPlayerObject;
    public GameObject[] playerObjects;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void refreshSlots()
    {
        if (!NetworkClient.active) return;
        if (!NetworkClient.ready) return;
        if (LobbySlotManager.instance == null) return;
        if (Application.loadedLevel != 0) return;
        playerObjects = LobbySlotManager.instance.players.ToArray();
        if (playerObjects.Length == 0) return;
        if (playerObjects[0] == null)
            return;
        mainTitle.text = playerObjects[0].GetComponent<NetworkRoomPlayerRev>().player_Name + "'s Room";
        fillSlotsWithPlayerObjects();
    }
    private void fillSlotsWithPlayerObjects()
    {
        int reverseEmptyint = 4;
        bool showKickButtonsToMe = false;
        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i].GetComponent<NetworkRoomPlayerRev>().isLocalPlayer)
            {
                showKickButtonsToMe = i == 0 ? true : false;
                mainSlots[i].transform.GetChild(1).gameObject.SetActive(true);
                localPlayerObject = playerObjects[i].GetComponent<NetworkRoomPlayerRev>();
            }
            if (playerObjects[i].GetComponent<NetworkRoomPlayerRev>().readyToBegin)
            {
                mainSlots[i].GetComponent<Image>().color = ReadyColor;
            }
            else
            {
                mainSlots[i].GetComponent<Image>().color = NotReadyColor;
            }
            mainSlots[i].transform.GetChild(2).gameObject.SetActive(showKickButtonsToMe);
            mainSlots[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerObjects[i].GetComponent<NetworkRoomPlayerRev>().player_Name;
            if (playerObjects[i].GetComponent<NetworkRoomPlayerRev>().player_Name == "") Invoke("refreshSlots", 0.1f);
            reverseEmptyint -= 1;
        }
        GameObject[] reversedObjects = (GameObject[])mainSlots.Clone();
        Array.Reverse(reversedObjects);
        for (int i = 0; i < reverseEmptyint; i++)
        {
            reversedObjects[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Empty";
            reversedObjects[i].transform.GetChild(1).gameObject.SetActive(false);
            reversedObjects[i].transform.GetChild(2).gameObject.SetActive(false);
            reversedObjects[i].GetComponent<Image>().color = emptyColor;
        }
    }
    public void giveReady()
    {
        if (localPlayerObject == null)
        {
            NotificationCreator.instance.createNotification("Error", "Local player not found!");
            return;
        }
        localPlayerObject.readyToBegin = !localPlayerObject.readyToBegin;
        clickSound.Play();
        localPlayerObject.CmdChangeReadyState(localPlayerObject.readyToBegin);
        localPlayerObject.CmdChangedReady(localPlayerObject.readyToBegin);
        EventSystem.current.SetSelectedGameObject(null);
    }

    //This list is some sort of anticheat
    //We Will First Ask Client For Disconnect If Is Edited Client Second Click Will Kick From Server
    private Dictionary<int, int> kickPlayerCounter = new Dictionary<int, int>();
    public void kickPlayer(int index)
    {
        if (localPlayerObject == null)
        {
            NotificationCreator.instance.createNotification("Error", "Local player not found!");
            return;
        }
        if (!kickPlayerCounter.ContainsKey(index)) kickPlayerCounter.Add(index, 0);
        int localKickPlayerCounter = kickPlayerCounter[index];
        if (localKickPlayerCounter == 0)
        {
            if (!MainMenuManager.instance.isEpicActive)
            {
                localPlayerObject.RpcKickPlayer(LobbySlotManager.instance.players[index].GetComponent<NetworkRoomPlayerRev>().connectionToClient);
                clickSound.Play();
                EventSystem.current.SetSelectedGameObject(null);
                kickPlayerCounter[index] += 1;
                return;
            }
            else
            {
                localPlayerObject.RpcKickEpicPlayer(LobbySlotManager.instance.players[index].GetComponent<NetworkRoomPlayerRev>().connectionToClient);
                clickSound.Play();
                EventSystem.current.SetSelectedGameObject(null);
                kickPlayerCounter[index] += 1;
                return;
            }
        }

        localPlayerObject.RpcKickPlayerNotification(LobbySlotManager.instance.players[index].GetComponent<NetworkRoomPlayerRev>().connectionToClient);
        if (MainMenuManager.instance.isEpicActive)
        {
            MainMenuManager.instance.kickEpicPlayer(index);
        }
        else
        {
            NetworkConnection conn = LobbySlotManager.instance.players[index].GetComponent<NetworkRoomPlayerRev>().netIdentity.connectionToClient;
            conn.Disconnect();
            kickPlayerCounter.Clear();
        }
        clickSound.Play();
        EventSystem.current.SetSelectedGameObject(null);
    }
}
