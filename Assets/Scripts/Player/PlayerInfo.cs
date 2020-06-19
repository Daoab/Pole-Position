using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[System.Serializable]
public class PlayerInfo : NetworkBehaviour
{
    #region Lobby variables
    [SyncVar] public string Name;

    [SyncVar] public int ID;

    [SyncVar] public Color color;

    [SyncVar] public bool isReady = false;

    [SyncVar] public bool isLeader = false;
    #endregion

    #region Race variables
    [SyncVar] public int CurrentPosition;

    [SyncVar] public int CurrentLap = 0;

    [SyncVar] public float distanceTravelled = 0f;

    [SyncVar] public bool goingBackwards = false;

    [SyncVar] public bool raceEnded = false;

    //Posición a la que se recuperará el jugador si choca
    [SyncVar] public Vector3 lastSafePosition;

    //Dirección a la que mirará el jugador si choca
    [SyncVar] public Vector3 crashRecoverForward;

    [SyncVar] public float totalTime = 0f;
    
    [SyncVar] public float currentLapTime = 0f;
    #endregion

    UIManager uiManager;
    PolePositionManager polePositionManager;

    [Tooltip("Tiempo máximo que pueden estar los jugadores yendo en dirección contraria hasta que se les avisa por la UI")]
    [SerializeField] float timeGoingBackwards = 1f;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        polePositionManager = FindObjectOfType<PolePositionManager>();

        //Color por defecto del coche (primero de los que se pueden seleccionar)
        color.r = 245/255f;
        color.g = 73/255f;
        color.b = 93/255f;
        color.a = 1f;

        //Lobby callbacks
        PlayerLobby.OnIsReady += ChangePlayerIsReady;
        PlayerLobby.OnName += ChangePlayerName;
        PlayerLobby.OnColor += ChangeColor;
        PlayerLobby.OnIsLeader += ChangeIsLeader;

        //Race callbacks
        SetupPlayer.OnCurrentPosition += ChangeCurrentPosition;
        SetupPlayer.OnCurrentLap += ChangeCurrentLap;
        SetupPlayer.OnDistanceTravelled += ChangeDistanceTravelled;
        SetupPlayer.OnGoingBackwards += ChangeGoingBackwards;
        SetupPlayer.OnRaceEnded += ChangeRaceEnded;
        SetupPlayer.OnLastSafePosition += ChangeLastSafePosition;
        SetupPlayer.OnCrashRecoverForward += ChangeCrashRecoverForward;

        RaceTimer.OnTotalTime += ChangeTotalTime;
        RaceTimer.OnLapTime += ChangeLapTime;
    }

    #region Lobby Callbacks
    //Actualiza el valor de isReady de un jugador concreto y actualiza la lista de jugadores y su estado en el lobby
    public void ChangePlayerIsReady(PlayerInfo player, bool isReady)
    {
        if(this.ID == player.ID) player.isReady = isReady;
        polePositionManager.UpdatePlayerListUI();
        polePositionManager.UpdateNumberOfPlayersReady();
    }

    //Actualiza el nombre de un jugador en concreto y actualza la lista de jugadores en el lobby
    public void ChangePlayerName(PlayerInfo player, string name)
    {
        if (this.ID == player.ID) player.Name = name;
        polePositionManager.UpdatePlayerListUI();
    }

    //Actualiza el color de un jugador concreto y el color del coche de preview en el lobby
    public void ChangeColor(PlayerInfo player, Color color)
    {
        if (this.ID == player.ID) player.color = color;

        if (player.isLocalPlayer) uiManager.UpdateCarPreviewColor(color);
    }

    //Actualiza el valor de isLeader (tendrá acceso a los ajustes de la carrera) de un jugador concreto
    public void ChangeIsLeader(PlayerInfo player, bool isLeader)
    {
        if (this.ID == player.ID) player.isLeader = isLeader;
    }
    #endregion

    #region Race Callbacks
    //Actualiza la posición en el podio de un jugador concreto
    public void ChangeCurrentPosition(PlayerInfo player, int currentPosition)
    {
        if (this.ID == player.ID && !player.raceEnded) this.CurrentPosition = currentPosition;
    }

    //Actualiza el número de vueltas que ha dado un jugador concreto, comrpueba si ha terminado la carrera
    //y actualiza el contador de vueltas en la UI
    public void ChangeCurrentLap(PlayerInfo player, int currentLap)
    {
        if (this.ID == player.ID)
        {
            currentLap = Mathf.Clamp(currentLap, 0, polePositionManager.numLaps);
            this.CurrentLap = currentLap;
            polePositionManager.CheckPlayerFinished(this);
        }

        if (this.isLocalPlayer)
            uiManager.UpdateLapProgress(this.CurrentLap);
    }

    //Actualiza la distancia que ha recorrido por el circuito un jugador concreto
    public void ChangeDistanceTravelled(PlayerInfo player, float distanceTravelled)
    {
        if (this.ID == player.ID) this.distanceTravelled = distanceTravelled;
    }

    //Actualiza goingBackwards de un jugdor concreto y la interfaz que indica si
    //un jugador va marcha atrás
    public void ChangeGoingBackwards(PlayerInfo player, bool goingBackwards)
    {
        if (this.ID == player.ID) this.goingBackwards = goingBackwards;

        if (this.isLocalPlayer)
            StartCoroutine(WaitChangeGoingBackwards(player, goingBackwards));
    }

    //Corrutina que espera un cierto tiempo antes de avisar a los jugadores de que van marcha atrás
    private IEnumerator WaitChangeGoingBackwards(PlayerInfo player, bool goingBackwards)
    {
        bool previousValue = goingBackwards;

        yield return new WaitForSecondsRealtime(timeGoingBackwards);

        if(this.goingBackwards == previousValue)
            uiManager.UpdateTurnBack(this.goingBackwards);
    }

    //Actualiza si un jugador concreto ha terminado la carrera
    public void ChangeRaceEnded(PlayerInfo player, bool raceEnded)
    {
        if (this.ID == player.ID) this.raceEnded = raceEnded;
    }

    //Actualiza la posición a la que se debe recuperar un jugador concreto si choca
    public void ChangeLastSafePosition(PlayerInfo player, Vector3 lastSafePosition)
    {
        if (this.ID == player.ID) this.lastSafePosition = lastSafePosition;
    }

    //Actualiza la dirección a la que ha de mirar un jugador concreto al recuperarse de un choque
    public void ChangeCrashRecoverForward(PlayerInfo player, Vector3 crashRecoverForward)
    {
        if (this.ID == player.ID) this.crashRecoverForward = crashRecoverForward;
    }

    //Actualiza el tiempo total que ha corrido un jugador y la interfaz que lo muestra
    public void ChangeTotalTime(PlayerInfo player, float totalTime)
    {
        if (this.ID == player.ID) player.totalTime = totalTime;

        if(this.isLocalPlayer)
            uiManager.UpdateTime(this.currentLapTime, this.totalTime);
    }

    //Actualiza el tiempo que ha corrido un jugador en la vuelta actual y la interfaz que lo muestra
    public void ChangeLapTime(PlayerInfo player, float lapTime)
    {
        if (this.ID == player.ID) player.currentLapTime = lapTime;

        if (this.isLocalPlayer)
            uiManager.UpdateTime(this.currentLapTime, this.totalTime);
    }
    #endregion
}