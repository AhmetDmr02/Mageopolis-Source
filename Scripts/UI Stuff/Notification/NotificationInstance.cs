using UnityEngine;

public class NotificationInstance : MonoBehaviour
{
    public void closeNotification()
    {
        Destroy(this.gameObject);
    }
}
