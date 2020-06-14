using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceNetworkBehaviour : NetworkBehaviour
{

    [SyncVar] public int numLaps = 1;
    [SerializeField] private int maxLaps = 6;

    UIManager uiManager;
    LayerMask raceEndedLayer;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        raceEndedLayer = LayerMask.NameToLayer("PlayerRaceEnded");
    }

    public void AddLaps()
    {
        numLaps++;
        numLaps = Mathf.Clamp(numLaps, 1, maxLaps);
        uiManager.UpdateLapsCounter(numLaps);
    }

    public void DecrementLaps()
    {
        numLaps--;
        numLaps = Mathf.Clamp(numLaps, 1, maxLaps);
        uiManager.UpdateLapsCounter(numLaps);
    }

    public void StartRace()
    {
        uiManager.ActivateRaceUI();

        PlayerLobby[] playersLobby = FindObjectsOfType<PlayerLobby>();
        
        foreach(PlayerLobby p in playersLobby)
        {
            p.InstantiateCar();
        }
    }

    public int GetMaxLaps()
    {
        return maxLaps;
    }

    public void CheckRaceEnd(PlayerInfo player)
    {
        if(player.CurrentLap >= numLaps)
        {
            Debug.Log("Race end");
            player.raceEnded = true;
            player.GetComponent<RaceTimer>().StopTimer();

            player.gameObject.layer = raceEndedLayer;
            Transform[] children = player.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
                child.gameObject.layer = raceEndedLayer;
            
            //Cuando la mayoría de jugadores han acabado la carrera se activa la interfaz de victoria
            //Activar UI de victoria
        }
    }
}