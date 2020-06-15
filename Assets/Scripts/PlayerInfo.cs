using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

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

    public override string ToString()
    {
        return Name;
    }

    [Command]
    public void CmdAddLap()
    {
        this.CurrentLap++;
    }

    [Command]
    public void CmdUpdateGoingBackwards(bool goingBackwards)
    {
        this.goingBackwards = goingBackwards;
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
}