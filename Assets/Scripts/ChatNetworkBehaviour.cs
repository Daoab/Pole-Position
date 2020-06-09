using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ChatNetworkBehaviour : NetworkBehaviour
{
    SyncListString playerNames;
    [SerializeField] GameObject playerListUI;

    public void AddPlayerName(string playerName)
    {
        playerNames.Add(playerName);
    }

    public void RemovePlayerName(string playerName)
    {
        playerNames.Remove(playerName);
    }

    public bool ContainsPlayerName(string playerName)
    {
        return playerNames.Contains(playerName);
    }

    public void UpdatePlayerListUI()
    {

    }

    private void Update()
    {
        string aux = "";
        foreach(string name in playerNames)
        {
            aux += name + " ";
        }
        Debug.Log(aux);
    }
}
