using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class LobbySlotManager : NetworkBehaviour
{
    public static LobbySlotManager instance;
    [SyncVar] public int availableID;
    public SyncList<uint> playerIDS = new SyncList<uint>();
    public List<GameObject> players = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void LateUpdate()
    {
        CheckPlayers();
    }
    private void CheckPlayers()
    {
        if (playerIDS.Count != players.Count)
        {
            players.Clear();
            for (int i = 0; i < playerIDS.Count; i++)
            {
                if (NetworkClient.spawned.TryGetValue(playerIDS[i], out NetworkIdentity identity))
                {
                    if (identity.gameObject.GetComponent<NetworkRoomPlayerRev>() == null) return;
                    players.Add(identity.gameObject);
                    if (MainLobbyUI.instance != null) MainLobbyUI.instance.refreshSlots();
                }
            }
        }
    }
    public void addObjectToList(uint identityId)
    {
        if (!isServer) return;
        playerIDS.Add(identityId);
    }
}
