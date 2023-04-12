using UnityEngine;

public class Quicksand_Requester : MonoBehaviour
{
    public void requestPayFee()
    {
        Quicksand.instance.RequestPayForQuicksand();
    }
}
