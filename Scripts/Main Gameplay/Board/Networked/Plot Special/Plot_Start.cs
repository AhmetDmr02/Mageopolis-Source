using UnityEngine;
using Mirror;

public class Plot_Start : NetworkBehaviour
{
    [SerializeField] private int moneyOnLand;

    private void Start()
    {
        BoardMoveManager.onMove += givePlayerMoney;
        BoardMoveManager.onMoveDone += givePlayerMoney;
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMove -= givePlayerMoney;
        BoardMoveManager.onMoveDone -= givePlayerMoney;
    }
    private void givePlayerMoney(PlotClass plotClass_, BoardPlayer boardPlayer_)
    {
        if (!isServer) return;
        if (plotClass_.landName != "Start") return;
        NetworkPlayerCON netPC = boardPlayer_.networkPlayerObject.GetComponent<NetworkPlayerCON>();
        netPC.PlayerMoney += moneyOnLand;
    }
    public void setSalary(int salary)
    {
        moneyOnLand = salary;
    }
}