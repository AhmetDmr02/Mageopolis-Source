using UnityEngine;

[CreateAssetMenu(fileName = "Create New Land", menuName = "New Land")]
public class LandInstance : ScriptableObject
{
    [Header("Main Stats")]
    public LandKind landKind;
    public string landName;
    [TextArea]
    public string landDescription;
    [TextArea]
    public string biomeDescription;
    public Color landThemeColor;
    public bool CustomBounce;

    [ShowWhen("CustomBounce", true)]
    public float landBounceY;

    [ShowWhen("isLandBuyable", true)]
    public LandPrices landPrices;

    [Space(10), Header("Etc.")]
    public bool defaultAudio;
    [ShowWhen("defaultAudio", false)]
    public AudioClip stepAudio, landAudio;
    [ShowWhen("defaultAudio", false)]
    public float landAudioFloat = 1f;

    [HideInInspector] public bool isLandBuyable;

    private void Awake()
    {
        isLandBuyable = landKind == LandKind.Buyable ? true : false;
    }
    private void OnValidate()
    {
        isLandBuyable = landKind == LandKind.Buyable ? true : false;
    }
}
[System.Serializable]
public class LandPrices
{
    public int landBuyPrice;
    public int landBaseSalary;
    //0,1,2,3;
    public int[] landUpgradeSalaries;
    //0,1,2,3;
    public int[] landUpgradePrices;
    //When Player Buy Every Land For Biome
    public float landHitPriceMultiplier;
    //Player Can Destroy Non Fully Upgraded Bases If Player Has One Plot On Library
    public float landDestroyMultiplier;
    //To Determine Hit
    public Biome landBiome;
    public bool isLandHit = false;
}

[System.Serializable]
public enum LandKind
{
    Buyable,
    EventLand,
    Empty
}
public enum Biome
{
    Cave,
    Swamp,
    Library,
    Tree,
    Void,
    GoldenLand,
    Mists,
    Volcano
}