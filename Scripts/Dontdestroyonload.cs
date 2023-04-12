using UnityEngine;

public class Dontdestroyonload : MonoBehaviour
{
    public static Dontdestroyonload instance;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
}
