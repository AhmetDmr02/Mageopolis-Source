using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HighlighterButton : MonoBehaviour, IPointerClickHandler
{
    public event Action clickedButton;
    [SerializeField] private Canvas highlighterButtonCanvas;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Highlighter.instance == null)
        {
            highlighterButtonCanvas.enabled = false;
            return;
        }
        if (!Highlighter.instance.IsInHighlightMode)
        {
            highlighterButtonCanvas.enabled = false;
            return;
        }
        clickedButton?.Invoke();
        SoundEffectManager.instance.PlayClickSound();
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void activateCanvas()
    {
        highlighterButtonCanvas.enabled = true;
    }
    public void deactivateCanvas()
    {
        highlighterButtonCanvas.enabled = false;
    }
}
