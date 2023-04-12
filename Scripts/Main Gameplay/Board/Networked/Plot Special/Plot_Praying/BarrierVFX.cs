using UnityEngine;

public class BarrierVFX : MonoBehaviour
{
    public int ownedPlotId;
    public void subscribeEvent()
    {
        PlotClass.LandBarrierWiped += listenForWipes;
    }
    private void OnDestroy()
    {
        PlotClass.LandBarrierWiped -= listenForWipes;
    }
    private void listenForWipes(int plotId)
    {
        if (plotId != ownedPlotId) return;
        PlotClass.LandBarrierWiped -= listenForWipes;
        Destroy(this.gameObject);
    }
}
