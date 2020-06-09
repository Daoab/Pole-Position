using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ChatNetworkBehaviour : NetworkBehaviour
{
    SyncListString playerNames;

    public void AddPlayerName(string playerName)
    {
        playerNames.Add(playerName);
    }

    public bool ContainsPlayerName(string playerName)
    {
        return playerNames.Contains(playerName);
    }

    private void Update()
    {
        foreach(string name in playerNames)
        {
            Debug.Log(name);
        }
    }
}
