using UnityEngine;

public class DiceParticlesOpener : MonoBehaviour
{
    public static bool highlighted { private set; get; }
    void Start()
    {
        QueueManager.queueChanged += CheckParticles;
        DiceManager.diceFired += CloseParticles;
        CloseParticles(0, 0, null);
    }
    private void OnDestroy()
    {
        QueueManager.queueChanged -= CheckParticles;
        DiceManager.diceFired -= CloseParticles;
    }
    public static void CheckParticles(uint playerId)
    {
        if (QueueManager.instance.CurrentQueue == NetworkPlayerCON.localPlayerCON.netId)
        {
            BoardRefrenceHolder.instance.DiceParticles.SetActive(true);
            highlighted = true;
        }
        else
        {
            BoardRefrenceHolder.instance.DiceParticles.SetActive(false);
            highlighted = false;
        }
    }
    public static void CloseParticles(int i, int i2, BoardPlayer bp)
    {
        BoardRefrenceHolder.instance.DiceParticles.SetActive(false);
        highlighted = false;
    }
}
