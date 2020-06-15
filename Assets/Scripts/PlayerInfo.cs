using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[System.Serializable]
public class PlayerInfo : NetworkBehaviour
{
    [SyncVar] public string Name;

    [SyncVar] public int ID;

    [SyncVar] public int CurrentPosition;

    [SyncVar] public int CurrentLap;

    [SyncVar] public Color color;

    [SyncVar] public float distanceTravelled = 0f;

    [SyncVar] public bool goingBackwards = false;

    [SyncVar] public bool raceEnded = false;

    //Posición a la que se recuperará el jugador si choca
    [SyncVar] public Vector3 lastSafePosition;

    //Dirección a la que mirará el jugador si choca
    [SyncVar] public Vector3 crashRecoverForward;

    UIManager uIManager;

    private void Start()
    {
        uIManager = FindObjectOfType<UIManager>();
    }

    public override string ToString()
    {
        return Name;
    }

    #region Commands
    [Command]
    public void CmdAddLap()
    {
        this.CurrentLap++;
        uIManager.UpdateLapProgress(this);
    }

    [Command]
    public void CmdUpdateGoingBackwards(bool goingBackwards)
    {
        this.goingBackwards = goingBackwards;
        if (isLocalPlayer)
        {
            uIManager.UpdateTurnBack(this.goingBackwards);
        }
    }

    [Command]
    public void CmdUpdateCrashInfo(Vector3 lastSafePosition, Vector3 crashRecoverForward)
    {
        this.lastSafePosition = lastSafePosition;
        this.crashRecoverForward = crashRecoverForward;
    }

    [Command]
    public void CmdRaceEnded(bool raceEnded)
    {
        this.raceEnded = raceEnded;
    }

    [Command]
    public void CmdUpdateRacePosition(int currentPosition)
    {
        this.CurrentPosition = currentPosition;
    }
    #endregion
}