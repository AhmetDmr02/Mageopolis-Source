using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Shapes;

public class MainNetworkManagerCON : NetworkRoomManager
{
    public static MainNetworkManagerCON instance { get; private set; }

    public override void onAwake()
    {
        base.onAwake();
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    public override void onStart()
    {
        base.onStart();
        playerSceneLoaded += IsSceneLoadedForAll;
    }
    public override void onDestroy_()
    {
        base.onDestroy_();
        playerSceneLoaded -= IsSceneLoadedForAll;
    }
    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerDisconnect(conn);
        if (LobbySlotManager.instance != null)
            LobbySlotManager.instance.playerIDS.Remove(conn.identity.netId);
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (QueueManager.instance != null)
            QueueManager.instance.ReCheckQueue(conn);
        //if (MainPlayerRefrences.instance != null)
        //    MainPlayerRefrences.instance.mainPlayerIds.Remove(conn.identity.netId);

        if (conn.identity.gameObject.GetComponent<NetworkRoomPlayerRev>() != null)
        {
            AnimatedTextCreator.instance.CreateAnimatedText($"{conn.identity.gameObject.GetComponent<NetworkRoomPlayerRev>().player_Name} is disconnected.", Color.red);
        }
        else if (conn.identity.gameObject.GetComponent<NetworkPlayerCON>() != null)
        {
            AnimatedTextCreator.instance.CreateAnimatedText($"{conn.identity.gameObject.GetComponent<NetworkPlayerCON>().PlayerName} is disconnected.", Color.red);
            conn.identity.gameObject.GetComponent<NetworkPlayerCON>().Disconnected = true;
        }
        base.OnServerDisconnect(conn);
        CalculateModelAndColorAttribute();
        CheckAgainIfSceneLoadedForAll();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (MainMenuManager.instance != null)
            MainMenuManager.instance.onHostStopCall();
    }
    public void CalculateModelAndColorAttribute()
    {
        if (NetworkPlayerCON.localPlayerCON == null) return;
        NetworkPlayerCON.localPlayerCON.RpcCalculateModelAndColorAttribute();
    }
    int playerSceneCounter;
    public void IsSceneLoadedForAll()
    {
        playerSceneCounter += 1;
        if (roomSlots.Count <= playerSceneCounter)
            MainGameManager.instance.isEveryOneLoaded = true;
    }
    void CheckAgainIfSceneLoadedForAll()
    {
        if (roomSlots.Count <= playerSceneCounter)
            MainGameManager.instance.isEveryOneLoaded = true;
    }
}
