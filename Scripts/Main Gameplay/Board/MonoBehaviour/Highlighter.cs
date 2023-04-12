using System.Collections.Generic;
using UnityEngine;
using System;

public class Highlighter : MonoBehaviour
{
    public bool IsInHighlightMode { get; private set; }
    [SerializeField] private GameObject highlightShadeObject;
    [SerializeField] private HighlighterButton finishSelectedButton;

    private Dictionary<PlotClass, float> normalFixedYPositions = new Dictionary<PlotClass, float>();
    //Main goal of this dict is basically keep track of selected objects particles and also to return selected plot classes
    private Dictionary<PlotClass, GameObject> selectedHighlightObjects = new Dictionary<PlotClass, GameObject>();
    public Dictionary<PlotClass, GameObject> SelectedHighlihtObjects => selectedHighlightObjects;

    private List<PlotClass> allHighlightedObjects = new List<PlotClass>();

    public static Highlighter instance;

    private GameObject cachedGO;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        RaycastCenter.lookingObjectChanged += adjustLookHeight;
        finishSelectedButton.clickedButton += finishSelecting;
    }
    private void OnDestroy()
    {
        RaycastCenter.lookingObjectChanged -= adjustLookHeight;
        finishSelectedButton.clickedButton -= finishSelecting;
    }

    private void finishSelecting()
    {
        if (!IsInHighlightMode) return;
        List<PlotClass> returnClass = new List<PlotClass>();
        foreach (KeyValuePair<PlotClass, GameObject> selectedPlots in selectedHighlightObjects)
        {
            returnClass.Add(selectedPlots.Key);
        }
        HighlighterClickCallback?.Invoke(returnClass.ToArray());
        //CloseHighlightMode();
    }

    private void listenForRaycastHit(GameObject go)
    {
        if (!IsInHighlightMode) return;
        if (go.GetComponent<PlotClass>() == null) return;
        if (go.GetComponent<PlotClass>().Highlighted == false) return;
        PlotClass plotClass_ = go.GetComponent<PlotClass>();
        if (allowMultipleSelection)
        {
            if (selectedHighlightObjects.ContainsKey(plotClass_))
            {
                Destroy(this.selectedHighlightObjects[plotClass_]);
                SoundEffectManager.instance.PlayClickSound();
                this.selectedHighlightObjects.Remove(plotClass_);
                HighlighterMultipleSelectArrayChanged?.Invoke();
            }
            else
            {
                GameObject createdParticle = EffectManager.instance.createHighlightParticle(plotClass_);
                SoundEffectManager.instance.PlayClickSound();
                if (IsInHighlightMode) this.selectedHighlightObjects.Add(plotClass_, createdParticle);
                HighlighterMultipleSelectArrayChanged?.Invoke();
            }
        }
        else
        {
            //CloseHighlightMode();
            SoundEffectManager.instance.PlayClickSound();
            PlotClass[] plotClass = { plotClass_ };
            HighlighterClickCallback?.Invoke(plotClass);
        }
    }

    private void adjustLookHeight(GameObject go)
    {
        if (!IsInHighlightMode) return;
        if (cachedGO != null && cachedGO.GetComponent<PlotClass>() != null)
        {
            cachedGO.GetComponent<PlotClass>().changeFixedY(8.5f, false);
        }
        if (go.GetComponent<PlotClass>() == null)
        {
            HighlighterHoverCallback?.Invoke(null);
            return;
        }
        if (go.GetComponent<PlotClass>().Highlighted == false)
        {
            HighlighterHoverCallback?.Invoke(null);
            return;
        }
        go.GetComponent<PlotClass>().changeFixedY(normalFixedYPositions[go.GetComponent<PlotClass>()], false);
        go.GetComponent<PlotClass>().changeFixedY(11f, true);
        cachedGO = go;
        HighlighterHoverCallback?.Invoke(go.GetComponent<PlotClass>());
    }

    #region Switching Highlight
    private bool allowMultipleSelection;
    public static Action<PlotClass[]> HighlighterClickCallback;
    public static Action<PlotClass> HighlighterHoverCallback;
    public static Action HighlighterMultipleSelectArrayChanged;
    public void SwitchToHighlightMode(PlotClass[] highlightPlots, bool allowMultipleSelection_)
    {
        if (IsInHighlightMode) return;
        setupBasicHighlight();
        foreach (PlotClass plotClass_ in highlightPlots)
        {
            float normalY = plotClass_.returnFixedY();
            normalFixedYPositions.Add(plotClass_, normalY);
            allHighlightedObjects.Add(plotClass_);
            plotClass_.changeFixedY(8.5f, true);
            plotClass_.Highlighted = true;
        }
        allowMultipleSelection = allowMultipleSelection_;
        if (allowMultipleSelection) finishSelectedButton.activateCanvas();
        RaycastCenter.lookingObjectLeftClicked += listenForRaycastHit;
    }
    public void CloseHighlightMode()
    {
        if (!IsInHighlightMode) return;
        RaycastCenter.lookingObjectLeftClicked -= listenForRaycastHit;
        IsInHighlightMode = false;
        highlightShadeObject.SetActive(false);
        finishSelectedButton.deactivateCanvas();
        foreach (KeyValuePair<PlotClass, float> library in normalFixedYPositions)
        {
            float normalY = library.Value;
            library.Key.changeFixedY(normalY, false);
            library.Key.Highlighted = false;
        }
        foreach (KeyValuePair<PlotClass, GameObject> Effectlibrary in selectedHighlightObjects)
        {
            if (Effectlibrary.Value == null) continue;
            Destroy(Effectlibrary.Value);
        }
        cachedGO = null;
        clearLists();
        //Closing action subscriptions
        Delegate[] delegates = HighlighterClickCallback?.GetInvocationList();
        if (delegates != null)
        {
            foreach (Delegate delegate_ in delegates)
            {
                if (delegate_.GetType() == typeof(Action<PlotClass[]>))
                    HighlighterClickCallback -= (Action<PlotClass[]>)delegate_;
            }
        }
        Delegate[] HoverDelegates = HighlighterHoverCallback?.GetInvocationList();
        if (HoverDelegates != null)
        {
            foreach (Delegate HoverDelegate_ in HoverDelegates)
            {
                if (HoverDelegate_.GetType() == typeof(Action<PlotClass>))
                    HighlighterHoverCallback -= (Action<PlotClass>)HoverDelegate_;
            }
        }
        Delegate[] changeDelegates = HighlighterMultipleSelectArrayChanged?.GetInvocationList();
        if (changeDelegates != null)
        {
            foreach (Delegate changeDelegate in changeDelegates)
            {
                if (changeDelegate.GetType() == typeof(Action))
                    HighlighterMultipleSelectArrayChanged -= (Action)changeDelegate;
            }
        }
        EndTourButton.CheckButton(null, null);
    }
    #endregion
    private void setupBasicHighlight()
    {
        IsInHighlightMode = true;
        highlightShadeObject.SetActive(true);
        clearLists();
    }
    private void clearLists()
    {
        normalFixedYPositions.Clear();
        allHighlightedObjects.Clear();
        selectedHighlightObjects.Clear();
    }
}
