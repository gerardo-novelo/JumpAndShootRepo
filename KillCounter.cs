using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillCounter : MonoBehaviour
{

    NetworkPlayer networkPlayer;


    private void Awake()
    {
        networkPlayer = GetComponentInParent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnKillMessageReceived(string message)
    {
        Debug.Log($"este es el mensaje {message}");
        Debug.Log($"este es el nickname {networkPlayer.nickName.ToString()}");
        if (message == networkPlayer.nickName.ToString())
        {
            networkPlayer.KillCounter++;
            Debug.Log($"{networkPlayer.KillCounter}");
        }

        if (networkPlayer.KillCounter >= 10)
        {
            Debug.Log($"Ya se deberia haber acabado");
            Application.Quit();
        }
    }
}
