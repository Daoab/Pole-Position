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
}