using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Reflection;

public static class UtulitiesOfDmr
{
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }
    public static void Shuffle<T>(this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T CopyInstance<T>(T original) where T : class
    {
        T copy = null;
        if (original != null)
        {
            copy = System.Activator.CreateInstance<T>();
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(original);
                field.SetValue(copy, value);
            }
        }
        return copy;
    }
    public static GameObject ReturnCorrespondPlayerById(uint ID)
    {
        GameObject[] go = MainPlayerRefrences.instance.PlayerObjects.ToArray();
        foreach (GameObject gameObj in go)
        {
            if (gameObj == null) continue;
            if (gameObj.GetComponent<NetworkIdentity>().netId == ID)
            {
                return gameObj;
            }
        }
        return null;
    }
    public static BoardPlayer ReturnBoardPlayerById(uint ID)
    {
        foreach (GameObject go in BoardMoveManager.instance.boardPlayers)
        {
            BoardPlayer boardPlayer = go.GetComponent<BoardPlayer>();
            if (boardPlayer.representedPlayerId == 0) continue;
            if (boardPlayer.representedPlayerId == ID)
            {
                return boardPlayer;
            }
        }
        return null;
    }
    public static List<PlotClass> ReturnPlotClassesForBoardPlayer(BoardPlayer boardPlayer, int forwardInt)
    {
        List<PlotClass> returnPlots = new List<PlotClass>();
        int currentIndex = boardPlayer.currentAnimationPlotIndex;
        int trackIndex = 0;
        //Adding current plot either for lerping later
        returnPlots.Add(BoardMoveManager.instance.LandGameobject[boardPlayer.currentAnimationPlotIndex].GetComponent<PlotClass>());
        for (int i = 0; i < forwardInt; i++)
        {
            int calculateIndex = currentIndex + (i + 1);
            //39 Is Length Of The Total Board Plots
            calculateIndex = calculateIndex > 39 ? calculateIndex % 40 : calculateIndex;
            GameObject currentLand = BoardMoveManager.instance.LandGameobject[calculateIndex];
            PlotClass currentPlot = currentLand.GetComponent<PlotClass>();
            returnPlots.Add(currentPlot);
            trackIndex = calculateIndex;
        }
        return returnPlots;
    }
    public static int ReturnIndexAfterBoardPlayerLands(int startIndex, int forwardInt)
    {
        int currentIndex = startIndex;
        int trackIndex = 0;
        //Adding current plot either for lerping later
        for (int i = 0; i < forwardInt; i++)
        {
            int calculateIndex = currentIndex + (i + 1);
            //39 Is Length Of The Total Board Plots
            calculateIndex = calculateIndex > 39 ? calculateIndex % 40 : calculateIndex;
            trackIndex = calculateIndex;
        }
        return trackIndex;
    }
    public static int ReturnLandIndexByReference(PlotClass plotClass)
    {
        for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
        {
            if (BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>() == plotClass)
            {
                return i;
            }
        }
        return -1;
    }
    public static PlotClass ReturnPlotClassByIndex(int plotIndex)
    {
        if (plotIndex < BoardMoveManager.instance.LandGameobject.Length)
        {
            if (BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>() != null)
                return BoardMoveManager.instance.LandGameobject[plotIndex].GetComponent<PlotClass>();
            else
                return null;
        }
        return null;
    }
    public static int GetCurrentPlotIncome(PlotClass _plotClass)
    {
        int totalSalary = 0;
        LandInstance landInstance = BoardMoveManager.instance.LandInstance[_plotClass.landIndex];
        totalSalary += landInstance.landPrices.landBaseSalary;
        for (int i = 0; i <= _plotClass.landCurrentUpgrade - 1; i++)
        {
            totalSalary += landInstance.landPrices.landUpgradeSalaries[i];
        }
        if (_plotClass.landPrices.isLandHit) totalSalary = (int)(totalSalary * _plotClass.landPrices.landHitPriceMultiplier);
        return totalSalary;
    }
    public static int GetVacuumPriceForPlayer(BoardPlayer boardPlayer)
    {
        int totalDebt = 0;
        GameObject[] plotClassObject = BoardMoveManager.instance.LandGameobject.ToArray();
        uint netId = boardPlayer.networkPlayerObject.GetComponent<NetworkIdentity>().netId;
        for (int i = 0; i < plotClassObject.Length; i++)
        {
            PlotClass plotClass_ = plotClassObject[i].GetComponent<PlotClass>();
            if (plotClass_.landKind != LandKind.Buyable) continue;
            if (plotClass_.ownedBy != netId) continue;
            int pureSalary = GetCurrentPlotIncome(plotClass_);
            int refinedSalary = (pureSalary / 100) * 15; //10 PERCENT
            totalDebt += refinedSalary;
        }
        return totalDebt;
    }
    public static List<PlotClass> ReturnOwnedByPlayerPlots(uint playerID)
    {
        List<PlotClass> playerOwnedClasses = new List<PlotClass>();
        foreach (GameObject go in BoardMoveManager.instance.LandGameobject)
        {
            if (go.GetComponent<PlotClass>() == null) continue;
            if (go.GetComponent<PlotClass>().landKind != LandKind.Buyable) continue;
            if (go.GetComponent<PlotClass>().ownedBy != playerID) continue;
            playerOwnedClasses.Add(go.GetComponent<PlotClass>());
        }
        return playerOwnedClasses;
    }
    public static int ReturnTotalWorthOfPlayerByID(uint playerID)
    {
        int totalPlayerWorth = 0;
        foreach (GameObject go in BoardMoveManager.instance.LandGameobject)
        {
            if (go.GetComponent<PlotClass>() == null) continue;
            if (go.GetComponent<PlotClass>().landKind != LandKind.Buyable) continue;
            if (go.GetComponent<PlotClass>().ownedBy != playerID) continue;
            totalPlayerWorth += calculateTotalPlotWorth(go.GetComponent<PlotClass>());
        }
        //Now Add Player Money On Top Of This
        BoardPlayer boardPlayer = ReturnBoardPlayerById(playerID);
        totalPlayerWorth += boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().PlayerMoney;
        return totalPlayerWorth;
    }
    public static int ReturnTotalWorthOfPlotClasses(PlotClass[] plotClasses)
    {
        int totalWorth = 0;
        foreach (PlotClass pc in plotClasses)
        {
            if (pc.landKind != LandKind.Buyable) continue;
            if (pc.ownedBy == 0) continue;
            totalWorth += calculateTotalPlotWorth(pc);
        }
        return totalWorth;
    }
    private static int calculateTotalPlotWorth(PlotClass plotClass_)
    {
        int totalWorth = 0;
        LandInstance landInstance = BoardMoveManager.instance.LandInstance[plotClass_.landIndex];
        totalWorth += landInstance.landPrices.landBuyPrice;
        for (int i = 0; i <= plotClass_.landCurrentUpgrade - 1; i++)
        {
            totalWorth += landInstance.landPrices.landUpgradePrices[i];
        }
        return totalWorth;
    }
    public static int[] ReturnOwnedLandsIndexesByPlayerID(uint playerID)
    {
        List<int> indexes = new List<int>();
        List<PlotClass> plotClasses = ReturnOwnedByPlayerPlots(playerID).ToList();
        foreach (PlotClass pc in plotClasses)
        {
            indexes.Add(pc.landIndex);
        }
        return indexes.ToArray();
    }
    public static Color32 ReturnPlayerColor(uint playerID)
    {
        if (ReturnCorrespondPlayerById(playerID).GetComponent<NetworkPlayerCON>() == null)
        {
            return Color.white;
        }
        else
        {
            return ReturnCorrespondPlayerById(playerID).GetComponent<NetworkPlayerCON>().playerColor;
        }
    }
    public static int ReturnWipeCostOfPlot(PlotClass plotClass_)
    {
        int i = ReturnTotalWorthOfPlotClasses(new PlotClass[] { plotClass_ });
        float totalWipeCost = i * plotClass_.landPrices.landDestroyMultiplier;
        return (int)totalWipeCost;
    }
    public static bool ReturnTrueIfCanPlayerWipeLands(NetworkPlayerCON netPC)
    {
        PlotClass[] ownedPlotClasses = ReturnOwnedByPlayerPlots(netPC.netId).ToArray();
        //Library Indexes
        int[] approvedIndexes = { 11, 12, 13 };
        foreach (PlotClass pc in ownedPlotClasses)
        {
            if (approvedIndexes.Contains(pc.landIndex))
            {
                return true;
            }
            else
            {
                continue;
            }
        }
        return false;
    }
    public static List<PlotClass> ReturnPlotClassesWithIntArray(int[] intArray)
    {
        List<PlotClass> myList = new List<PlotClass>();
        foreach (int i in intArray)
        {
            myList.Add(BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>());
        }
        return myList;
    }
    public static int GiveMeRealRandomNumbers(int min, int max)
    {
        if (min >= max)
        {
            throw new ArgumentException("Invalid range.");
        }

        byte[] data = new byte[4];
        int value;

        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            do
            {
                crypto.GetBytes(data);
                value = Math.Abs(BitConverter.ToInt32(data, 0));
            } while (value > max * (Int32.MaxValue / max));
        }

        return min + (value % (max - min + 1));
    }
}
