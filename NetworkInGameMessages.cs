using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkInGameMessages : NetworkBehaviour
{
    InGameMessageUIHandler inGameMessageUIHandler;
    KillCounter killCounter;

    private void Awake()
    {
        killCounter = GetComponent<KillCounter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SendInGameRPCMessage(string userNickName, string message)
    {
        RPC_InGameMessage($"<b>{userNickName}</b> {message}");
    }

    public void SendInGameRPCEndMessage(string message)
    {
        RPC_InGameMessage($"{message}");
    }

    public void SendInGameRPCKillMessage(string message)
    {
        RPC_InGameKillMessage($"{message}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_InGameMessage(string message, RpcInfo info = default)
    {
        //Debug.Log($"[RPC] InGameMessage {message}");

        if (inGameMessageUIHandler == null)
            inGameMessageUIHandler = NetworkPlayer.Local.localCameraHandler.GetComponentInChildren<InGameMessageUIHandler>();

        if (inGameMessageUIHandler != null)
            inGameMessageUIHandler.OnGameMessageReceived(message);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_InGameKillMessage(string message, RpcInfo info = default)
    {
        Debug.Log($"[RPC] InGameMessage {message}");

        killCounter.OnKillMessageReceived(message);
    }

}
