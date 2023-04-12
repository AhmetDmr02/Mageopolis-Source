using UnityEngine;
using TMPro;
using EZCameraShake;
public class MainBoardDirector : MonoBehaviour
{
    //This script is just mess because there is no point making it efficient as possible its just controller for board animation
    private Animator animator;
    private int diceInt1, diceInt2;
    [SerializeField] private TextMeshProUGUI totalText;
    private void Start()
    {
        animator = this.gameObject.GetComponent<Animator>();
        DiceManager.diceFired += initDiceAnimation;
    }
    public void giveRandomRotationToDices()
    {
        Transform[] t = { BoardRefrenceHolder.instance.Dices[0].transform, BoardRefrenceHolder.instance.Dices[1].transform };
        float powerf = 250;
        t[0].eulerAngles = Vector3.Lerp(t[0].eulerAngles, new Vector3(Random.Range(0, 256), Random.Range(0, 256), Random.Range(0, 256)), powerf * Time.deltaTime);
        t[1].eulerAngles = Vector3.Lerp(t[1].eulerAngles, new Vector3(Random.Range(0, 256), Random.Range(0, 256), Random.Range(0, 256)), powerf * Time.deltaTime);
    }
    public void InitLaunchSoundEffect()
    {
        totalText.text = "";
        SoundEffectManager.instance.PlayDiceLaunchSound();
    }
    public void InitLandSoundEffect()
    {
        SoundEffectManager.instance.PlayDiceLandSound();
        BoardRefrenceHolder.instance.Dices[0].gameObject.transform.eulerAngles = BoardRefrenceHolder.instance.Dices[0].eulerAnglesOfDice[diceInt1 - 1];
        BoardRefrenceHolder.instance.Dices[1].gameObject.transform.eulerAngles = BoardRefrenceHolder.instance.Dices[1].eulerAnglesOfDice[diceInt2 - 1];
    }
    public void InitLandImpactEffect()
    {
        SoundEffectManager.instance.PlayDiceImpactSound();
        EffectManager.instance.PlayDiceHitEffect();
        totalText.text = (diceInt1 + diceInt2).ToString();
    }
    public void ShakeCamera()
    {
        CameraShaker.Instance.ShakeOnce(1, 1, 0, 1.3f);
    }
    public void initDiceAnimation(int firstDice, int secondDice, BoardPlayer bp)
    {
        diceInt1 = firstDice;
        diceInt2 = secondDice;
        animator.Play("DiceLaunch", -1, 0.0f);
    }
    private void OnDestroy()
    {
        DiceManager.diceFired -= initDiceAnimation;
    }
}
