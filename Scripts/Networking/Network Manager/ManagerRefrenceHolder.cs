using UnityEngine;
using System.Collections.Generic;
using Mirror;
public class ManagerRefrenceHolder : MonoBehaviour
{
    //I made a typo by writing Refrence sadly cannot change because of my VC :(
    public static ManagerRefrenceHolder instance;
    public GameObject networkManagerObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
