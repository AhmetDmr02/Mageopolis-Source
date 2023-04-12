using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // The object to orbit around
    [SerializeField] private float distance = 200f; // The distance of the camera from the target
    [SerializeField] private float xSpeed = 120f; // The speed of the horizontal rotation
    [SerializeField] private float ySpeed = 120f; // The speed of the vertical rotation

    [SerializeField] private float yMinLimit = -20f;
    [SerializeField] private float yMaxLimit = 80f;
    [SerializeField] private float distanceMin = 60f;
    [SerializeField] private float distanceMax = 300f;
    [SerializeField] private float scrollSpeed = 25f;

    private float x = 0f; // The horizontal rotation
    private float y = 0f; // The vertical rotation

    [SerializeField] private bool perspectiveMode = false;
    [SerializeField] private Vector3 orthographicLoc;
    [SerializeField] private Vector3 orthographicRot;

    private Transform previousTransform;
    private float previousDistance = 0f;
    private Vector3 lookObjectVec;
    private void Start()
    {
        RaycastCenter.lookingObjectMiddleClicked += setTarget;
    }
    private void OnDestroy()
    {
        RaycastCenter.lookingObjectMiddleClicked -= setTarget;
    }
    void Update()
    {
        if (target != null)
        {
            if (!perspectiveMode) return;
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime, distanceMin, distanceMax);

            //If Player Changes Target 
            if (target != previousTransform)
            {
                previousTransform = target;
                lookObjectVec = target.GetComponent<Renderer>().bounds.center;
                Quaternion rotation_ = Quaternion.Euler(y, x, 0f);
                Vector3 position_ = rotation_ * new Vector3(0f, 0f, -distance) + lookObjectVec;

                transform.rotation = rotation_;
                transform.position = position_;
            }

            //The reason for we don't update our camera position and rotation not because only for performance
            //But for the camera shaker
            if (distance == previousDistance && !Input.GetMouseButton(1)) return;
            previousDistance = distance;
            //Only Set lookObjectVec When Target Changes
            Quaternion rotation = Quaternion.Euler(y, x, 0f);
            Vector3 position = rotation * new Vector3(0f, 0f, -distance) + lookObjectVec;

            transform.rotation = rotation;
            transform.position = position;
        }
    }
    public void switchPerspectiveMode(bool isItOrthographic)
    {
        perspectiveMode = !isItOrthographic;
        if (perspectiveMode)
        {
            this.gameObject.GetComponent<Camera>().orthographic = isItOrthographic;
            this.gameObject.transform.GetChild(0).GetComponent<Camera>().orthographic = isItOrthographic;
            x = orthographicRot.x;
            y = orthographicRot.y;
        }
        else
        {
            this.gameObject.GetComponent<Camera>().orthographic = isItOrthographic;
            this.gameObject.transform.GetChild(0).GetComponent<Camera>().orthographic = isItOrthographic;
            this.gameObject.transform.position = orthographicLoc;
            this.gameObject.transform.eulerAngles = orthographicRot;
            distance = 200;
        }
    }
    private void setTarget(GameObject lookingObject)
    {
        if (!perspectiveMode) return;
        target = lookingObject.transform;
    }
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void setScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
    public void setXSpeed(float speed)
    {
        xSpeed = speed;
    }
    public void setYSpeed(float speed)
    {
        ySpeed = speed;
    }
}
