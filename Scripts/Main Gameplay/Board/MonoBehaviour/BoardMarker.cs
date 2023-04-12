using UnityEngine;

public class BoardMarker : MonoBehaviour
{
    private void Start()
    {
        BoardMoveManager.onMoveDone += onPlayerLandComplete;
    }
    private void onPlayerLandComplete(PlotClass plotClass, BoardPlayer bp)
    {
        BoardMoveManager.onMoveDone -= onPlayerLandComplete;
        Destroy(this.gameObject);
    }
    private void OnDestroy()
    {
        BoardMoveManager.onMoveDone -= onPlayerLandComplete;
    }
}
