using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Mirror;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices;
using EpicTransport;

public class MainMenuManager : EOSLobby
{
    //Add Info Piercer
    [Header("Main Menu,Main UI Assigns")]
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Slider setTargetFpsSlider;
    [SerializeField] private TextMeshProUGUI targetFpsGUI;
    [SerializeField] private TMP_Dropdown resDropdown;
    [SerializeField] private TMP_Dropdown graphicDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle closeMusicToggle;
    [SerializeField] private Toggle vsyncToggle;
    [Space(15)]
    [Header("Game Object Assigns")]
    [SerializeField] private AudioSource mainMenuMusicSource;
    [SerializeField] private Canvas mainMenuCanvas, roomCanvas;
    [Header("Misc")]
    public static MainMenuManager instance;
    public bool isEpicActive;
    [SerializeField] private GameObject EpicJoining;
    Resolution[] resolutions;
    //
    [HideInInspector] public string playerName;
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
    private void OnEnable()
    {
        //subscribe to events
        if (isEpicActive)
        {
            CreateLobbySucceeded += OnCreateLobbySuccess;
            JoinLobbySucceeded += OnJoinLobbySuccess;
            LeaveLobbySucceeded += OnLeaveLobbySuccess;
            CreateLobbyFailed += lobbyCreationFailed;
            JoinLobbyFailed += joinLobbyFailed;
            FindLobbiesFailed += joinLobbyFailed;
        }
    }

    //deregister events
    private void OnDisable()
    {
        if (isEpicActive)
        {
            //unsubscribe from events
            CreateLobbySucceeded -= OnCreateLobbySuccess;
            JoinLobbySucceeded -= OnJoinLobbySuccess;
            LeaveLobbySucceeded -= OnLeaveLobbySuccess;
            FindLobbiesFailed -= joinLobbyFailed;
        }
    }
    private new void Start()
    {
        #region screen settings stuff
        setTargetFpsSlider.gameObject.SetActive(false);
        if (!PlayerPrefs.HasKey("isVsync"))
        {
            vsyncToggle.isOn = true;
            QualitySettings.vSyncCount = 1;
        }
        resDropdown.onValueChanged.AddListener(val => setRes(val));
        graphicDropdown.onValueChanged.AddListener(val => setGraphic(val));
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "hz";
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentIndex = i;
            }
        }
        resDropdown.AddOptions(options);
        _soundSlider.onValueChanged.AddListener(val => setAudio(val));
        fullscreenToggle.onValueChanged.AddListener(val => setFullscreenTick(val));
        closeMusicToggle.onValueChanged.AddListener(val => toggleAudioMenu(val));
        vsyncToggle.onValueChanged.AddListener(val => setVsync(val));
        setTargetFpsSlider.onValueChanged.AddListener(val => setTargetFps(val));
        #endregion
        if (PlayerPrefs.HasKey("masterAudio")) { AudioListener.volume = PlayerPrefs.GetFloat("masterAudio"); _soundSlider.value = PlayerPrefs.GetFloat("masterAudio"); }
        if (PlayerPrefs.HasKey("MainMenuMusic"))
        {
            int i = PlayerPrefs.GetInt("MainMenuMusic");
            if (i == 1) { mainMenuMusicSource.Pause(); closeMusicToggle.isOn = true; } else { mainMenuMusicSource.UnPause(); closeMusicToggle.isOn = false; }
        }
        if (PlayerPrefs.HasKey("SetGraphic"))
        {
            int index = PlayerPrefs.GetInt("SetGraphic");
            graphicDropdown.value = index;
        }
        if (PlayerPrefs.HasKey("resIndex"))
        {
            int index = PlayerPrefs.GetInt("resIndex");
            int getFullscreen = PlayerPrefs.GetInt("setFullscreenTick");
            bool isFull = getFullscreen == 1 ? true : false;
            if (index > resolutions.Length) return;
            resDropdown.value = index;
            resDropdown.RefreshShownValue();
        }
        else
        {
            resDropdown.value = currentIndex;
            resDropdown.RefreshShownValue();
        }
        if (PlayerPrefs.HasKey("setFullscreenTick"))
        {
            bool fullScreenBool = PlayerPrefs.GetInt("setFullscreenTick") == 1 ? true : false;
            Screen.fullScreen = fullScreenBool;
            fullscreenToggle.isOn = fullScreenBool;
        }
        if (PlayerPrefs.HasKey("isVsync"))
        {
            bool isVsyncBool = PlayerPrefs.GetInt("isVsync") == 1 ? true : false;
            vsyncToggle.isOn = isVsyncBool;
            setVsync(isVsyncBool);
        }
        if (PlayerPrefs.HasKey("targetFps"))
        {
            if (vsyncToggle.isOn) return;
            int targetFps = PlayerPrefs.GetInt("targetFps");
            setTargetFpsSlider.value = targetFps;
            setTargetFps(targetFps);
        }
    }
    public void setVsync(bool isSync)
    {
        if (isSync)
        {
            QualitySettings.vSyncCount = 1;
            setTargetFpsSlider.gameObject.SetActive(false);
            PlayerPrefs.SetInt("isVsync", 1);
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            setTargetFpsSlider.gameObject.SetActive(true);
            targetFpsGUI.text = $"Target Fps: {(int)setTargetFpsSlider.value}";
            Application.targetFrameRate = (int)setTargetFpsSlider.value;
            PlayerPrefs.SetInt("isVsync", 0);
        }
        PlayerPrefs.Save();
    }
    public void setTargetFps(float value)
    {
        if (value < 20) return;
        int realValue = (int)value;
        targetFpsGUI.text = $"Target Fps: {realValue}";
        Application.targetFrameRate = realValue;
        PlayerPrefs.SetInt("targetFps", realValue);
        PlayerPrefs.Save();
    }
    public void setRes(int index)
    {
        Resolution res = resolutions[index];
        int getFullscreen = PlayerPrefs.GetInt("setFullscreenTick");
        bool isFull = getFullscreen == 1 ? true : false;
        Screen.SetResolution(res.width, res.height, isFull, res.refreshRate);
        PlayerPrefs.SetInt("resIndex", index);
        PlayerPrefs.Save();
    }
    public void setAudio(float volume)
    {
        if (AudioListener.pause == true) AudioListener.pause = false;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("masterAudio", volume);
        PlayerPrefs.Save();
        //infoPiercer.audioFloat = volume;
    }
    public void setGraphic(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("SetGraphic", index);
        PlayerPrefs.Save();
    }
    public void setFullscreenTick(bool isScreenFull)
    {
        Screen.fullScreen = isScreenFull;
        PlayerPrefs.SetInt("setFullscreenTick", isScreenFull ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void toggleAudioMenu(bool toggleVal)
    {
        if (toggleVal) { mainMenuMusicSource.Pause(); PlayerPrefs.SetInt("MainMenuMusic", 1); } else { mainMenuMusicSource.UnPause(); PlayerPrefs.SetInt("MainMenuMusic", 0); }
        PlayerPrefs.Save();
    }
    public void switchMenuToRoom()
    {
        mainMenuCanvas.enabled = !mainMenuCanvas.enabled;
        roomCanvas.enabled = !roomCanvas.enabled;
    }
    public void leaveServer()
    {
        if (isEpicActive)
        {
            LeaveLobby();
            return;
        }
        if (NetworkClient.active && NetworkClient.isConnected)
        {
            if (isEpicActive) LeaveLobby();
            NetworkManager.singleton.StopHost();

        }
        else if (NetworkClient.isConnected)
        {
            if (isEpicActive) LeaveLobby();
            NetworkManager.singleton.StopClient();
        }
        MainMenuButton.isPanelActive = false;
        mainMenuCanvas.enabled = true;
        roomCanvas.enabled = false;
        //switchMenuToRoom();
    }
    public void onHostStopCall()
    {
        if (Application.loadedLevel != 0) return;
        MainMenuButton.isPanelActive = false;
        mainMenuCanvas.enabled = true;
        roomCanvas.enabled = false;
    }
    public void setupEpicLobby(string playerName)
    {
        isLobbyOwner = true;
        CreateLobby(4, LobbyPermissionLevel.Publicadvertised, false, new AttributeData[] { new AttributeData { Key = AttributeKeys[0], Value = playerName }, });
        EpicJoining.SetActive(true);
    }
    public void joinEpicLobby(string Id)
    {
        Debug.Log("Joining epic lobby with " + Id);
        JoinLobbyByID(Id);
        EpicJoining.SetActive(true);
    }

    //when the lobby is successfully created, start the host
    private void OnCreateLobbySuccess(List<Attribute> attributes)
    {
        NetworkManager.singleton.StartHost();
        isLobbyOwner = true;
        EpicJoining.SetActive(false);
        isJoining = false;
    }

    //when the user joined the lobby successfully, set network address and connect
    private void OnJoinLobbySuccess(List<Attribute> attributes)
    {

        NetworkManager netManager = NetworkManager.singleton;
        netManager.networkAddress = attributes.Find((x) => x.Data.Key == hostAddressKey).Data.Value.AsUtf8;
        netManager.StartClient();
        WaitUntilOfDmr.InvokeWithDelay(this, () =>
        {
            isJoining = false;
            EpicJoining.SetActive(false);
        }, () => NetworkClient.isConnecting == false);
    }
    private void OnLeaveLobbySuccess()
    {
        Debug.Log("Leave Lobby Success");
        NetworkManager netManager = NetworkManager.singleton;
        netManager.StopHost();
        netManager.StopClient();
    }
    private void lobbyCreationFailed(string errMessage)
    {
        NotificationCreator.instance.createNotification("Error", errMessage);
        isJoining = false;
        EpicJoining.SetActive(false);
    }
    private void joinLobbyFailed(string errMessage)
    {
        NotificationCreator.instance.createNotification("Error", errMessage);
        isJoining = false;
        EpicJoining.SetActive(false);
    }
    public void clickToCopyboard()
    {
        GUIUtility.systemCopyBuffer = GetCurrentLobbyId();
    }
    public void kickEpicPlayer(int index)
    {
        NetworkRoomPlayerRev roomPlayer = LobbySlotManager.instance.players[index].GetComponent<NetworkRoomPlayerRev>();
        if (!Server.EpicIdsWithConnectionId.ContainsKey(roomPlayer.connectionToClient.connectionId))
        {
            NotificationCreator.instance.createNotification("Error", "Cannot find target player epic id");
            return;
        }
        string keyId = Server.EpicIdsWithConnectionId[roomPlayer.connectionToClient.connectionId];
        ProductUserId userIdClass = ProductUserId.FromString(keyId);
        KickFromLobby(userIdClass, roomPlayer.netIdentity);
    }
}
