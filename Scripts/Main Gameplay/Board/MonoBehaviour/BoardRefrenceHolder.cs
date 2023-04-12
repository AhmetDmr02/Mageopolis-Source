using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardRefrenceHolder : MonoBehaviour
{
    //I made a typo by writing Refrence sadly cannot change because of my VC :(
    public static BoardRefrenceHolder instance;
    public Camera UIboardCamera;
    public Camera boardCamera;
    public GameObject[] plotsObjectsWithOrder;
    public Dice[] Dices;
    public GameObject TimerObject;
    public Image TimerBar;
    public EndTourButton endTourButton;
    public CameraController cameraController;
    public GameObject vacuumPlace;
    public TextMeshProUGUI vacuumText;
    public Animator middleChargerAnimator, treeAnimator, volcanoAnimator;
    public ParticleSystem volcanoBallEffect, volcanoBallMuzzle;
    public GameObject quicksandInterface;
    public GameObject vacuumEffect;
    public GameObject DiceParticles;
    public EndGameStats GameEndStats;
    public ChanceVisualizer chanceVisualiser;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
}
