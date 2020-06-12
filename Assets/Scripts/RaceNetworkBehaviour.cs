using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceNetworkBehaviour : NetworkBehaviour
{

    [SyncVar] public int numLaps = 1;
    [SerializeField] private int maxLaps = 6;

    UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
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
        PlayerLobby[] playersLobby = FindObjectsOfType<PlayerLobby>();

        foreach(PlayerLobby p in playersLobby)
        {
            p.InstantiateCar();
        }
    }

}
