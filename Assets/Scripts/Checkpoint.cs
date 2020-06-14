using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] bool isGoal = false;
    [SerializeField] Checkpoint nextCheckpoint;

    public float distanceToNextCheckpoint;

    RaceNetworkBehaviour raceNetworkBehaviour;

    private void Start()
    {
        raceNetworkBehaviour = FindObjectOfType<RaceNetworkBehaviour>();
        distanceToNextCheckpoint = Mathf.Abs(Vector3.Distance(gameObject.transform.position, nextCheckpoint.gameObject.transform.position));
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && other.GetComponent<SetupPlayer>().isLocalPlayer)
        {
            PlayerInfo playerInfo = other.GetComponent<PlayerInfo>();

            if (isGoal)
            {
                playerInfo.CmdAddLap();
                raceNetworkBehaviour.CheckRaceEnd(playerInfo);
            }

            nextCheckpoint.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
