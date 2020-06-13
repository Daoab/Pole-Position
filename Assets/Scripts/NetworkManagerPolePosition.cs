using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerPolePosition : NetworkManager
{
    LobbyNetworkBehaviour chatNetworkBehaviour;
    UIManager uIManager;

    public override void Awake()
    {
        base.Awake();
        chatNetworkBehaviour = FindObjectOfType<LobbyNetworkBehaviour>();
        uIManager = FindObjectOfType<UIManager>();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
    }

    public void ReplacePlayer(NetworkConnection connection, GameObject newPlayerPrefab, int id, string name, Vector3 position, Quaternion rotation, Color carColor)
    {
        GameObject oldPlayer = connection.identity.gameObject;

        GameObject newPlayer = GameObject.Instantiate(newPlayerPrefab, position, rotation);
        PlayerInfo playerInfo = newPlayer.GetComponent<PlayerInfo>();

        playerInfo.Name = name;
        playerInfo.ID = id;
        playerInfo.color = carColor;

        newPlayer.GetComponent<SetupPlayer>().UpdatePlayerModelColor(carColor);

        NetworkServer.ReplacePlayerForConnection(connection, newPlayer, true);

        NetworkServer.Destroy(oldPlayer);
    }
}
