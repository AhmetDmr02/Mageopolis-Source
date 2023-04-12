using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameConfigurator : MonoBehaviour
{
    [SerializeField] private Slider playerMoveSpeed, eachTourSalary;
    [SerializeField] private Toggle shouldPlayersEndTour, should3MonopolyWinGame, shouldSideMonopolyWinGame;
    [SerializeField] private TextMeshProUGUI startMoneyText;
    private void FixedUpdate()
    {
        if (BoardMoveManager.instance == null) return;
        playerMoveSpeed.value = ConfigPiercer.instance.playerSpeeds;
        shouldPlayersEndTour.isOn = ConfigPiercer.instance.shouldPlayersEnd;
        startMoneyText.text = $"Each Tour Salary: {ConfigPiercer.instance.startSalary}";
        eachTourSalary.value = ConfigPiercer.instance.startSalary;
        should3MonopolyWinGame.isOn = ConfigPiercer.instance.GameShouldEndWith3Monopoly;
        shouldSideMonopolyWinGame.isOn = ConfigPiercer.instance.GameShouldEndWithSideMonopoly;
    }
    private void Start()
    {
        playerMoveSpeed.onValueChanged.AddListener(val => playerMoveChanged(val));
        eachTourSalary.onValueChanged.AddListener(val => eachTourSalaryChanged(val));
        shouldPlayersEndTour.onValueChanged.AddListener(val => autoEndTourChanged(val));
        should3MonopolyWinGame.onValueChanged.AddListener(val => should3MonopolyWinGameChanged(val));
        shouldSideMonopolyWinGame.onValueChanged.AddListener(val => shouldSideMonopolyWinGameChanged(val));
    }
    private void playerMoveChanged(float value)
    {
        if (!BoardMoveManager.instance.isServer) return;
        ConfigPiercer.instance.playerSpeeds = value;
        BoardMoveManager.instance.speed = value;
    }
    private void eachTourSalaryChanged(float value)
    {
        if (!BoardMoveManager.instance.isServer) return;
        ConfigPiercer.instance.startSalary = (int)value;
        BoardMoveManager.instance.GetComponent<Plot_Start>().setSalary((int)value);
    }
    private void autoEndTourChanged(bool value)
    {
        if (!BoardMoveManager.instance.isServer) return;
        ConfigPiercer.instance.shouldPlayersEnd = value;
    }
    private void should3MonopolyWinGameChanged(bool value)
    {
        if (!BoardMoveManager.instance.isServer) return;
        ConfigPiercer.instance.GameShouldEndWith3Monopoly = value;
    }
    private void shouldSideMonopolyWinGameChanged(bool value)
    {
        if (!BoardMoveManager.instance.isServer) return;
        ConfigPiercer.instance.GameShouldEndWithSideMonopoly = value;
    }
}
