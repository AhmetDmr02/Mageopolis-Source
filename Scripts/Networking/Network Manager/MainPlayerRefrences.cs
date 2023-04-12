using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class MainPlayerRefrences : NetworkBehaviour
{
    public static MainPlayerRefrences instance;
    public SyncList<uint> mainPlayerIds = new SyncList<uint>();
    [SerializeField] private List<GameObject> playerObjects = new List<GameObject>();
    public List<GameObject> PlayerObjects => playerObjects;
    public static event Action playerListChanged;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    void FixedUpdate()
    {
        checkDisconnectedOrConnectedPlayers();
    }
    private void checkDisconnectedOrConnectedPlayers()
    {
        if (playerObjects.Any(x => x == null)) removeFromId();
        if (mainPlayerIds.Count != playerObjects.Count)
        {
            playerObjects.Clear();
            for (int i = 0; i < mainPlayerIds.Count; i++)
            {
                if (NetworkClient.spawned.TryGetValue(mainPlayerIds[i], out NetworkIdentity identity))
                {
                    if (identity.gameObject.GetComponent<NetworkPlayerCON>() == null) continue;
                    playerObjects.Add(identity.gameObject);
                    playerListChanged?.Invoke();
                    if (PlayerStatsGUI.instance != null)
                    {
                        PlayerStatsGUI.instance.RecalculateGUI(playerObjects.ToArray());
                        PlayerStatsGUI.instance.gameObject.GetComponent<PlayerSelectModelAndColor>().recalculateAll();
                        MainGameManager.instance.recalculateUsedColors();
                    }
                }
            }
        }
    }
    public void removeFromId()
    {
        if (!isServer) return;
        List<uint> currentPlayerIds = new List<uint>();
        foreach (uint uin in mainPlayerIds)
        {
            currentPlayerIds.Add(uin);
        }
        for (int i = 0; i < playerObjects.Count; i++)
        {
            if (playerObjects[i] == null) continue;
            currentPlayerIds.Remove(playerObjects[i].GetComponent<NetworkIdentity>().netId);
        }
        foreach (uint ui in currentPlayerIds)
        {
            mainPlayerIds.Remove(ui);
        }
        playerListChanged?.Invoke();
        //playerObjects.Clear();
    }
}
