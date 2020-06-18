using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class SetupPlayer : NetworkBehaviour
{
    [SyncVar] private int m_ID;
    [SyncVar] private string m_Name;

    [SerializeField] Text debug;
    [SerializeField] GameObject updateRaceOrderTrigger;

    private UIManager m_UIManager;
    private NetworkManagerPolePosition m_NetworkManager;
    private PlayerController m_PlayerController;
    private PlayerInfo m_PlayerInfo;
    private PolePositionManager m_PolePositionManager;

    [SerializeField] MeshRenderer carBody;
    [SerializeField] GameObject playerCar;

    private bool carStarted = false;

    public static event Action<PlayerInfo, int> OnCurrentPosition;
    public static event Action<PlayerInfo, int> OnCurrentLap;
    public static event Action<PlayerInfo, float> OnDistanceTravelled;
    public static event Action<PlayerInfo, bool> OnGoingBackwards;
    public static event Action<PlayerInfo, bool> OnRaceEnded;
    public static event Action<PlayerInfo, Vector3> OnLastSafePosition;
    public static event Action<PlayerInfo, Vector3> OnCrashRecoverForward;
    public static event Action<PlayerInfo> OnUpdateListUI;

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        m_ID = connectionToClient.connectionId;
    }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        m_PlayerInfo.ID = m_ID;
        m_PlayerInfo.CurrentLap = 0;
        m_PlayerInfo.isLeader = m_PolePositionManager.CheckIsLeader();
        m_PolePositionManager.AddPlayer(m_PlayerInfo);
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
    }

    public override void OnStopClient()
    {
        m_PolePositionManager.RemovePlayer(m_PlayerInfo);
        base.OnStopClient();
    }

    #endregion

    private void Awake()
    {
        m_PlayerInfo = GetComponent<PlayerInfo>();
        m_PlayerController = GetComponent<PlayerController>();
        m_NetworkManager = FindObjectOfType<NetworkManagerPolePosition>();
        m_PolePositionManager = FindObjectOfType<PolePositionManager>();
        m_UIManager = FindObjectOfType<UIManager>();
        debug = m_UIManager.GetDebugText();
    }

    public void PlaceCar()
    {
        playerCar.SetActive(true);
        carBody.materials[1].color = m_PlayerInfo.color;

        if (isLocalPlayer)
        {
            ConfigureCamera();
            m_UIManager.ActivateRaceUI();
            m_PolePositionManager.UpdateRaceProgress();
            m_UIManager.UpdateLapProgress(m_PlayerInfo.CurrentLap);
            StartCoroutine(Timer());
        }
    }

    IEnumerator Timer()
    {
        m_UIManager.ActivateCountdown();
        yield return new WaitForSecondsRealtime(m_PolePositionManager.countdown);
        StartCar();
    }

    // Start is called before the first frame update
    public void StartCar()
    {
        if (isLocalPlayer)
        {
            GetComponent<RaceTimer>().enabled = true;
            carStarted = true;

            m_PlayerController.enabled = true;
            m_PlayerController.OnSpeedChangeEvent += OnSpeedChangeEventHandler;
            updateRaceOrderTrigger.SetActive(true);
        }
    }

    private void Update()
    {
        if (isLocalPlayer && carStarted)
        {
            m_PolePositionManager.UpdateRaceCarState(m_PlayerInfo, this);
        }
    }

    void OnSpeedChangeEventHandler(float speed)
    {
        m_UIManager.UpdateSpeed((int) speed * 5); // 5 for visualization purpose (km/h)
    }

    void ConfigureCamera()
    {
        if (Camera.main != null) Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
    }

    #region Race variable callbacks
    //Command y Rpc para currentPosition
    [Command]
    public void CmdChangeCurrentPosition(int currentPosition)
    {
        RpcChangeCurrentPosition(currentPosition);
    }

    [ClientRpc]
    public void RpcChangeCurrentPosition(int currentPosition)
    {
        OnCurrentPosition?.Invoke(m_PlayerInfo, currentPosition);
    }

    //Command y Rpc para CurrentLap
    [Command]
    public void CmdChangeCurrentLap(int currentLap)
    {
        RpcChangeCurrentLap(currentLap);
    }

    [ClientRpc]
    public void RpcChangeCurrentLap(int currentLap)
    {
        OnCurrentLap?.Invoke(m_PlayerInfo, currentLap);
    }

    //Command y Rpc para distanceTravelled
    [Command]
    public void CmdChangeDistanceTravelled(float distanceTravelled)
    {
        RpcChangeDistanceTravelled(distanceTravelled);
    }

    [ClientRpc]
    public void RpcChangeDistanceTravelled(float distanceTravelled)
    {
        OnDistanceTravelled?.Invoke(m_PlayerInfo, distanceTravelled);
    }

    //Command y Rpc para goingBackwards
    [Command]
    public void CmdChangeGoingBackwards(bool goingBackwards)
    {
        RpcChangeGoingBackwards(goingBackwards);
    }

    [ClientRpc]
    public void RpcChangeGoingBackwards(bool goingBackwards)
    {
        OnGoingBackwards?.Invoke(m_PlayerInfo, goingBackwards);
    }

    //Command y Rpc para raceEnded
    [Command]
    public void CmdChangeRanceEnded(bool raceEnded)
    {
        RpcChangeRaceEnded(raceEnded);
    }

    [ClientRpc]
    public void RpcChangeRaceEnded(bool raceEnded)
    {
        OnRaceEnded?.Invoke(m_PlayerInfo, raceEnded);
    }

    //Command y Rpc para lastSafePosition
    [Command]
    public void CmdChangeLastSafePosition(Vector3 lastSafePosition)
    {
        RpcChangeLastSafePosition(lastSafePosition);
    }

    [ClientRpc]
    public void RpcChangeLastSafePosition(Vector3 lastSafePosition)
    {
        OnLastSafePosition?.Invoke(m_PlayerInfo, lastSafePosition);
    }

    //Command y Rpc para crashRecoverForward
    [Command]
    public void CmdChangeCrashRecoverForward(Vector3 crashRecoverForward)
    {
        RpcChangeCrashRecoverForward(crashRecoverForward);
    }

    [ClientRpc]
    public void RpcChangeCrashRecoverForward(Vector3 crashRecoverForward)
    {
        OnCrashRecoverForward?.Invoke(m_PlayerInfo, crashRecoverForward);
    }

    //Command y Rpc para actualizar lista de jugadores
    [Command]
    public void CmdUpdateListUI()
    {
        RpcUpdateListUI();
    }

    [ClientRpc]
    public void RpcUpdateListUI()
    {
        OnUpdateListUI?.Invoke(m_PlayerInfo);
    }
    #endregion
}