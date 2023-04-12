using Mirror;
using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class PingIndicator : MonoBehaviour
{
    public TextMeshProUGUI pingText;
    void FixedUpdate()
    {
        // only while client is active
        if (!NetworkClient.active) return;
        if (NetworkServer.active)
        {
            //Server 
            pingText.text = "";
            return;
        }
        pingText.text = $"Ping: {Math.Round(NetworkTime.rtt * 1000)}ms";
    }
}
