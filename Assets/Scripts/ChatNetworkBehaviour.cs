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
    SyncListString playerNames;
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

        Debug.Log("Jugadores listos: " + playersReady);
    }

    public void AddPlayerName(string playerName)
    {
        playerNames.Add(playerName);
        RpcUpdatePlayerListUI();
    }

    public void RemovePlayerName(string playerName, bool isReady)
    {
        if (isReady)
        {
            Interlocked.Decrement(ref playersReady);
        }

        playerNames.Remove(playerName);

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

        string aux = "";

        for (int i = 0; i < playerNames.Count; i++)
        {
            aux += playerNames[i] + " ";
        }

        players.text = aux;

        for (int i = 0; i < playerListUI.Length; i++)
        {
            playerListUI[i].text = defaultText;
            playerReadyImageUI[i].color = playerNotReadyColor;
        }

        for(int i = 0; i < playerNames.Count; i++)
        {
            Debug.Log(playerNames[i] + " Estoy en el segundo for");
            playerListUI[i].text = playerNames[i];
            playerReadyImageUI[i].color = playerNotReadyColor;
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

    private void Update()
    {

        //RpcUpdatePlayerListUI();

        string aux = "";
        foreach(string name in playerNames)
        {
            aux += name + " ";
        }
        Debug.Log(aux);
    }
}
