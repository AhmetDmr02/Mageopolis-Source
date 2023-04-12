using UnityEngine;

public class DestroyMeAfter : MonoBehaviour
{
    [SerializeField] private float destroySeconds;

    private void Start()
    {
        Destroy(this.gameObject, destroySeconds);
    }
}
