using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Network : MonoSingleton<Network>
{
    public async void SendJumpRequest()
    {
        if (Client.Instance.Connected == false)
        {
            Debug.LogError("Not connected to server.");
            return;
        }

        var data = new { Action = "Jump" };
        string json = JsonUtility.ToJson(data);
        byte[] jsonData = Encoding.ASCII.GetBytes(json);

        Client.Instance.SendRequest(jsonData);
    }
}
