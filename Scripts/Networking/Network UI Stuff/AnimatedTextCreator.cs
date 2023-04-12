using Mirror;
using TMPro;
using UnityEngine;

public class AnimatedTextCreator : NetworkBehaviour
{
    [SerializeField] private GameObject animatedTextObject;
    [SerializeField] private Canvas canvasMain;
    public static AnimatedTextCreator instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void FixedUpdate()
    {
        if (canvasMain != null) return;
        canvasMain = MainNetworkManagerCON.singleton.gameObject.transform.GetChild(2).GetComponent<Canvas>();
    }
    public void CreateAnimatedText(string text, Color32 color)
    {
        if (!isServer) return;
        createAnimatedText(text, color);
    }

    private GameObject recentObject;
    [ClientRpc]
    private void createAnimatedText(string text, Color32 color)
    {
        if (recentObject != null) Destroy(this.recentObject);
        GameObject go = Instantiate(animatedTextObject, canvasMain.transform);
        go.GetComponent<TextMeshProUGUI>().text = text;
        go.GetComponent<TextMeshProUGUI>().color = color;
        go.GetComponent<Animation>().Play();
        go.AddComponent<DestroyMe>().destroyMeAfterSeconds(25);
        recentObject = go;
    }
}
