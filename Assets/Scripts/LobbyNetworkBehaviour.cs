using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.Basic;
using System.Linq;
using System.Threading;

[System.Serializable]
public struct PlayerData
{
    public int id;
    public string name;
    public bool isReady;
    public bool isLeader;
    public Color color;
}

[System.Serializable]
public class SyncListPlayerData : SyncList<PlayerData> {}

public class LobbyNetworkBehaviour : NetworkBehaviour
{
    private SyncListPlayerData playerDataList = new SyncListPlayerData();

    SemaphoreSlim updatePlayersReady = new SemaphoreSlim(1, 1);

    int playersReady = 0;
    [SerializeField] Text[] playerListUI;
    [SerializeField] Image[] playerReadyImageUI;
    [SerializeField] string defaultText = "Waiting...";
    [SerializeField] Text players;

    [SerializeField] Color playerReadyColor;
    [SerializeField] Color playerNotReadyColor;

    PlayerLobby playerLeader;

    [SyncVar (hook = nameof(NotifyPlayersReady))] public bool allPlayersReady;

    private void Start()
    {
        playerDataList.Callback += PrintPlayerNames;
        playerDataList.Callback += UpdateUI;
    }

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

        updatePlayersReady.Wait();

        if (playerReady)
        {
            playersReady++;
        }
        else
        {
            playersReady--;
        }

        updatePlayersReady.Release();

        if (playersReady > 1 && playersReady >= (playerDataList.Count / 2) + 1)
        {
            Debug.Log("LISTOS PARA COMENZAR");
            allPlayersReady = true;
        }
        else
        {
            allPlayersReady = false;
        }

        Debug.Log("Jugadores listos: " + playersReady);
    }

    public void UpdatePlayerIsReady(int id, bool isReady)
    {
        int index = FindPlayerDataIndex(id);

        PlayerData newPlayerData = playerDataList[index];
        newPlayerData.isReady = isReady;
        
        playerDataList[index] = newPlayerData;

        UpdateNumberOfPlayersReady(isReady);

        UpdatePlayerListUI();
    }

    public void UpdatePlayerColor(int id, Color color)
    {
        int index = FindPlayerDataIndex(id);

        //La synclist solo se actualiza si le metemos una struct nueva, no podemos modificar la lista directamente
        PlayerData newPlayerData = playerDataList[index];

        newPlayerData.color = color;

        playerDataList[index] = newPlayerData;
    }

    public void AddPlayer(/*int id, string name, bool isReady, bool isLeader, Color color,*/ PlayerLobby player)
    {
        PlayerData p = new PlayerData
        {
            id = player.id,
            name = player.playerName,
            isReady = player.isReady,
            isLeader = player.isLeader,
            color = player.playerColor
        };

        if (p.isLeader)
        {
            playerLeader = player;
        }

        playerDataList.Add(p);
        UpdatePlayerListUI(); 
    }

    public void RemovePlayer(int id)
    {
        int index = FindPlayerDataIndex(id);

        if (index == -1) return;

        if (playerDataList[index].isReady)
        {
            updatePlayersReady.Wait();
            playersReady--;
            updatePlayersReady.Release();
        }

        playerDataList.RemoveAt(index);

        if (playerDataList.Count > 1)
        {
            UpdatePlayerListUI();
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
    
    public void UpdatePlayerListUI()
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
    
    public void UpdateUI(SyncListPlayerData.Operation op, int index, PlayerData oldPlayerData, PlayerData newPlayerData)
    {
        UpdatePlayerListUI();
    }

    public SyncListPlayerData GetPlayersData()
    {
        return playerDataList;
    }

    private void PrintPlayerNames(SyncListPlayerData.Operation op, int index, PlayerData oldPlayerData, PlayerData newPlayerData)
    {
        string aux = "";

        for(int i = 0; i < playerDataList.Count; i++)
        {
            aux += playerDataList[i].name + " ";
        }

        players.text = aux;
    }

    public void NotifyPlayersReady(bool oldValue, bool newValue)
    {
        playerLeader.UpdateGoButtonState(newValue);
    }
}