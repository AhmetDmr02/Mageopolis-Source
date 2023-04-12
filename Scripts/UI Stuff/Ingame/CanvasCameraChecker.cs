using UnityEngine;

public class CanvasCameraChecker : MonoBehaviour
{

    void FixedUpdate()
    {
        if (BoardRefrenceHolder.instance == null) return;
        this.transform.GetComponent<Canvas>().worldCamera = BoardRefrenceHolder.instance.UIboardCamera;
    }
}
