using Mirror;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DeveloperPortal : MonoBehaviour
{
    //public bool toggle;
    //public GameObject go;

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F6))
    //    {
    //        toggle = !toggle;
    //    }
    //}

    //private void FixedUpdate()
    //{
    //    if (toggle)
    //    {
    //        if (!NetworkClient.ready) return;
    //        if (Application.loadedLevel != 0) return;
    //        GameObject[] playerObjects = LobbySlotManager.instance.players.ToArray();
    //        go = playerObjects[0];
    //        NetworkRoomPlayerRev localPlayerObject = go.GetComponent<NetworkRoomPlayerRev>();
    //        localPlayerObject.readyToBegin = !localPlayerObject.readyToBegin;
    //        localPlayerObject.CmdChangeReadyState(localPlayerObject.readyToBegin);
    //        localPlayerObject.CmdChangedReady(localPlayerObject.readyToBegin);
    //        EventSystem.current.SetSelectedGameObject(null);
    //        /*
    //        if (!NetworkClient.ready) return;
    //        NetworkClient.localPlayer.GetComponent<NetworkRoomPlayerExt>().CmdChangeReadyState(!NetworkClient.localPlayer.GetComponent<NetworkRoomPlayerExt>().readyToBegin);
    //        */
    //    }
    //}
}
