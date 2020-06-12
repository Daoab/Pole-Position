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

    //Devuelve el índice de los datos asociados a un jugador según su id, y si no lo encuentra devuelve -1
    public int FindPlayerDataIndex(int id)
    {
        for (int i = 0; i < playerDataList.Count; i++)
        {
            if (playerDataList[i].id == id)
                return i;
        }
        return -1;
    }

    #region Updates

    //Se comprueba el número de jugadores listos, y si se puede empezar la partida
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

        //Se puede comenzar la partida si la mayoría de jugadores (la mitad más uno (1)) están listos
        if (playersReady > 1 && playersReady >= (playerDataList.Count / 2) + 1)
        {
            allPlayersReady = true;
        }
        else
        {
            allPlayersReady = false;
        }
    }

    //Actualiza el valor isReady de un jugador según su id
    public void UpdatePlayerIsReady(int id, bool isReady)
    {
        int index = FindPlayerDataIndex(id);

        //No se puede modificar la estructura de la SyncList,
        //ya que si no no se modificaría su dirty bit y no se modificaría en el resto de clientes
        PlayerData newPlayerData = playerDataList[index];
        newPlayerData.isReady = isReady;

        playerDataList[index] = newPlayerData;

        UpdateNumberOfPlayersReady(isReady);

        UpdatePlayerListUI();
    }

    //Se actualiza el color de un jugador según su id
    public void UpdatePlayerColor(int id, Color color)
    {
        int index = FindPlayerDataIndex(id);

        //La synclist solo se actualiza si le metemos una struct nueva, no podemos modificar la lista directamente
        PlayerData newPlayerData = playerDataList[index];

        newPlayerData.color = color;

        playerDataList[index] = newPlayerData;
    }

    //Se muestra en la interfaz el nombre de los jugadores conectados, y si están listos o no
    public void UpdatePlayerListUI()
    {
        for (int i = 0; i < playerListUI.Length; i++)
        {
            playerListUI[i].text = defaultText;
            playerReadyImageUI[i].color = playerNotReadyColor;
        }

        for (int i = 0; i < playerDataList.Count; i++)
        {
            playerListUI[i].text = playerDataList[i].name;
            if (playerDataList[i].isReady)
                playerReadyImageUI[i].color = playerReadyColor;
        }
    }
    #endregion

    #region SyncList Methods

    public void AddPlayer(PlayerLobby player)
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

    //El jugador que ha entrado primero a la partida es el líder de la misma
    //Puede modificar las opciones de la carrera y empezar la partida
    public bool CheckLeader()
    {
        return playerDataList.Count == 0;
    }

    #endregion

    #region Hooks

    private void PrintPlayerNames(SyncListPlayerData.Operation op, int index, PlayerData oldPlayerData, PlayerData newPlayerData)
    {
        string aux = "";

        for(int i = 0; i < playerDataList.Count; i++)
        {
            aux += playerDataList[i].name + " ";
        }

        players.text = aux;
    }

    //Cuando se actualiza allPlayersReady se comprueba si se puede empezar la partida, y por tanto si se puede mostrar el botón de go
    public void NotifyPlayersReady(bool oldValue, bool newValue)
    {
        playerLeader.UpdateGoButtonState(newValue);
    }

    //Cuando se actualiza la lista de datos de los jugadores, se actualiza también la interfaz
    public void UpdateUI(SyncListPlayerData.Operation op, int index, PlayerData oldPlayerData, PlayerData newPlayerData)
    {
        UpdatePlayerListUI();
    }

    #endregion
}