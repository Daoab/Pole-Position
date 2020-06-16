using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] bool isGoal = false;
    [SerializeField] Checkpoint nextCheckpoint;

    public float distanceToNextCheckpoint;

    //RaceNetworkBehaviour raceNetworkBehaviour;
    UIManager uIManager;

    private void Start()
    {
        //raceNetworkBehaviour = FindObjectOfType<RaceNetworkBehaviour>();
        distanceToNextCheckpoint = Mathf.Abs(Vector3.Distance(gameObject.transform.position, nextCheckpoint.gameObject.transform.position));
        uIManager = FindObjectOfType<UIManager>();
    }

    //Se ha implementado un sistema de checkpoints para asegurar que las vueltas al circuito se realizan correctamente
    //Cuando un jugador local toca un checkpoint, se activa el siguiente, hasta llegar a la meta.
    //Cuando el jugador ha pasado por todos los checkpoints y toca la meta se contabiliza una vuelta
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && other.GetComponent<SetupPlayer>().isLocalPlayer)
        {
            PlayerInfo playerInfo = other.GetComponent<PlayerInfo>();
            SetupPlayer setupPlayer = other.GetComponent<SetupPlayer>();

            if (isGoal)
            {
                setupPlayer.CmdChangeCurrentLap(playerInfo.CurrentLap + 1);
                //raceNetworkBehaviour.CheckRaceEnd(playerInfo);
            }

            nextCheckpoint.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
