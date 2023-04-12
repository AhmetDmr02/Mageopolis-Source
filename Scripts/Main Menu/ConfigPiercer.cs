using Mirror;
public class ConfigPiercer : NetworkBehaviour
{
    [SyncVar(hook = nameof(changeBoardManagerValue))]
    public float playerSpeeds;
    [SyncVar]
    public int startSalary;
    [SyncVar]
    public bool shouldPlayersEnd;
    [SyncVar]
    public bool GameShouldEndWithSideMonopoly;
    [SyncVar]
    public bool GameShouldEndWith3Monopoly;

    public static ConfigPiercer instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void changeBoardManagerValue(float oldValue, float newValue)
    {
        if (BoardMoveManager.instance != null) BoardMoveManager.instance.speed = newValue;
    }
}
