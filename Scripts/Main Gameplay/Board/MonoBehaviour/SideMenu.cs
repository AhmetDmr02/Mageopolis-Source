using System.Linq;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class SideMenu : MonoBehaviour
{
    [SerializeField] private Toggle playerStatsAlwaysOn;
    [SerializeField] private Toggle perspectiveCam;
    [SerializeField] private Toggle overlayPlayerStats;
    [SerializeField] private Toggle disableMusic;
    [SerializeField] private Slider setCamSensX;
    [SerializeField] private Slider setCamSensY;
    [SerializeField] private Slider setScrollSens;
    [SerializeField] private Slider setPlayerStatsScale;
    [SerializeField] public TextMeshProUGUI buttonText;
    [SerializeField] private GameObject sideMenu;
    private bool sideMenuPanelOpen;

    private void Start()
    {
        setPlayerStatsScale.onValueChanged.AddListener(val => setPlayerStatsScaleUI(val));
        setScrollSens.onValueChanged.AddListener(val => setScrollSensivity(val));
        setCamSensX.onValueChanged.AddListener(val => setHorizontalSens(val));
        setCamSensY.onValueChanged.AddListener(val => setVerticalSens(val));
        playerStatsAlwaysOn.onValueChanged.AddListener(val => setPlayerStatsAlwaysOn(val));
        perspectiveCam.onValueChanged.AddListener(val => setPerspectiveCam(val));
        overlayPlayerStats.onValueChanged.AddListener(val => setOverlayPlayerStats(val));
        disableMusic.onValueChanged.AddListener(val => setDisableMusic(val));
        checkPlayerPrefs();
    }
    private void Update()
    {
        checkForMenuToggle();
    }
    private void checkPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("setPerspectiveCam"))
        {
            int savedVar = PlayerPrefs.GetInt("setPerspectiveCam");
            bool savedBool = savedVar == 1 ? true : false;
            perspectiveCam.isOn = savedBool;
            setPerspectiveCam(savedBool);
        }
        if (PlayerPrefs.HasKey("setPlayerStatsAlwaysOn"))
        {
            int savedVar = PlayerPrefs.GetInt("setPlayerStatsAlwaysOn");
            bool savedBool = savedVar == 1 ? true : false;
            playerStatsAlwaysOn.isOn = savedBool;
            setPlayerStatsAlwaysOn(savedBool);
        }
        if (PlayerPrefs.HasKey("setVerticalSens"))
        {
            float savedVar = PlayerPrefs.GetFloat("setVerticalSens");
            setCamSensY.value = savedVar;
            setVerticalSens(savedVar);
        }
        if (PlayerPrefs.HasKey("setHorizontalSens"))
        {
            float savedVar = PlayerPrefs.GetFloat("setHorizontalSens");
            setCamSensX.value = savedVar;
            setHorizontalSens(savedVar);
        }
        if (PlayerPrefs.HasKey("setScrollSensivity"))
        {
            float savedVar = PlayerPrefs.GetFloat("setScrollSensivity");
            setScrollSens.value = savedVar;
            setScrollSensivity(savedVar);
        }
        if (PlayerPrefs.HasKey("setPlayerStatsScaleUI"))
        {
            float savedVar = PlayerPrefs.GetFloat("setPlayerStatsScaleUI");
            setPlayerStatsScale.value = savedVar;
            setPlayerStatsScaleUI(savedVar);
        }
        if (PlayerPrefs.HasKey("setMusic"))
        {
            int savedVar = PlayerPrefs.GetInt("setMusic");
            bool savedBool = savedVar == 1 ? true : false;
            disableMusic.isOn = savedBool;
            setDisableMusic(savedBool);
        }
    }
    private void setOverlayPlayerStats(bool value)
    {
        if (PlayerStatsGUI.instance == null) return;
        GameObject[] go = PlayerStatsGUI.instance.PlayerStatObject.ToArray();
        foreach (GameObject go2 in go)
        {
            go2.SetActive(value);
        }
    }
    private void setDisableMusic(bool value)
    {
        //if (SoundEffectManager.instance == null) return;
        if (!value)
        {
            SoundEffectManager.instance.MainGameMusic.Play();
            SoundEffectManager.instance.MainGameMusic.UnPause();
        }
        else
        {
            SoundEffectManager.instance.MainGameMusic.Stop();
        }

        int saveInt = value ? 1 : 0;
        PlayerPrefs.SetInt("setMusic", saveInt);
        PlayerPrefs.Save();
    }
    private void setPerspectiveCam(bool value)
    {
        if (BoardRefrenceHolder.instance == null) return;
        BoardRefrenceHolder.instance.cameraController.switchPerspectiveMode(!value);
        int saveInt = value ? 1 : 0;
        PlayerPrefs.SetInt("setPerspectiveCam", saveInt);
        PlayerPrefs.Save();
    }
    private void setPlayerStatsAlwaysOn(bool value)
    {
        if (BoardMoveManager.instance == null) return;
        foreach (GameObject bp in BoardMoveManager.instance.boardPlayers)
        {
            BoardPlayer boardPlayer = bp.GetComponent<BoardPlayer>();
            boardPlayer.togglePlayerNames = value;
            //GameObject Doesn't Matter
            boardPlayer.checkTogglePlayerNames(this.gameObject);
        }
        int saveInt = value ? 1 : 0;
        PlayerPrefs.SetInt("setPlayerStatsAlwaysOn", saveInt);
        PlayerPrefs.Save();
    }
    private void setVerticalSens(float value)
    {
        if (BoardRefrenceHolder.instance == null) return;
        BoardRefrenceHolder.instance.cameraController.setYSpeed(value);
        PlayerPrefs.SetFloat("setVerticalSens", value);
        PlayerPrefs.Save();
    }
    private void setHorizontalSens(float value)
    {
        if (BoardRefrenceHolder.instance == null) return;
        BoardRefrenceHolder.instance.cameraController.setXSpeed(value);
        PlayerPrefs.SetFloat("setHorizontalSens", value);
        PlayerPrefs.Save();
    }
    private void setScrollSensivity(float value)
    {
        if (BoardRefrenceHolder.instance == null) return;
        BoardRefrenceHolder.instance.cameraController.setScrollSpeed(value);
        PlayerPrefs.SetFloat("setScrollSensivity", value);
        PlayerPrefs.Save();
    }
    private void setPlayerStatsScaleUI(float value)
    {
        if (BoardMoveManager.instance == null) return;
        foreach (GameObject bp in BoardMoveManager.instance.boardPlayers)
        {
            bp.transform.GetChild(3).GetChild(0).transform.localScale = new Vector3(value, value, value);
            Vector3 posBP = bp.transform.GetChild(3).GetChild(0).transform.localPosition;
            bp.transform.GetChild(3).GetChild(0).transform.localPosition = new Vector3(posBP.x, 0, posBP.z);
            bp.transform.GetChild(3).GetChild(0).transform.localPosition = new Vector3(posBP.x, 10f + value, posBP.z);
        }
        PlayerPrefs.SetFloat("setPlayerStatsScaleUI", value);
        PlayerPrefs.Save();
    }
    private void checkForMenuToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            buttonText.color = Color.black;
            buttonText.text = "Return To Main Menu";
            WarningInt = 0;
            if (sideMenuPanelOpen)
                sideMenu.SetActive(false);
            else
                sideMenu.SetActive(true);
            sideMenuPanelOpen = !sideMenuPanelOpen;
        }
    }
    int WarningInt;
    public void returnToMainMenu()
    {
        if (WarningInt == 0)
        {
            buttonText.text = "Are U Sure?";
            WarningInt += 1;
        }
        else if (WarningInt == 1)
        {
            buttonText.color = Color.red;
            buttonText.text = "Do You Really Wanna Leave?";
            WarningInt += 1;
        }
        else if (WarningInt == 2)
        {
            if (NetworkClient.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();

            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
        }
    }
    public void quitPanel()
    {
        sideMenuPanelOpen = false;
        sideMenu.SetActive(false);
        buttonText.color = Color.black;
        buttonText.text = "Return To Main Menu";
        WarningInt = 0;
    }
}
