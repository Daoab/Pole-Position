using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerPolePosition : NetworkManager
{
    ChatNetworkBehaviour chatNetworkBehaviour;

    public override void Awake()
    {
        base.Awake();
        chatNetworkBehaviour = FindObjectOfType<ChatNetworkBehaviour>();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        
    }
}
