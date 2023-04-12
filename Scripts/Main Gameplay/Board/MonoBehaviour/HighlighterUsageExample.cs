using System.Collections.Generic;
using UnityEngine;

public class HighlighterUsageExample : MonoBehaviour
{
    // This script created for easier debug purposes
    public static HighlighterUsageExample instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void OnDestroy()
    {
        Highlighter.HighlighterClickCallback -= callbackFromHighlighter;
    }
    public void requestHighlight()
    {
        if (Highlighter.instance == null) return;
        List<PlotClass> pc = new List<PlotClass>();
        int i = 0;
        foreach (GameObject go in BoardMoveManager.instance.LandGameobject)
        {
            i += 1;
            if (i % 2 == 0) continue;
            pc.Add(go.GetComponent<PlotClass>());
        }
        Highlighter.instance.SwitchToHighlightMode(pc.ToArray(), true);
        Highlighter.HighlighterClickCallback += callbackFromHighlighter;
        Highlighter.HighlighterHoverCallback += callbackHoverFromHighlighter;
        requestedHighlight = true;
    }
    private bool requestedHighlight;
    private void callbackFromHighlighter(PlotClass[] classes)
    {
        if (!requestedHighlight) return;
        requestedHighlight = false;
        Highlighter.HighlighterClickCallback -= callbackFromHighlighter;
        foreach (PlotClass plotClass_ in classes)
        {
            Debug.Log($"{plotClass_.landName} was selected");
        }
    }
    private void callbackHoverFromHighlighter(PlotClass class_)
    {
        Debug.Log($"{class_.landName} was hovered");
    }
}
