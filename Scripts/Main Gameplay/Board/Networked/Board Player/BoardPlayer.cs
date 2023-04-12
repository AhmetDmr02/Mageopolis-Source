using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;

public class BoardPlayer : NetworkBehaviour
{
    [Header("Offline Stuff")]
    [SerializeField] private GameObject playerNamePanel;
    [SerializeField] private Transform barTransformHolder;
    public bool togglePlayerNames = false;

    [Space(15), Header("Online Stuff")]
    [SyncVar(hook = "onNetworkObjectChangedHook")]
    [SerializeField] private GameObject networkObjectOfPlayer;

    [SyncVar(hook = nameof(revalidateCurrentLandIndex))]
    public int currentLandIndex;
    public bool currentLandIndexOutdated;

    public int currentAnimationPlotIndex;

    [SyncVar]
    public bool playerFinishedTravel;

    public GameObject networkPlayerObject => networkObjectOfPlayer;

    [SyncVar]
    public uint representedPlayerId;

    [HideInInspector] public int previousDiceOne, previousDiceTwo;


    private void Start()
    {
        //QueueManager.queueChanged += checkTogglePlayerNames;
        RaycastCenter.lookingObjectChanged += checkTogglePlayerNames;
        RaycastCenter.lookingObjectLeftClicked += togglePlayerPanel;
        RaycastCenter.lookingObjectChanged += SetPlayerGold;
        if (isServer)
        {
            playerFinishedTravel = true;
        }
    }
    private void OnDestroy()
    {
        //Add Later For Cooldown UI
        //QueueManager.queueChanged -= checkTogglePlayerNames;
        RaycastCenter.lookingObjectChanged -= checkTogglePlayerNames;
        RaycastCenter.lookingObjectLeftClicked -= togglePlayerPanel;
        RaycastCenter.lookingObjectChanged -= SetPlayerGold;
    }
    [ClientRpc]
    public void RpcSetUpObjectProperties(GameObject playerObject, Color32 color32, uint representedId)
    {
        Renderer materialRenderer = this.gameObject.transform.GetChild(0).GetComponent<Renderer>();
        materialRenderer.material.color = color32;
        if (!isServer) return;
        networkObjectOfPlayer = playerObject;
        //Who actually owns player
        representedPlayerId = representedId;
    }
    private void onNetworkObjectChangedHook(GameObject oldVal, GameObject newVal)
    {
        SetPlayerNamePanel(oldVal, newVal);
        SetPlayerGold(newVal);
        RefreshQueueIndicator(oldVal, newVal);
    }
    #region UI Stuff
    private void RefreshQueueIndicator(GameObject oldVal, GameObject newVal)
    {
        this.gameObject.transform.GetChild(1).GetComponent<QueueIndicator>().checkQueueIndicators(QueueManager.instance.CurrentQueue);
    }
    public void checkTogglePlayerNames(GameObject go)
    {
        if (go == null) { this.gameObject.transform.GetChild(3).gameObject.SetActive(false); return; }
        StopAllCoroutines();
        if (togglePlayerNames)
            StartCoroutine(fadeOpenPanel(true));
        else if (!go.transform.IsChildOf(this.gameObject.transform))
            StartCoroutine(fadeOpenPanel(false));
        else if (go.transform.IsChildOf(this.gameObject.transform))
            StartCoroutine(fadeOpenPanel(true));
    }
    private IEnumerator fadeOpenPanel(bool open)
    {
        CanvasGroup cg = this.gameObject.transform.GetChild(3).GetComponent<CanvasGroup>();
        if (open)
        {
            this.gameObject.transform.GetChild(3).gameObject.SetActive(true);
            while (cg.alpha < 1)
            {
                cg.alpha += 1.2f * Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            if (cg.alpha == 0) yield break;
            while (cg.alpha > 0)
            {
                cg.alpha -= 2f * Time.deltaTime;
                yield return null;
            }
            this.gameObject.transform.GetChild(3).gameObject.SetActive(false);
        }
        yield return null;
    }
    private void togglePlayerPanel(GameObject go)
    {
        if (go.transform.IsChildOf(this.gameObject.transform))
        {
            togglePlayerNames = !togglePlayerNames;
            checkTogglePlayerNames(this.gameObject.transform.GetChild(0).gameObject);
            SoundEffectManager.instance.CreateDummyAudioAt(SoundEffectManager.instance.clickSound2, this.transform.position, 1, 1, 0.5f, 0);
        }
    }
    private void SetPlayerNamePanel(GameObject oldVal, GameObject newVal)
    {
        TextMeshProUGUI playerNameText = this.gameObject.transform.GetChild(3).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        playerNameText.text = newVal.GetComponent<NetworkPlayerCON>().PlayerName;
        playerNameText.color = newVal.GetComponent<NetworkPlayerCON>().playerColor;
        checkTogglePlayerNames(null);

        //NOTE: ADD LATER FOR COOLDOWN BAR CHECK
        //checkTogglePlayerNames(QueueManager.instance.CurrentQueue);
    }
    private void SetPlayerGold(GameObject boardPlayer)
    {
        TextMeshProUGUI playerCoinText = this.gameObject.transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        if (networkObjectOfPlayer != null)
        {
            if (networkObjectOfPlayer.GetComponent<NetworkPlayerCON>().PlayerMoney < 0)
            {
                playerCoinText.text = "0";
            }
            else
            {
                string moneyString = networkObjectOfPlayer.GetComponent<NetworkPlayerCON>().PlayerMoney.ToString("000,000,000");
                string moneyString2 = moneyString.TrimStart('0', '.');
                playerCoinText.text = moneyString2;
            }
        }
    }
    private void revalidateCurrentLandIndex(int oldVal, int newVal)
    {
        currentLandIndexOutdated = false;
    }
    [ClientRpc]
    public void SetFalseOutdatedBool()
    {
        currentLandIndexOutdated = false;
    }
    #endregion
}
