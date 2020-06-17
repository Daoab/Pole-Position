using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PolePositionManager : NetworkBehaviour
{
    public int numPlayers;
    public NetworkManagerPolePosition networkManager;
    private UIManager uiManager;

    private readonly List<PlayerInfo> m_Players = new List<PlayerInfo>(4);
    public PlayerInfo playerLeader;
    SemaphoreSlim modifyPlayerSemaphore = new SemaphoreSlim(1, 1);

    [SyncVar] bool allPlayersReady = false;
    SemaphoreSlim updatePlayersReady = new SemaphoreSlim(1, 1);

    private CircuitController m_CircuitController;

    [Tooltip("Ángulo máximo del coche respecto a la dirección de la pista hasta que se detecta que va hacia atrás")]
    [SerializeField][Range(0f, 180f)] float goingBackwardsThreshold = 110f;

    private float circuitLength = 0f;

    [SyncVar] public int numLaps = 1;
    [SerializeField] private int maxLaps = 6;

    public int countdown = 3;

    LayerMask raceEndedLayer;

    private void Awake()
    {
        if (networkManager == null) networkManager = FindObjectOfType<NetworkManagerPolePosition>();
        if (m_CircuitController == null) m_CircuitController = FindObjectOfType<CircuitController>();

        circuitLength = m_CircuitController.CircuitLength;

        uiManager = FindObjectOfType<UIManager>();
        raceEndedLayer = LayerMask.NameToLayer("PlayerRaceEnded");

        Button minusButton = uiManager.GetMinusButton();
        minusButton.onClick.AddListener(() => DecrementLaps());

        Button addButton = uiManager.GetAddButton();
        addButton.onClick.AddListener(() => AddLaps());
    }

    #region PlayerList Methods
    public void AddPlayer(PlayerInfo player)
    {
        modifyPlayerSemaphore.Wait();
        if (player.isLeader) playerLeader = player;
        m_Players.Add(player);
        modifyPlayerSemaphore.Release();
    }

    public void RemovePlayer(PlayerInfo player)
    {
        modifyPlayerSemaphore.Wait();
        m_Players.Remove(player);
        playerLeader.GetComponent<PlayerLobby>().CmdUpdateUI();
        modifyPlayerSemaphore.Release();
    }
    
    public bool CheckIsLeader()
    {
        return m_Players.Count == 0;
    }
    #endregion

    #region Lobby
    public void UpdateNumberOfPlayersReady()
    {
        updatePlayersReady.Wait();

        int numPlayersReady = 0;

        foreach(PlayerInfo player in m_Players)
        {
            if (player.isReady)
                numPlayersReady++;
        }

        //Se puede comenzar la partida si la mayoría de jugadores (la mitad más uno (1)) están listos
        allPlayersReady = (numPlayersReady > 1 && numPlayersReady >= (m_Players.Count / 2) + 1);
        //allPlayersReady = true;
        playerLeader.GetComponent<PlayerLobby>().UpdateGoButtonState(allPlayersReady);

        updatePlayersReady.Release();
    }
    #endregion

    #region Race
    //Calcula la distancia que han recorrido los jugadores en total en el circuito, y los ordena según esa distancia,
    //de modo que se pueda obtener su posición en la carrera
    public void UpdateRaceProgress()
    {
        if (m_Players.Count == 0 && isLocalPlayer)
            return;

        for (int i = 0; i < m_Players.Count; ++i)
        {
            this.m_Players[i].GetComponent<SetupPlayer>().CmdChangeDistanceTravelled(ComputeCarArcLength(i));
            Debug.Log(this.m_Players[i].Name + " " + this.m_Players[i].distanceTravelled);
        }

        m_Players.Sort((x, y) => y.distanceTravelled.CompareTo(x.distanceTravelled));

        string raceOrder = "";

        for(int i = 0; i < m_Players.Count; i++)
        {
            raceOrder += this.m_Players[i].Name + " ";
            this.m_Players[i].GetComponent<SetupPlayer>().CmdChangeCurrentPosition(i + 1);
        }

        uiManager.UpdateRaceProgress(this.m_Players);

        Debug.Log("El orden de carrera es: " + raceOrder);
    }

    //Calcula cuánta distancia han recorrido los jugadores en el circuito tomando como referencia los puntos de los segmentos.
    //Dependiendo del número de vueltas que hayan dado los jugadores, se suma la longitud completa del circuito a la que
    //lleven recorrida en la vuelta actual para hallar la distancia total recorrida.
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

        minArcL += m_CircuitController.CircuitLength * m_Players[ID].CurrentLap;

        return minArcL;
    }

    //Comprueba si el coche está yendo marcha atrás, y establece el punto al que se ha de colocar al jugador en caso de que vuelque.
    public void UpdateRaceCarState(PlayerInfo player, SetupPlayer setupPlayer)
    {
        Vector3 carPos = player.transform.position;
        Vector3 carFwd = player.transform.forward;

        int segIdx;
        float carDist;
        Vector3 carProj;

        float minArcL =
          this.m_CircuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDist);

        //Se actualizan los datos de recuperación de choques del jugador
        setupPlayer.CmdChangeLastSafePosition(carProj);
        setupPlayer.CmdChangeCrashRecoverForward(m_CircuitController.GetSegment(segIdx));

        //Comprobación de si va hacia atrás (según el ángulo entre el forward del coche y la dirección del circuito)
        float ang = Vector3.Angle(m_CircuitController.GetSegment(segIdx), carFwd);
        setupPlayer.CmdChangeGoingBackwards(ang > goingBackwardsThreshold);

        if (player.goingBackwards) Debug.Log(player.name + " sentido contrario: " + player.goingBackwards);
    }
    #endregion

    #region laps
    public void AddLaps()
    {
        Debug.Log("AddLaps");
        numLaps++;
        numLaps = Mathf.Clamp(numLaps, 1, maxLaps);
        uiManager.UpdateLapsSettingsUI(numLaps);
    }

    public void DecrementLaps()
    {
        numLaps--;
        numLaps = Mathf.Clamp(numLaps, 1, maxLaps);
        uiManager.UpdateLapsSettingsUI(numLaps);
    }

    public int GetMaxLaps()
    {
        return maxLaps;
    }
    #endregion

    //Se muestra en la interfaz el nombre de los jugadores conectados, y si están listos o no
    public void UpdatePlayerListUI()
    {
        uiManager.UpdatePlayerListUI(m_Players);
    }

    #region RaceStart Rpc and RaceEnd
    [ClientRpc]
    public void RpcStartRace()
    {
        foreach (PlayerInfo p in m_Players)
        {
            p.GetComponent<PlayerLobby>().InstantiateCar();
        }
    }

    //Cuando un jugador termina la carrera, se indica que la ha terminado, y se le permite seguir jugando.
    //Sin embargo, no puede chocarse con otros jugadores y se para su contador de tiempo.
    public void CheckRaceEnd(PlayerInfo player)
    {
        /*
        if(player.CurrentLap >= numLaps)
        {
            Debug.Log("Race end");
            player.CmdRaceEnded(true);
            player.GetComponent<RaceTimer>().StopTimer();

            player.gameObject.layer = raceEndedLayer;
            Transform[] children = player.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
                child.gameObject.layer = raceEndedLayer;
            
            //Cuando la mayoría de jugadores han acabado la carrera se activa la interfaz de victoria
            //Activar UI de victoria
            //Mantener posición de los jugadores que han acabado
        }*/
    }
    #endregion
}