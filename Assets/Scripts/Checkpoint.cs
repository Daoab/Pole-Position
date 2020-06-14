using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] bool isGoal = false;
    [SerializeField] Checkpoint nextCheckpoint;

    RaceNetworkBehaviour raceNetworkBehaviour;

    private void Start()
    {
        raceNetworkBehaviour = FindObjectOfType<RaceNetworkBehaviour>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && other.GetComponent<SetupPlayer>().isLocalPlayer)
        {
            if (isGoal)
            {
                PlayerInfo playerInfo = other.GetComponent<PlayerInfo>();
                playerInfo.CurrentLap++;
                raceNetworkBehaviour.CheckRaceEnd(playerInfo);
            }
                
            nextCheckpoint.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
