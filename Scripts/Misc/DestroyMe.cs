using UnityEngine;

public class DestroyMe : MonoBehaviour
{
    public void destroyMeAfterSeconds(float destroyF)
    {
        Destroy(this.gameObject, destroyF);
    }
}
