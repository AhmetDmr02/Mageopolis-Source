using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BoardMoveManager : NetworkBehaviour
{
    [SerializeField] private LandInstance[] landInstanceByOrder;
    [SerializeField] private GameObject[] landGameobjectByOrder;

    public GameObject[] LandGameobject => landGameobjectByOrder;
    public LandInstance[] LandInstance => landInstanceByOrder;

    //Normally This shouldn't be here but mirror wants to keep in here for some reason?
    public GameObject Rabu, Zabu, Kabu, Ububu;

    public static BoardMoveManager instance;
    public SyncList<GameObject> boardPlayers = new SyncList<GameObject>();
    public static event Action CloseAllQueueIndicators;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    public void setupBoard()
    {
        Debug.Log("Called Setup Board");
        landGameobjectByOrder = BoardRefrenceHolder.instance.plotsObjectsWithOrder.ToArray();
        //Assigning Plots
        for (int i = 0; i < landGameobjectByOrder.Length; i++)
        {
            PlotClass plotClass;
            plotClass = landGameobjectByOrder[i].AddComponent<PlotClass>();
            plotClass.landKind = landInstanceByOrder[i].landKind;
            plotClass.landName = landInstanceByOrder[i].landName;
            plotClass.landPrices = UtulitiesOfDmr.CopyInstance(landInstanceByOrder[i].landPrices);
            plotClass.landDescription = landInstanceByOrder[i].landDescription;
            plotClass.landColor = landInstanceByOrder[i].landThemeColor;
            plotClass.landBiomeDesc = landInstanceByOrder[i].biomeDescription;
            plotClass.ownedBy = 0;
            plotClass.landIndex = i;
            plotClass.bounceY = landInstanceByOrder[i].CustomBounce ? landInstanceByOrder[i].landBounceY : landGameobjectByOrder[i].transform.localPosition.y - 1;
            //Getting center of object
            Renderer cache = landGameobjectByOrder[i].GetComponent<Renderer>();
            plotClass.slidePos = new Vector3(cache.bounds.center.x, cache.bounds.center.y + 2.2f, cache.bounds.center.z);
            //This place is little bit messy but transforms are the same across all plots
            Transform OurChildObject = landGameobjectByOrder[i].transform.GetChild(0);
            List<Vector3> playerSitLocations = new List<Vector3>();
            playerSitLocations.Add(OurChildObject.GetChild(3).position);
            playerSitLocations.Add(OurChildObject.GetChild(5).position);
            playerSitLocations.Add(OurChildObject.GetChild(4).position);
            playerSitLocations.Add(OurChildObject.GetChild(2).position);
            plotClass.playerSitPositions = playerSitLocations.ToArray();
            if (landInstanceByOrder[i].landKind == LandKind.Buyable)
                plotClass.monolithRuneParent = landGameobjectByOrder[i].transform.GetChild(0).GetChild(1).gameObject;
        }
        //Creating Players
        if (!isServer) return;
        for (int i = 0; i < MainPlayerRefrences.instance.PlayerObjects.Count; i++)
        {
            NetworkPlayerCON networkPlayer = MainPlayerRefrences.instance.PlayerObjects[i].GetComponent<NetworkPlayerCON>();
            GameObject spawnObject = null;
            switch (networkPlayer.playerModel)
            {
                case NetworkPlayerCON.PlayerModel.Zabu:
                    spawnObject = Zabu;
                    break;
                case NetworkPlayerCON.PlayerModel.Kabu:
                    spawnObject = Kabu;
                    break;
                case NetworkPlayerCON.PlayerModel.Rabu:
                    spawnObject = Rabu;
                    break;
                case NetworkPlayerCON.PlayerModel.Ububu:
                    spawnObject = Ububu;
                    break;
            }
            GameObject spawnedObject = Instantiate(spawnObject);
            NetworkServer.Spawn(spawnedObject, this.gameObject);
            BoardPlayer BP = spawnedObject.GetComponent<BoardPlayer>();
            BP.RpcSetUpObjectProperties(networkPlayer.gameObject, networkPlayer.playerColor, networkPlayer.netId);
            boardPlayers.Add(spawnedObject);
            spawnedObject.transform.position = landGameobjectByOrder[0].GetComponent<PlotClass>().playerSitPositions[i];
            BP.currentLandIndex = 0;
            BP.currentAnimationPlotIndex = 0;
            RpcSetupBoard(spawnedObject, landGameobjectByOrder[0].GetComponent<PlotClass>().playerSitPositions[i]);
        }
        //Assign First Queue
        //QueueManager.instance.CmdInitFirstQueue();
        QueueManager.instance.StartCoroutine(QueueManager.instance.InitFirstQueue(5f));
    }

    #region Rpcs
    [ClientRpc]
    public void RpcSetupBoard(GameObject spawnedObject, Vector3 desiredPos)
    {
        spawnedObject.transform.position = desiredPos;
    }
    #endregion
    //Getting how many player in plot so we can set their positions truly
    public List<GameObject> returnPuppetListFromLandIndex(int index)
    {
        List<GameObject> puppets = new List<GameObject>();
        foreach (GameObject go in boardPlayers)
        {
            if (go.GetComponent<BoardPlayer>().currentLandIndex == index)
            {
                puppets.Add(go);
            }
        }
        return puppets;
    }


    #region Board Player Moving Part
    [Space(15)]
    [Header("Move Stats")]
    private Vector3 startPosition;
    private Vector3 endPosition;
    public float speed = 3.5f;
    [SerializeField] private float bumpHeight = 5f;
    [SerializeField] private float progress = 2.0f;
    private Transform currentBoardPlayer;
    private bool waitForMove;
    public static event Action<PlotClass, BoardPlayer> onMove;
    public static event Action<PlotClass, BoardPlayer> onMoveDone;
    private Quicksand quicksand;
    [SerializeField] private List<MoveOrders> moveQueue = new List<MoveOrders>();
    private void Start()
    {
        onMove += onLandPass;
        onMoveDone += onLandStay;
        DiceManager.diceLanded += onDiceLanded;
        DiceManager.diceFired += preventFromDesync;
        progress = 25;
        quicksand = this.gameObject.GetComponent<Quicksand>();
    }
    void Update()
    {
        //Bounce Animation
        if (progress < 1 && MainGameManager.instance.serverGameState == MainGameManager.ServerGameState.Ingame)
        {
            progress += Time.deltaTime * speed;
            float t = Mathf.PingPong(progress, 1);
            float yBump = Mathf.Sin(Mathf.PingPong(progress, 1) * Mathf.PI) * bumpHeight;
            Vector3 target = new Vector3(endPosition.x, startPosition.y + yBump, endPosition.z);
            currentBoardPlayer.position = Vector3.Lerp(startPosition, target, t);
        }
    }
    private void FixedUpdate()
    {
        if (moveQueue.Count > 0)
        {
            if (moveActionBusy) return;
            moveActionBusy = true;
            StartCoroutine(moveBoardPlayer(moveQueue[0].plotClasses.ToList(), moveQueue[0].player.transform, moveQueue[0].diceOne, moveQueue[0].diceTwo, moveQueue[0].landmarkPosition));
            moveQueue.RemoveAt(0);
        }
    }
    private void OnDestroy()
    {
        onMove -= onLandPass;
        onMoveDone -= onLandStay;
        DiceManager.diceLanded -= onDiceLanded;
        DiceManager.diceFired -= preventFromDesync;
    }
    bool moveActionBusy = false;
    private IEnumerator moveBoardPlayer(List<PlotClass> plotsToGo, Transform playerTransform, int diceOne, int diceTwo, int landmarkPosition)
    {
        moveActionBusy = true;
        CloseAllQueueIndicators?.Invoke();
        EffectManager.instance.createLandMarkAt(landmarkPosition);

        currentBoardPlayer = playerTransform;
        List<PlotClass> plotsToProceed = plotsToGo.ToList();
        bool isItLast = false;
        for (int i = 0; i < plotsToGo.Count; i++)
        {
            if (i + 2 >= plotsToGo.Count) isItLast = true;
            if (!isItLast)
            {
                StartCoroutine(moveAction(plotsToGo[i + 1], plotsToGo[i].slidePos, plotsToGo[i + 1].slidePos, false, playerTransform, diceOne, diceTwo));
                waitForMove = true;
                yield return new WaitUntil(() => !waitForMove);
            }
            else
            {
                StartCoroutine(moveAction(plotsToGo[i + 1], plotsToGo[i].slidePos, plotsToGo[i + 1].slidePos, true, playerTransform, diceOne, diceTwo));
                waitForMove = false;
                yield break;
            }
        }
    }
    [ClientRpc]
    public void ReSync()
    {
        foreach (GameObject go in boardPlayers)
        {
            BoardPlayer boardPlayer = go.GetComponent<BoardPlayer>();
            boardPlayer.currentAnimationPlotIndex = boardPlayer.currentLandIndex;
        }
    }
    private void preventFromDesync(int i, int i2, BoardPlayer boardPlayer)
    {
        //If somehow manual move function called right before dice land it causes desync
        boardPlayer.playerFinishedTravel = false;
        boardPlayer.currentLandIndexOutdated = true;
    }

    private IEnumerator moveAction(PlotClass targetPlotClass, Vector3 locationA, Vector3 locationB, bool isThisLast, Transform boardPlayer, int diceOne, int diceTwo)
    {
        startPosition = locationA;
        endPosition = locationB;
        progress = 0;
        yield return new WaitUntil(() => progress >= 1);
        if (isThisLast)
        {
            BoardPlayer bp = boardPlayer.GetComponent<BoardPlayer>();
            boardPlayer.GetComponent<BoardPlayer>().currentAnimationPlotIndex = UtulitiesOfDmr.ReturnLandIndexByReference(targetPlotClass);
            if (isServer) bp.playerFinishedTravel = true;
            if (isServer) clientRpcCheckButton(targetPlotClass.landIndex, bp.representedPlayerId);
            onMoveDone?.Invoke(targetPlotClass, bp);
            targetPlotClass.GetComponent<ILandPress>().OnLand(boardPlayer);
            moveActionBusy = false;
            if (!isServer) yield break;
            resumeTimers();
        }
        else
        {
            waitForMove = false;
            BoardPlayer bp = boardPlayer.GetComponent<BoardPlayer>();
            onMove?.Invoke(targetPlotClass, bp);
            targetPlotClass.GetComponent<ILandPress>().OnLandPressed(boardPlayer);
        }
    }
    private void onLandPass(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        //Debug.Log("Player Passed At: " + plotClass.landName);
    }
    private void onLandStay(PlotClass plotClass, BoardPlayer boardPlayer)
    {
        //Debug.Log("Player Stopped At: " + plotClass.landName);
    }
    [ClientRpc]
    private void onDiceLandedRpc(int diceOne, int diceTwo, BoardPlayer boardPlayer)
    {
        if (isServer) return;
        if (boardPlayer == null) return;
        List<PlotClass> plotClasses = UtulitiesOfDmr.ReturnPlotClassesForBoardPlayer(boardPlayer, (diceOne + diceTwo));
        //This sometimes causes minor visual desync on lagged clients which is not really big of a problem because server will sync it on next queue
        //But lets add better way of moving players without caring about latency too much
        //StartCoroutine(moveBoardPlayer(plotClasses, boardPlayer.transform, diceOne, diceTwo));
        int landMarkposition = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentAnimationPlotIndex, diceOne + diceTwo);

        addMoveQueue(plotClasses, boardPlayer, diceOne, diceTwo, landMarkposition);
    }

    private void onDiceLanded(int diceOne, int diceTwo, BoardPlayer boardPlayer)
    {
        if (!isServer) return;
        List<PlotClass> plotClasses = UtulitiesOfDmr.ReturnPlotClassesForBoardPlayer(boardPlayer, (diceOne + diceTwo));
        pauseTimers();
        uint currentPlayerId = boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId;
        if (quicksand.TrappedPlayers.Contains(currentPlayerId))
        {
            if (diceOne == diceTwo)
            {
                quicksand.TrappedPlayers.Remove(currentPlayerId);
                quicksand.ToggleQuicksandInterface(false);
                #region Move Command
                boardPlayer.currentLandIndex = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentLandIndex, diceOne + diceTwo);
                DiceManager.instance.currentQueueAlreadyUsedDice = true;

                //This sometimes causes minor visual desync on lagged clients which is not really big of a problem because server will sync it on next queue
                //But lets add better way of moving players without caring about latency too much
                //StartCoroutine(moveBoardPlayer(plotClasses, boardPlayer.transform, diceOne, diceTwo));
                int landMarkposition = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentAnimationPlotIndex, diceOne + diceTwo);

                addMoveQueue(plotClasses, boardPlayer, diceOne, diceTwo, landMarkposition);
                onDiceLandedRpc(diceOne, diceTwo, boardPlayer);
                //////////////////////////////////////////////////////////
                #endregion
            }
            else
            {
                boardPlayer.currentLandIndexOutdated = false;
                boardPlayer.SetFalseOutdatedBool();
                quicksand.AdjustReleaseDict(currentPlayerId);
                boardPlayer.playerFinishedTravel = true;
                resumeTimers();
                QueueManager.instance.GetServerNextQueue(1, 2);
            }
        }
        else
        {
            bool isQuicksandAprroved = quicksand.CheckAndAdjustForQuicksand(diceOne, diceTwo, boardPlayer);
            if (isQuicksandAprroved)
            {
                //We need to do this before calling manual moving function
                boardPlayer.playerFinishedTravel = true;
                MovePlayerTo(10, boardPlayer);
                //Closing All Queue Indicatiors Number Doesn't Matter If We Close It All
                CloseAllQueueIndicators?.Invoke();
            }
            else
            {
                boardPlayer.currentLandIndex = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentLandIndex, diceOne + diceTwo);
                DiceManager.instance.currentQueueAlreadyUsedDice = true;

                //This sometimes causes minor visual desync on lagged clients which is not really big of a problem because server will sync it on next queue
                //But lets add better way of moving players without caring about latency too much
                //StartCoroutine(moveBoardPlayer(plotClasses, boardPlayer.transform, diceOne, diceTwo));
                int landMarkposition = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentAnimationPlotIndex, diceOne + diceTwo);

                addMoveQueue(plotClasses, boardPlayer, diceOne, diceTwo, landMarkposition);
                onDiceLandedRpc(diceOne, diceTwo, boardPlayer);
                //////////////////////////////////////////////////////////
            }
        }
    }
    [ClientRpc]
    private void onServerManualMoveCalled(int landIndex, BoardPlayer boardPlayer, int oldCurrentIndex)
    {
        if (landIndex < 0) return;
        if (landIndex > 39) return; //Length of the board
        if (QueueManager.instance.CurrentQueue != boardPlayer.networkPlayerObject.GetComponent<NetworkPlayerCON>().netId) return;
        boardPlayer.currentAnimationPlotIndex = oldCurrentIndex;
        int distance = (landIndex - oldCurrentIndex + 40) % 40;
        List<PlotClass> plotClasses = UtulitiesOfDmr.ReturnPlotClassesForBoardPlayer(boardPlayer, distance);
        int landMarkposition = UtulitiesOfDmr.ReturnIndexAfterBoardPlayerLands(boardPlayer.currentAnimationPlotIndex, distance);
        //This sometimes causes minor visual desync on lagged clients which is not really big of a problem because server will sync it on next queue
        //But lets add better way of moving players without caring about latency too much
        //StartCoroutine(moveBoardPlayer(plotClasses, boardPlayer.transform, 0, 0)); //dice numbers doesn't matter

        //New Implementation:
        addMoveQueue(plotClasses, boardPlayer, 0, 0, landMarkposition);
    }
    public void MovePlayerTo(int landIndex, BoardPlayer boardPlayer)
    {
        if (!isServer) return;
        if (landIndex < 0) return;
        if (landIndex > 39) return; //Length of the board
        //Checking if dice is rolled 
        if (!DiceManager.instance.spamPreventer) return;
        if (!boardPlayer.playerFinishedTravel) return;
        if (landIndex == boardPlayer.currentLandIndex) return;
        boardPlayer.playerFinishedTravel = false;
        int oldCurrentIndex = boardPlayer.currentLandIndex;
        DiceManager.instance.currentQueueAlreadyUsedDice = true;
        boardPlayer.currentLandIndex = landIndex;
        pauseTimers();
        onServerManualMoveCalled(landIndex, boardPlayer, oldCurrentIndex);
    }
    private void addMoveQueue(List<PlotClass> plotClasses, BoardPlayer player, int diceOne, int diceTwo, int landmarkPosition)
    {
        MoveOrders moveOrder = new MoveOrders();
        moveOrder.plotClasses = plotClasses.ToArray();
        moveOrder.player = player.transform;
        moveOrder.diceOne = diceOne;
        moveOrder.diceTwo = diceTwo;
        moveOrder.landmarkPosition = landmarkPosition;
        moveQueue.Add(moveOrder);
    }
    #endregion
    private void pauseTimers()
    {
        DiceTimer.instance.setDecreaseRate(0);
        QueueTimer.instance.setDecreaseRate(0);
    }
    private void resumeTimers()
    {
        DiceTimer.instance.setDecreaseRate(1);
        QueueTimer.instance.setDecreaseRate(1);
    }
    [ClientRpc]
    private void clientRpcCheckButton(int plotIndex, uint boardPlayerId)
    {
        if (boardPlayerId != NetworkPlayerCON.localPlayerCON.netId) return;
        PlotClass plotClass = UtulitiesOfDmr.ReturnPlotClassByIndex(plotIndex);
        BoardPlayer boardPlayerIndex = UtulitiesOfDmr.ReturnBoardPlayerById(boardPlayerId);
        DiceParticlesOpener.CheckParticles(boardPlayerId);
        EndTourButton.CheckButton(plotClass, boardPlayerIndex);
    }
}

[System.Serializable]
public class PlotClass : MonoBehaviour, ILandPress
{
    [Header("Main Stats")]
    public LandKind landKind;
    public string landName;
    public string landDescription;
    public string landBiomeDesc;
    public LandPrices landPrices;
    [Header("Visual Stuff")]
    [HideInInspector] public Vector3 slidePos;
    public Vector3[] playerSitPositions;
    [HideInInspector] public GameObject monolithRuneParent;
    [HideInInspector] public TextMeshProUGUI plotText;
    public Color landColor;
    [Header("Default Visuals")]
    [HideInInspector] public string defaultString;
    [Header("Misc Script Stuff")]
    public uint ownedBy;
    public IHitSpecial special;
    public int landIndex;
    public int landCurrentUpgrade;
    public float bounceY;
    [HideInInspector] public bool Highlighted;
    public bool isItProtected;

    private float fixedPositionY;
    private float lerpSpeed = 5f;
    private Vector3 desiredPos;
    private Transform cacheT;

    /// <summary>
    /// uint: Owner Before Plot Wipe
    /// int: Wiped Plot Index
    /// </summary>
    public static event Action<int, uint> LandWiped;
    public static event Action<int> LandBarrierWiped;

    public static event Action<int, uint> PlayerOwnershipChanged;

    private void Start()
    {
        Vector3 cache = this.transform.position;
        fixedPositionY = cache.y;
        cacheT = this.transform;
        desiredPos = new Vector3(cache.x, fixedPositionY, cache.z);
        AssignPlotText(transform.GetChild(0).GetChild(0));
        defaultString = plotText.text;
    }
    void FixedUpdate()
    {
        if (cacheT.position.y < fixedPositionY)
            cacheT.position = Vector3.Lerp(cacheT.position, desiredPos, lerpSpeed * Time.deltaTime);
        else if (cacheT.position.y > fixedPositionY)
            cacheT.position = Vector3.Lerp(cacheT.position, new Vector3(desiredPos.x, fixedPositionY, desiredPos.z), lerpSpeed * Time.deltaTime);
    }
    public void changeFixedY(float Y, bool Add)
    {
        fixedPositionY = Add ? fixedPositionY += Y : fixedPositionY = Y;
        desiredPos.y = Y;
    }
    public float returnFixedY()
    {
        return fixedPositionY;
    }
    public void OnLandPressed(Transform boardPlayer)
    {
        this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, bounceY, this.gameObject.transform.localPosition.z);
        AudioClip bounceSound = BoardMoveManager.instance.LandInstance[landIndex].stepAudio == null ? SoundEffectManager.instance.BounceDefault : BoardMoveManager.instance.LandInstance[landIndex].stepAudio;
        SoundEffectManager.instance.CreateDummyAudioAt(bounceSound, slidePos, 0.95f, 1.05f, 0.9f, 0.9f);
        //Animation Play Part
        if (this.gameObject.GetComponent<ILandAnimations>() != null) this.gameObject.GetComponent<ILandAnimations>().playBounceAnimation();
    }

    public void OnLand(Transform boardPlayer)
    {
        //Bounce Effect 
        this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, bounceY, this.gameObject.transform.localPosition.z);

        //Sound Effect
        if (boardPlayer.GetComponent<BoardPlayer>().networkPlayerObject == null)
        {
            Debug.Log("Player is disconnected");
            AudioClip bounceSound = BoardMoveManager.instance.LandInstance[landIndex].stepAudio == null ? SoundEffectManager.instance.BounceDefault : BoardMoveManager.instance.LandInstance[landIndex].stepAudio;
            SoundEffectManager.instance.CreateDummyAudioAt(bounceSound, slidePos, 0.95f, 1.05f, 0.9f, 0.9f);
            return;
        }
        if (NetworkPlayerCON.localPlayerCON.GetComponent<NetworkIdentity>().netId == boardPlayer.GetComponent<BoardPlayer>().networkPlayerObject.GetComponent<NetworkIdentity>().netId)
        {
            if (BoardMoveManager.instance.LandInstance[landIndex].landAudio != null)
                SoundEffectManager.instance.CreateDummyAudioAt(BoardMoveManager.instance.LandInstance[landIndex].landAudio, slidePos, 0.95f, 1.05f, BoardMoveManager.instance.LandInstance[landIndex].landAudioFloat, 0.1f);
            AudioClip bounceSound = BoardMoveManager.instance.LandInstance[landIndex].stepAudio == null ? SoundEffectManager.instance.BounceDefault : BoardMoveManager.instance.LandInstance[landIndex].stepAudio;
            SoundEffectManager.instance.CreateDummyAudioAt(bounceSound, slidePos, 0.95f, 1.05f, 0.9f, 0.9f);
        }
        else
        {
            AudioClip bounceSound = BoardMoveManager.instance.LandInstance[landIndex].stepAudio == null ? SoundEffectManager.instance.BounceDefault : BoardMoveManager.instance.LandInstance[landIndex].stepAudio;
            SoundEffectManager.instance.CreateDummyAudioAt(bounceSound, slidePos, 0.95f, 1.05f, 0.9f, 0.9f);
        }

        //Animation Play Part
        if (this.gameObject.GetComponent<ILandAnimations>() != null) this.gameObject.GetComponent<ILandAnimations>().playBounceAnimation();
        //Positioning Of Players
        #region positioning players
        positionPlayers();
        #endregion
    }
    private void positionPlayers()
    {
        List<GameObject> onBoardPlayers = new List<GameObject>();
        foreach (GameObject go in BoardMoveManager.instance.boardPlayers)
        {
            BoardPlayer bp = go.GetComponent<BoardPlayer>();
            if (bp.currentLandIndexOutdated)
            {
                WaitUntilOfDmr.InvokeWithDelay(this, positionPlayers, () => !bp.currentLandIndexOutdated);
                return;
            }
            if (bp.currentLandIndex == landIndex)
            {
                onBoardPlayers.Add(go);
            }
        }

        if (onBoardPlayers.Count == 1)
            onBoardPlayers[0].transform.position = slidePos;
        else
            for (int i = 0; i < onBoardPlayers.Count; i++)
                onBoardPlayers[i].transform.position = playerSitPositions[i];

    }
    void AssignPlotText(Transform t)
    {
        //its faster to write a code that assigns each text than assigning every each text via editor
        foreach (Transform child in t)
        {
            if (child.GetComponent<TextMeshProUGUI>() != null)
            {
                plotText = child.GetComponent<TextMeshProUGUI>();
            }
            AssignPlotText(child);
        }
    }

    public void WipeLand()
    {
        if (isItProtected)
        {
            if (this.landPrices.landBiome == Biome.Tree)
            {
                if (this.landPrices.isLandHit) return;
            }
            RemoveBarrier();
        }
        else
        {
            uint cachedOwnedBy = ownedBy;
            ResetLand(false);
            LandWiped?.Invoke(landIndex, cachedOwnedBy);
        }
    }
    public void RemoveBarrier()
    {
        isItProtected = false;
        LandBarrierWiped?.Invoke(landIndex);
    }
    public void ChangeOwnership(uint playerID)
    {
        ownedBy = playerID;
        PlayerOwnershipChanged?.Invoke(landIndex, playerID);
    }
    public void ResetLand(bool hardReset)
    {
        uint cachedPlayerId = ownedBy;
        if (hardReset)
        {
            if (isItProtected)
            {
                LandBarrierWiped?.Invoke(landIndex);
                isItProtected = false;
            }
        }
        if (landKind != LandKind.Buyable) return;
        ChangeOwnership(0);
        isItProtected = false;
        landCurrentUpgrade = 0;
        plotText.text = defaultString;
        Vector3 effectPos = slidePos;
        effectPos.y += 8;
        if (hardReset) LandWiped?.Invoke(landIndex, cachedPlayerId);
        SoundEffectManager.instance.PlayFlareUpSound();
        InvokerOfDmr.InvokeWithDelay(EffectManager.instance, EffectManager.instance.CreateLandWipeEffect, 0.5f, effectPos);
        LandBuyManager.instance.InvokeRecalculateAction(landIndex, 0, 0);
    }
}

[System.Serializable]
internal class MoveOrders
{
    internal PlotClass[] plotClasses;
    internal Transform player;
    internal int diceOne, diceTwo;
    internal int landmarkPosition;
}
