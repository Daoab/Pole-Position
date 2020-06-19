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

    [SerializeField] float timeGoingBackwards = 1f;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        polePositionManager = FindObjectOfType<PolePositionManager>();

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
    public void ChangePlayerIsReady(PlayerInfo player, bool isReady)
    {
        if(this.ID == player.ID) player.isReady = isReady;
        polePositionManager.UpdatePlayerListUI();
        polePositionManager.UpdateNumberOfPlayersReady();
    }

    public void ChangePlayerName(PlayerInfo player, string name)
    {
        if (this.ID == player.ID) player.Name = name;
        polePositionManager.UpdatePlayerListUI();
    }

    public void ChangeColor(PlayerInfo player, Color color)
    {
        if (this.ID == player.ID) player.color = color;

        if (player.isLocalPlayer) uiManager.UpdateCarPreviewColor(color);
    }

    public void ChangeIsLeader(PlayerInfo player, bool isLeader)
    {
        if (this.ID == player.ID) player.isLeader = isLeader;
    }
    #endregion

    #region Race Callbacks
    public void ChangeCurrentPosition(PlayerInfo player, int currentPosition)
    {
        if (this.ID == player.ID && !player.raceEnded) this.CurrentPosition = currentPosition;
    }

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

    public void ChangeDistanceTravelled(PlayerInfo player, float distanceTravelled)
    {
        if (this.ID == player.ID) this.distanceTravelled = distanceTravelled;
    }

    public void ChangeGoingBackwards(PlayerInfo player, bool goingBackwards)
    {
        if (this.ID == player.ID) this.goingBackwards = goingBackwards;

        if (this.isLocalPlayer)
            StartCoroutine(WaitChangeGoingBackwards(player, goingBackwards));
    }

    private IEnumerator WaitChangeGoingBackwards(PlayerInfo player, bool goingBackwards)
    {
        bool previousValue = goingBackwards;

        yield return new WaitForSecondsRealtime(timeGoingBackwards);

        if(this.goingBackwards == previousValue)
            uiManager.UpdateTurnBack(this.goingBackwards);
    }

    public void ChangeRaceEnded(PlayerInfo player, bool raceEnded)
    {
        if (this.ID == player.ID) this.raceEnded = raceEnded;
    }

    public void ChangeLastSafePosition(PlayerInfo player, Vector3 lastSafePosition)
    {
        if (this.ID == player.ID) this.lastSafePosition = lastSafePosition;
    }

    public void ChangeCrashRecoverForward(PlayerInfo player, Vector3 crashRecoverForward)
    {
        if (this.ID == player.ID) this.crashRecoverForward = crashRecoverForward;
    }

    public void ChangeTotalTime(PlayerInfo player, float totalTime)
    {
        if (this.ID == player.ID) player.totalTime = totalTime;

        if(this.isLocalPlayer)
            uiManager.UpdateTime(this.currentLapTime, this.totalTime);
    }

    public void ChangeLapTime(PlayerInfo player, float lapTime)
    {
        if (this.ID == player.ID) player.currentLapTime = lapTime;

        if (this.isLocalPlayer)
            uiManager.UpdateTime(this.currentLapTime, this.totalTime);
    }
    #endregion
}