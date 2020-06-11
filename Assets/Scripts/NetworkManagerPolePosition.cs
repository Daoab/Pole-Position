using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerPolePosition : NetworkManager
{
    LobbyNetworkBehaviour chatNetworkBehaviour;

    public override void Awake()
    {
        base.Awake();
        chatNetworkBehaviour = FindObjectOfType<LobbyNetworkBehaviour>();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        
    }
}
