using UnityEngine;
using Mirror;
using static EndGameStats;
using System.Collections.Generic;
using System.Linq;

public class GameEnder : NetworkBehaviour
{
    public bool GameEnded;
    public static GameEnder instance;
    public Monopolies[] Monopolies;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        LandBuyManager.PlayerBoughtLand += checkForEnding;
    }
    public void checkForEnding(int i, uint ui)
    {
        if (!isServer) return;
        //This is test code have terrible performance will rewrite asap
        List<NetworkPlayerCON> players = new List<NetworkPlayerCON>();

        #region Monopoly Check
        if (ConfigPiercer.instance.GameShouldEndWith3Monopoly || ConfigPiercer.instance.GameShouldEndWith3Monopoly)
        {
            foreach (GameObject go in MainPlayerRefrences.instance.PlayerObjects)
            {
                if (go == null) continue;
                if (go.GetComponent<NetworkPlayerCON>() == null) continue;
                players.Add(go.GetComponent<NetworkPlayerCON>());
            }
            foreach (NetworkPlayerCON player in players)
            {
                int playerMonopolyCount = 0;
                foreach (Monopolies monopoly in Monopolies)
                {
                    List<PlotClass> plotClasses = new List<PlotClass>();
                    foreach (GameObject go in BoardMoveManager.instance.LandGameobject)
                    {
                        foreach (LandInstance instance in monopoly.Instances)
                        {
                            if (instance.landName != go.GetComponent<PlotClass>().landName) continue;
                            plotClasses.Add(go.GetComponent<PlotClass>());
                        }
                    }
                    bool monopolyBroken = false;
                    foreach (PlotClass subPlotClass in plotClasses)
                    {
                        if (subPlotClass.ownedBy != player.netId)
                        {
                            monopolyBroken = true;
                            break;
                        }
                    }
                    if (monopolyBroken)
                        continue;
                    else
                    {
                        if (monopoly.monopolyKind == MonopolyKind.SingleMonopoly)
                        {
                            if (ConfigPiercer.instance.GameShouldEndWith3Monopoly)
                            {
                                playerMonopolyCount += 1;
                                if (playerMonopolyCount == 3)
                                {
                                    //Game Ended 
                                    AnimatedTextCreator.instance.CreateAnimatedText($"{player.PlayerName} Won By Owning 3 Biomes!", player.playerColor);
                                    InvokerOfDmr.InvokeWithDelay(GameEnder.instance, GameEnder.instance.EndGame, 3);
                                    QueueManager.instance.GameDone = true;
                                    DiceManager.instance.cannotRollDice = true;
                                    QueueManager.instance.cannotEndTour = true;
                                    QueueTimer.instance.setDecreaseRate(0);
                                }
                            }
                        }
                        else
                        {
                            if (ConfigPiercer.instance.GameShouldEndWithSideMonopoly)
                            {
                                //Game Ended
                                AnimatedTextCreator.instance.CreateAnimatedText($"{player.PlayerName} Won By Owning One Side Of The Board!", player.playerColor);
                                InvokerOfDmr.InvokeWithDelay(GameEnder.instance, GameEnder.instance.EndGame, 3);
                                QueueManager.instance.GameDone = true;
                                DiceManager.instance.cannotRollDice = true;
                                QueueManager.instance.cannotEndTour = true;
                                QueueTimer.instance.setDecreaseRate(0);
                            }
                        }
                    }
                }
            }
        }
        #endregion
        if (!GameEnded && !QueueManager.instance.GameDone)
        {
            if (QueueManager.instance.queueList.Count == 1)
            {
                NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(QueueManager.instance.queueList[0]).GetComponent<NetworkPlayerCON>();
                AnimatedTextCreator.instance.CreateAnimatedText($"{netPc.PlayerName} Won!", netPc.playerColor);
                InvokerOfDmr.InvokeWithDelay(GameEnder.instance, GameEnder.instance.EndGame, 3);
                QueueManager.instance.GameDone = true;
            }
        }
    }
    [ClientRpc]
    public void EndGame()
    {
        if (GameEnded) return;
        GameEnded = true;
        List<PlayerEndStats> playerEndStats = new List<PlayerEndStats>();
        foreach (GameObject netPcGO in MainPlayerRefrences.instance.PlayerObjects)
        {
            NetworkPlayerCON netPc = netPcGO.GetComponent<NetworkPlayerCON>();
            PlayerEndStats playerStats = new PlayerEndStats();
            playerStats.PlayerName = netPc.PlayerName;
            playerStats.PlayerMoney = UtulitiesOfDmr.ReturnTotalWorthOfPlayerByID(netPc.netId);
            playerStats.PlayerColor = netPc.playerColor;
            playerEndStats.Add(playerStats);
        }
        BoardRefrenceHolder.instance.GameEndStats.InitEndState(playerEndStats.ToArray());
    }
}
[System.Serializable]
public class Monopolies
{
    public LandInstance[] Instances;
    public MonopolyKind monopolyKind;
}
public enum MonopolyKind
{
    SingleMonopoly,
    SideMonopoly
}
