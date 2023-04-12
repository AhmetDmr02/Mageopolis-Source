using UnityEngine;

public class BarPosition : MonoBehaviour
{
    public Transform trackTransform;
    [SerializeField] private Transform cameraObj;
    private void Start()
    {
        trackTransform = this.transform;
        QueueManager.queueChanged += changePosition;
    }
    private void OnDestroy()
    {
        QueueManager.queueChanged -= changePosition;
    }
    private void FixedUpdate()
    {
        if (cameraObj == null)
        {
            cameraObj = Camera.main.transform;
        }
        // Rotate object to face camera
        transform.LookAt(cameraObj);

        // Get current rotation quaternion
        Quaternion currentRotation = transform.rotation;

        // Create a new quaternion that rotates 90 degrees around the z-axis
        Quaternion additionalRotation = Quaternion.Euler(0, 0, -90);

        // Combine the current rotation with the additional rotation
        Quaternion newRotation = currentRotation * additionalRotation;

        transform.rotation = newRotation;
        if (trackTransform != null)
            this.gameObject.transform.position = trackTransform.position;
    }
    private void changePosition(uint playerId)
    {
        BoardPlayer boardObject = UtulitiesOfDmr.ReturnBoardPlayerById(playerId);
        trackTransform = boardObject == null ? BoardMoveManager.instance.gameObject.transform : boardObject.transform.GetChild(4);
    }
}
