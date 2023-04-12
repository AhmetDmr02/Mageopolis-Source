using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class HitDetector : NetworkBehaviour
{
    [SerializeField]
    private hitGroup[] hitGroups;
    private void Start()
    {
        LandBuyManager.PlayerBoughtLand += checkForHits;
        PlotClass.LandWiped += checkForHits;
    }
    private void OnDestroy()
    {
        LandBuyManager.PlayerBoughtLand -= checkForHits;
        PlotClass.LandWiped -= checkForHits;
    }
    private void checkForHits(int plotIndex, uint playerId)
    {
        if (!isServer) return;
        if (playerId == 0) return;
        if (UtulitiesOfDmr.ReturnCorrespondPlayerById(playerId) == null) return;
        NetworkPlayerCON netPc = UtulitiesOfDmr.ReturnCorrespondPlayerById(playerId).GetComponent<NetworkPlayerCON>();
        hitGroup[] currentlyOwnedGroups = hitPlotsByPlayer(playerId);
        if (currentlyOwnedGroups.Length < 1)
        {
            //Currently have no hits 
            foreach (hitGroup groups in hitGroups)
            {
                bool doesPlayerHaveHit = isPlayerHit(groups, playerId);
                if (doesPlayerHaveHit)
                {
                    Debug.Log($"{netPc.PlayerName} owns {groups.hitPlotGroups[0].landPrices.landBiome} biome");
                    AnimatedTextCreator.instance.CreateAnimatedText($"{netPc.PlayerName} owns {groups.hitPlotGroups[0].landPrices.landBiome} biome", Color.cyan);
                    groups.hittedBy = playerId;
                    if (BoardMoveManager.instance.LandGameobject[groups.hitInterfaceIndex].GetComponent<IHitSpecial>() != null)
                        BoardMoveManager.instance.LandGameobject[groups.hitInterfaceIndex].GetComponent<IHitSpecial>().WhenHit(playerId);
                    else
                        Debug.LogWarning("Hit interface cannot called");
                }
            }
        }
        else
        {
            //First Check If Plot Destroyed
            foreach (hitGroup checkForUnhit in currentlyOwnedGroups)
            {
                bool doesPlayerHaveHit = isPlayerHit(checkForUnhit, playerId);
                if (!doesPlayerHaveHit)
                {
                    Debug.Log($"{netPc.PlayerName} no longer owns {checkForUnhit.hitPlotGroups[0].landPrices.landBiome} biome");
                    AnimatedTextCreator.instance.CreateAnimatedText($"{netPc.PlayerName} no longer owns {checkForUnhit.hitPlotGroups[0].landPrices.landBiome} biome", Color.red);
                    checkForUnhit.hittedBy = 0;
                    if (BoardMoveManager.instance.LandGameobject[checkForUnhit.hitInterfaceIndex].GetComponent<IHitSpecial>() != null)
                        BoardMoveManager.instance.LandGameobject[checkForUnhit.hitInterfaceIndex].GetComponent<IHitSpecial>().WhenUnhit(playerId);
                    else
                        Debug.LogWarning("Unhit interface cannot called");
                }
            }
            //Then Check If Player Owned New Plots
            foreach (hitGroup groups in hitGroups)
            {
                if (groups.hittedBy == playerId) continue;
                bool doesPlayerHaveHit = isPlayerHit(groups, playerId);
                if (doesPlayerHaveHit)
                {
                    Debug.Log($"{netPc.PlayerName} owns {groups.hitPlotGroups[0].landPrices.landBiome} biome");
                    AnimatedTextCreator.instance.CreateAnimatedText($"{netPc.PlayerName} owns {groups.hitPlotGroups[0].landPrices.landBiome} biome", Color.cyan);
                    groups.hittedBy = playerId;
                    if (BoardMoveManager.instance.LandGameobject[groups.hitInterfaceIndex].GetComponent<IHitSpecial>() != null)
                        BoardMoveManager.instance.LandGameobject[groups.hitInterfaceIndex].GetComponent<IHitSpecial>().WhenHit(playerId);
                    else
                        Debug.LogWarning("Hit interface cannot called");
                }
            }
        }
    }
    private bool isPlayerHit(hitGroup group, uint playerId)
    {
        if (!isServer) return false;
        bool returnBool = true;
        foreach (LandInstance landInstance in group.hitPlotGroups)
        {
            for (int i = 0; i < BoardMoveManager.instance.LandGameobject.Length; i++)
            {
                PlotClass plotClass = BoardMoveManager.instance.LandGameobject[i].GetComponent<PlotClass>();
                if (landInstance.landName == plotClass.landName)
                {
                    if (plotClass.ownedBy != playerId)
                    {
                        returnBool = false;
                        break;
                    }
                }
            }
        }
        return returnBool;
    }
    private hitGroup[] hitPlotsByPlayer(uint playerId)
    {
        if (!isServer) return null;
        List<hitGroup> returnGroups = new List<hitGroup>();
        foreach (hitGroup group in hitGroups)
        {
            if (group.hittedBy == playerId)
            {
                returnGroups.Add(group);
            }
        }
        return returnGroups.ToArray();
    }

    [System.Serializable]
    private class hitGroup
    {
        /// <summary>
        /// Interface will be called from that index when player hits
        /// Example of 1 = BoardMoveManager.instance.LandGameobject[1].GetComponent<IHitSpecial>();
        /// </summary>
        public int hitInterfaceIndex;
        public uint hittedBy;
        public LandInstance[] hitPlotGroups;
    }
}
