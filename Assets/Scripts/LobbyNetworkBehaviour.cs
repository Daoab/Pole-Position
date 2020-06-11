using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.Basic;
using System.Linq;
using System.Threading;

//[System.Serializable]
public class PlayerData
{
    public int id { get; set; }
    public string name { get; set; }
    public bool isReady { get; set; }
    public bool isLeader { get; set; }
    //public Color color { get; set; }

    public PlayerData(int id, string name, bool isReady, bool isLeader)
    {
        this.id = id;
        this.name = name;
        this.isReady = isReady;
        this.isLeader = isLeader;
        //this.color = color;
    }

    public PlayerData(){}
}


[System.Serializable]
public class SyncListPlayerData : SyncList<PlayerData> {}

public class LobbyNetworkBehaviour : NetworkBehaviour
{
    private SyncListPlayerData playerDataList = new SyncListPlayerData();

    int playersReady = 0;
    [SerializeField] Text[] playerListUI;
    [SerializeField] Image[] playerReadyImageUI;
    [SerializeField] string defaultText = "Waiting...";
    [SerializeField] Text players;

    [SerializeField] Color playerReadyColor;
    [SerializeField] Color playerNotReadyColor;

    public int FindPlayerDataIndex(int id)
    {
        for (int i = 0; i < playerDataList.Count; i++)
        {
            if (playerDataList[i].id == id)
                return i;
        }

        return -1;
    }

    public void UpdateNumberOfPlayersReady(bool playerReady)
    {
        if (playerReady)
        {
            Interlocked.Increment(ref playersReady);
        }
        else
        {
            Interlocked.Decrement(ref playersReady);
        }

        if (playersReady > 1 && playersReady >= (playerDataList.Count / 2) + 1)
        {
            Debug.Log("LISTOS PARA COMENZAR");
        }

        Debug.Log("Jugadores listos: " + playersReady);
    }


    public void UpdatePlayerIsReady(int id, bool isReady)
    {
        int index = FindPlayerDataIndex(id);
        playerDataList[index].isReady = isReady;

        RpcUpdatePlayerListUI();
    }

    /*
    public void UpdatePlayerColor(int id, Color color)
    {
        int index = FindPlayerDataIndex(id);
        playerDataList[index].color = color;
    }
    */

    public void AddPlayer(int id, string name, bool isReady, bool isLeader, Color color)
    {
        PlayerData p = new PlayerData(id, name, isReady, isLeader);

        playerDataList.Add(p);
        RpcUpdatePlayerListUI();
    }

    public void RemovePlayer(int id)
    {
        int index = FindPlayerDataIndex(id);

        if (index == -1) return;

        if (playerDataList[index].isReady)
        {
            Interlocked.Decrement(ref playersReady);
        }

        playerDataList.RemoveAt(index);

        if (playerDataList.Count > 1)
        {
            RpcUpdatePlayerListUI();
        }
    }


    public bool ContainsPlayerName(string playerName)
    {
        foreach (PlayerData p in playerDataList)
        {
            if (p.name.Equals(playerName))
            {
                return true;
            }
        }

        return false;
    }


    public bool CheckLeader()
    {
        return playerDataList.Count == 0;
    }
    
    [ClientRpc]
    public void RpcUpdatePlayerListUI()
    {
        for (int i = 0; i < playerListUI.Length; i++)
        {
            playerListUI[i].text = defaultText;
            playerReadyImageUI[i].color = playerNotReadyColor;
        }

        for(int i = 0; i < playerDataList.Count; i++)
        {
            playerListUI[i].text = playerDataList[i].name;
            if(playerDataList[i].isReady)
                playerReadyImageUI[i].color = playerReadyColor;
        }
    }

    private void Update()
    {
        string aux = "";

        for (int i = 0; i < playerDataList.Count; i++)
        {
            aux += playerDataList[i].isReady.ToString() + " ";
        }

        Debug.Log(aux);
    }

    private void Start()
    {
        playerDataList.Callback += PrintPlayerNames;
    }

    private void PrintPlayerNames(SyncListPlayerData.Operation op, int index, PlayerData oldItem, PlayerData newItem)
    {
        string aux = "";

        for(int i = 0; i < playerDataList.Count; i++)
        {
            aux += playerDataList[i].name + " ";
        }

        players.text = aux;
    }
}
