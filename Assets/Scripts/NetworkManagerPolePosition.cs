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

    public void ReplacePlayer(NetworkConnection connection, GameObject newPlayer)
    {
        GameObject oldPlayer = connection.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(connection, newPlayer);
        NetworkServer.Destroy(oldPlayer);

        //Spawnear coche en alguno de los puntos de spawn disponibles
    }
}
