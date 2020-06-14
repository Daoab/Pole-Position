using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mirror;
using UnityEngine;

public class PolePositionManager : NetworkBehaviour
{
    public int numPlayers;
    public NetworkManagerPolePosition networkManager;

    private readonly List<PlayerInfo> m_Players = new List<PlayerInfo>(4); //Error de concurrencia, es una lista normal
    SemaphoreSlim addPlayerSemaphore = new SemaphoreSlim(1, 1);

    private CircuitController m_CircuitController;
    private GameObject[] m_DebuggingSpheres;

    [Tooltip("Ángulo máximo del coche respecto a la dirección de la pista hasta que se detecta que va hacia atrás")]
    [SerializeField][Range(0f, 180f)] float goingBackwardsThreshold = 110f;

    private float circuitLength = 0f;

    private void Awake()
    {
        if (networkManager == null) networkManager = FindObjectOfType<NetworkManagerPolePosition>();
        if (m_CircuitController == null) m_CircuitController = FindObjectOfType<CircuitController>();

        m_DebuggingSpheres = new GameObject[networkManager.maxConnections];
        for (int i = 0; i < networkManager.maxConnections; ++i)
        {
            m_DebuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_DebuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }

        circuitLength = m_CircuitController.CircuitLength;
    }

    private void Update()
    {
        if (m_Players.Count == 0)
            return;

        UpdateRaceProgress();
    }

    public void AddPlayer(PlayerInfo player)
    {
        addPlayerSemaphore.Wait();
        m_Players.Add(player);
        addPlayerSemaphore.Release();
    }

    public void UpdateRaceProgress()
    {
        for (int i = 0; i < m_Players.Count; ++i)
        {
            this.m_Players[i].distanceTravelled = ComputeCarArcLength(i);
            //Debug.Log(this.m_Players[i].Name + " Distancia: " + this.m_Players[i].distanceTravelled);
        }

        m_Players.Sort((x, y) => y.distanceTravelled.CompareTo(x.distanceTravelled));

        string myRaceOrder = "";
        foreach (var _player in m_Players)
        {
            myRaceOrder += _player.Name + " ";
        }

        Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

    float ComputeCarArcLength(int ID)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = this.m_Players[ID].transform.position;
        Vector3 carFwd = this.m_Players[ID].transform.forward;

        int segIdx;
        float carDist;
        Vector3 carProj;

        float minArcL =
            this.m_CircuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDist);

        //Se actualizan los datos de recuperación de choques del jugador
        this.m_Players[ID].lastSafePosition = carProj;
        this.m_Players[ID].crashRecoverForward = m_CircuitController.GetSegment(segIdx);

        //Comprobación de si va hacia atrás (según el ángulo entre el forward del coche y la dirección del circuito)
        float ang = Vector3.Angle(m_CircuitController.GetSegment(segIdx), carFwd);
        this.m_Players[ID].goingBackwards = ang > goingBackwardsThreshold;

        if (this.m_Players[ID].goingBackwards) Debug.Log(m_Players[ID].name + " sentido contrario: " + m_Players[ID].goingBackwards);

        this.m_DebuggingSpheres[ID].transform.position = carProj;

        minArcL += m_CircuitController.CircuitLength * (m_Players[ID].CurrentLap /*- 1*/);

        /*if (this.m_Players[ID].CurrentLap == 0)
        {
            minArcL -= m_CircuitController.CircuitLength;
        }
        else
        {
            
        }*/

        return minArcL;
    }
}