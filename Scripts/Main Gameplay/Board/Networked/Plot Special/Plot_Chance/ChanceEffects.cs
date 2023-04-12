using Mirror;

public class ChanceEffects : NetworkBehaviour
{
    public static ChanceEffects instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    public void PlayChanceEffect(uint PlayerId, int EffectId)
    {
        if (!isServer) return;
        NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(PlayerId).GetComponent<NetworkPlayerCON>();
        BoardPlayer boardPlayer = UtulitiesOfDmr.ReturnBoardPlayerById(PlayerId);
        //Go To Quicksand,
        //I should've used switch rather than else if 
        //I didn't expected to stack up this much
        if (EffectId == 1)
        {
            //10 is quicksand
            BoardMoveManager.instance.MovePlayerTo(10, boardPlayer);
            Quicksand.instance.RemovePlayerFromDiceDict(PlayerId);
        }
        else if (EffectId == 2)
        {
            //Move x forward
            int whereToLand = UtulitiesOfDmr.GiveMeRealRandomNumbers(1, 39);
            if (boardPlayer.currentLandIndex == whereToLand)
            {
                if (whereToLand == 39)
                {
                    BoardMoveManager.instance.MovePlayerTo(whereToLand - 1, boardPlayer);
                }
                else if (whereToLand == 1)
                {
                    BoardMoveManager.instance.MovePlayerTo(whereToLand + 1, boardPlayer);
                }
                else
                {
                    if (whereToLand + 1 == 10) Quicksand.instance.RemovePlayerFromDiceDict(PlayerId);
                    BoardMoveManager.instance.MovePlayerTo(whereToLand + 1, boardPlayer);
                }
            }
            else
            {
                if (whereToLand == 10) Quicksand.instance.RemovePlayerFromDiceDict(PlayerId);
                BoardMoveManager.instance.MovePlayerTo(whereToLand, boardPlayer);
            }
        }
        else if (EffectId == 3)
        {
            //Go To Start
            BoardMoveManager.instance.MovePlayerTo(0, boardPlayer);
        }
        else if (EffectId == 4)
        {
            //Go To Teleport
            BoardMoveManager.instance.MovePlayerTo(30, boardPlayer);
        }
        else if (EffectId == 5)
        {
            //Go To Tree
            BoardMoveManager.instance.MovePlayerTo(20, boardPlayer);
        }
        else if (EffectId == 6)
        {
            //You Won 300k Gold From Random Chest
            netPc.PlayerMoney += 300000;
        }
        else if (EffectId == 7)
        {
            //You Won 100k Gold From Random Chest
            netPc.PlayerMoney += 100000;
        }
        else if (EffectId == 8)
        {
            //You Lost 100K
            if (netPc.PlayerMoney >= 100000)
            {
                netPc.PlayerMoney -= 100000;
            }
            else
            {
                netPc.PlayerMoney -= netPc.PlayerMoney;
            }
        }
        else if (EffectId == 9)
        {
            //You Lost 300K
            if (netPc.PlayerMoney >= 300000)
            {
                netPc.PlayerMoney -= 300000;
            }
            else
            {
                netPc.PlayerMoney -= netPc.PlayerMoney;
            }
        }
    }
}
