using UnityEngine;

public class LookatY : MonoBehaviour
{
    [SerializeField] private Transform lookatObject;
    private Transform lookatTarget;
    void FixedUpdate()
    {
        if (BoardRefrenceHolder.instance == null) return;
        lookatTarget = BoardRefrenceHolder.instance.boardCamera.transform;
        transform.LookAt(transform.position + lookatTarget.transform.rotation * Vector3.forward,
            lookatTarget.transform.rotation * Vector3.up);
    }
}
