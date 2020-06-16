using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerPolePosition : NetworkManager
{
    //LobbyNetworkBehaviour chatNetworkBehaviour;
    UIManager uIManager;
    PolePositionManager polePositionManager;

    public override void Awake()
    {
        base.Awake();
        uIManager = FindObjectOfType<UIManager>();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        NetworkServer.Destroy(conn.identity.gameObject);
        base.OnClientDisconnect(conn);
    }
}
