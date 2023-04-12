using UnityEngine;
using UnityEngine.EventSystems;

public class TeleportSkipButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TeleportManager.instance != null) TeleportManager.instance.RequestSkip();
        EventSystem.current.SetSelectedGameObject(null);
    }
}
