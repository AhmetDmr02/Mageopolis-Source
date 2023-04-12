using UnityEngine;
using TMPro;
public class NotificationCreator : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Canvas mainCanvas;
    [HideInInspector] public static NotificationCreator instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void createNotification(string title, string content)
    {
        GameObject GO = Instantiate(notificationPanel, mainCanvas.transform);
        GO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = title;
        GO.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = content;
    }
}
