using TMPro;
using UnityEngine;
using Mirror;

public class VacuumSystem : NetworkBehaviour
{
    [SyncVar(hook = "refreshText")]
    private int vacuumMoney;
    public int VacuumMoney => vacuumMoney;
    public static VacuumSystem instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    void refreshText(int oldVal, int newVal)
    {
        BoardRefrenceHolder.instance.vacuumText.text = $"Total: {newVal}";
    }
    public void adjustMoney(int value)
    {
        vacuumMoney += value;
    }
    public void decreaseMoney(int value)
    {
        vacuumMoney -= value;
    }
}
