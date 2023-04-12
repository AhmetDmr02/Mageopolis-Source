using UnityEngine;

[CreateAssetMenu(fileName = "New Chance Instance", menuName = "Create Chance Instance")]
public class ChanceInstance : ScriptableObject
{
    public string ChanceTitle;
    public string ChanceDescription;
    public Sprite ChanceThumbnail;
    public int ChanceEffectId;
    public bool shouldPlayerRecheckEndTourButton = false;
}
