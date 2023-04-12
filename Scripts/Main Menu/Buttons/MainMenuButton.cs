using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using Mirror;
using kcp2k;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private buttonProperties buttonProperties_;
    [SerializeField] private float buttonScaleRateMax, buttonScaleRateMin;
    [SerializeField] private GameObject panel_;
    [SerializeField] private AudioSource clickSound;
    public static bool isPanelActive;
    private float desiredScale;
    [SerializeField] private Canvas parentCanvas;
    //Join button
    [SerializeField] private bool isJoinbool, isHostbool;
    [SerializeField] private bool isEpicActive;
    [SerializeField][ShowWhen("isJoinbool")] private TextMeshProUGUI joinText;
    [SerializeField][ShowWhen("isJoinbool")] private TMP_InputField joinIpInputfield;
    [SerializeField][ShowWhen("isJoinbool")] private TMP_InputField joinPortInputfield;
    [SerializeField][ShowWhen("isJoinbool")] private TMP_InputField joinNameInputfield;
    [SerializeField][ShowWhen("isHostbool")] private TMP_InputField joinHostInputfield;
    [SerializeField] private KcpTransport kcpTransport;
    private void Start()
    {
        desiredScale = buttonScaleRateMin;
    }

    private void Update()
    {
        //X,Y,Z Doesn't matter
        //Debug.Log(isPanelActive);
        if (this.transform.localScale.x != desiredScale)
        {
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, new Vector3(desiredScale, desiredScale, desiredScale), 0.2f);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (isPanelActive) return;
        switch (buttonProperties_)
        {
            case buttonProperties.JoinLobby:
                InitOpenPanelAnimation();
                break;
            case buttonProperties.CreateLobby:
                InitOpenPanelAnimation();
                break;
            case buttonProperties.Options:
                InitOpenPanelAnimation();
                break;
            case buttonProperties.Exit:
                Application.Quit();
                break;
        }
        clickSound.Play();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isPanelActive) return;
        desiredScale = buttonScaleRateMax;
        this.gameObject.GetComponent<AudioSource>().Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        desiredScale = buttonScaleRateMin;
    }

    public void InitOpenPanelAnimation()
    {
        Animator anim = this.GetComponent<Animator>();
        isPanelActive = true;
        anim.Play("OpenButton");
    }
    public void ClosePanel()
    {
        panel_.SetActive(false);
    }
    public void OpenPanel()
    {
        panel_.SetActive(true);
    }
    public void hostRoom()
    {
        try
        {
            string nameTrimmedEnd = joinHostInputfield.text.TrimEnd();
            string name = nameTrimmedEnd.TrimStart();
            char[] c = name.ToCharArray();
            if (c.Length > 25) { NotificationCreator.instance.createNotification("Error", "Name length is excessive!"); return; }
            if (name == "") { NotificationCreator.instance.createNotification("Error", "Enter name"); return; }
            if (!NetworkClient.isConnected)
            {
                if (isEpicActive)
                {
                    MainMenuManager.instance.setupEpicLobby(name);
                    MainMenuManager.instance.playerName = name;
                    EventSystem.current.SetSelectedGameObject(null);
                    InitClosePanelAnimation();
                    return;
                }
                NetworkManager.singleton.StartHost();
            }
            else if (NetworkClient.isConnected && NetworkClient.active)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            MainMenuManager.instance.playerName = name;
            EventSystem.current.SetSelectedGameObject(null);
            InitClosePanelAnimation();
        }
        catch (System.Exception er)
        {
            NotificationCreator.instance.createNotification("Error", er.ToString());
            return;
        }
    }
    public void FixedUpdate()
    {
        if (buttonProperties_ != buttonProperties.JoinLobby) return;
        if (Application.loadedLevel == 0)
        {
            if (NetworkClient.isConnecting)
            {
                joinText.GetComponent<TextMeshProUGUI>().text = "Cancel Join Request";
            }
            else
            {
                joinText.GetComponent<TextMeshProUGUI>().text = "Join Lobby";
            }
        }
    }
    public void joinRoom()
    {
        try
        {
            string nameTrimmedEnd = joinNameInputfield.text.TrimEnd();
            string name = nameTrimmedEnd.TrimStart();
            char[] c = name.ToCharArray();
            if (c.Length > 25) { NotificationCreator.instance.createNotification("Error", "Name length is excessive!"); return; }
            if (name == "") { NotificationCreator.instance.createNotification("Error", "Enter name"); return; }
            if (NetworkClient.isConnected) { NotificationCreator.instance.createNotification("Error", "You are already connected!"); return; }
            if (NetworkClient.isConnecting)
            {
                NetworkManager.singleton.StopClient();
                return;
            }
            if (NetworkClient.isConnecting) { NotificationCreator.instance.createNotification("Error", "You are already trying to connecting!"); return; }
            string ip = joinIpInputfield.text.Trim();
            string port = joinPortInputfield.text.Trim();
            if (isEpicActive)
            {
                Debug.Log("Epic Active");
                MainMenuManager.instance.joinEpicLobby(ip);
                EventSystem.current.SetSelectedGameObject(null);
                InitClosePanelAnimation();
                MainMenuManager.instance.playerName = name;
                return;
            }
            MainMenuManager.instance.playerName = name;
            if (ip == "localhost")
            {
                NetworkManager.singleton.networkAddress = ip;
            }
            else
            {
                NetworkManager.singleton.networkAddress = ip;
                ushort blankPortValue;
                if (ushort.TryParse(port.Trim(), out blankPortValue))
                {
                    if (port.Trim() == "") kcpTransport.Port = blankPortValue;
                }
            }
            NetworkManager.singleton.StartClient();
            EventSystem.current.SetSelectedGameObject(null);
            InitClosePanelAnimation();
        }
        catch (System.Exception err)
        {
            NotificationCreator.instance.createNotification("Error", err.ToString());
            return;
        }
    }
    public void InitClosePanelAnimation()
    {
        Animator anim = this.GetComponent<Animator>();
        anim.Play("CloseButton");
        EventSystem.current.SetSelectedGameObject(null);
        isPanelActive = false;
    }
}
public enum buttonProperties
{
    JoinLobby,
    CreateLobby,
    Options,
    Exit
}
