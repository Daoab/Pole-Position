using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.Basic;
using System.Linq;
using System.Threading;


public class ChatNetworkBehaviour : NetworkBehaviour
{
    SyncListString playerNames = new SyncListString();
    
    int playersReady = 0;
    [SerializeField] Text[] playerListUI;
    [SerializeField] Image[] playerReadyImageUI;
    [SerializeField] string defaultText = "Waiting...";
    [SerializeField] Text players;

    [SerializeField] Color playerReadyColor;
    [SerializeField] Color playerNotReadyColor;

    public void UpdatePlayersReady(bool playerReady)
    {
        if (playerReady)
        {
            Interlocked.Increment(ref playersReady);
        }
        else
        {
            Interlocked.Decrement(ref playersReady);
        }

        if (playersReady > 1 && playersReady >= (playerNames.Count / 2) + 1)
        {
            Debug.Log("LISTOS PARA COMENZAR");
        }

        Debug.Log("Jugadores listos: " + playersReady);
    }

    // QUITAR SI FUNCIONA PLAYERCHATLIST
    public void AddPlayerName(string playerName)
    {
        playerNames.Add(playerName);
        RpcUpdatePlayerListUI();
    }

    // QUITAR SI FUNCIONA PLAYERCHATLIST
    public void RemovePlayerName(string playerName, bool isReady)
    {
        if (isReady)
        {
            Interlocked.Decrement(ref playersReady);
        }

        playerNames.RemoveAt(playerNames.IndexOf(playerName));

        if (playerNames.Count > 1)
        {
            RpcUpdatePlayerListUI();
        }
    }


    public bool ContainsPlayerName(string playerName)
    {
        return playerNames.Contains(playerName);
    }


    public bool CheckLeader()
    {
        return playerNames.Count == 0;
    }
    
    [ClientRpc]
    public void RpcUpdatePlayerListUI()
    {
        for (int i = 0; i < playerListUI.Length; i++)
        {
            playerListUI[i].text = defaultText;
            //playerReadyImageUI[i].color = playerNotReadyColor;
        }

        for(int i = 0; i < playerNames.Count; i++)
        {
            playerListUI[i].text = playerNames[i];
        }
    }

    [ClientRpc]
    public void RpcUpdateCheckReadyList(string playerName, bool isReady)
    {
        for (int i = 0; i < playerNames.Count; i++)
        {
            if (playerListUI[i].text.Equals(playerName))
            {
                if (isReady)
                {
                    playerReadyImageUI[i].color = playerReadyColor;
                }
                else
                {
                    playerReadyImageUI[i].color = playerNotReadyColor;
                }
            }
        }
    }

    private void Start()
    {
        playerNames.Callback += PrintPlayerNames;
    }

    private void Update()
    {
        /*
        string aux = "";
        foreach(string name in playerNames)
        {
            aux += name + " ";
        }

        Debug.Log(aux);
        */
    }

    private void PrintPlayerNames(SyncListString.Operation op, int index, string oldItem, string newItem)
    {
        string aux = "";

        foreach(string name in playerNames)
        {
            aux += name + " ";
        }

        players.text = aux;
    }
}
