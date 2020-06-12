using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerLobby : NetworkBehaviour
{
    [SyncVar] public string playerName = "";
    [SyncVar] public int id = 0;
    [SyncVar] public bool isReady = false;

    [SyncVar] public bool isLeader = false;
    [SerializeField] public Color playerColor;
    SyncListFloat colorList = new SyncListFloat();

    LobbyNetworkBehaviour lobbyNetworkBehaviour;

    InputField inputField;
    Button nextButton;
    Button readyButton;

    UIManager uIManager;

    public static event Action<PlayerLobby, string> OnMessage;

    #region Setup Mirror
    public override void OnStartServer()
    {
        base.OnStartServer();
        id = connectionToClient.connectionId;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        lobbyNetworkBehaviour.RemovePlayer(id);
    }
    #endregion

    public void Start()
    {
        if (isLocalPlayer)
        {
            GetUIReferences();
        }

        colorList.Callback += UpdatePlayerColor;
        lobbyNetworkBehaviour = FindObjectOfType<LobbyNetworkBehaviour>();
    }

    //Se asocian listeners a los elementos de la interfaz
    private void GetUIReferences()
    {
        uIManager = FindObjectOfType<UIManager>();

        inputField = uIManager.GetUsernameUIInputField();

        nextButton = uIManager.GetUsernameNextButton();

        nextButton.onClick.AddListener(() => CmdCheckPlayerName(inputField.text));
        nextButton.onClick.AddListener(() => uIManager.ActivateLobbyWindow());
        nextButton.onClick.AddListener(() => ActivateRaceSettings());

        readyButton = uIManager.GetReadyButton();
        readyButton.onClick.AddListener(() => CmdCheckPlayersReady());

        //Se recorren cada uno de los botones de cambio de color,
        //y se le asocia un listener que cambia el color del coche mostrado en el menú
        Button[] colorChangeButtons = uIManager.GetColorChangeButtons().GetComponentsInChildren<Button>();

        foreach (Button colorButton in colorChangeButtons)
        {
            float[] color = colorButton.GetComponent<ColorChangeButton>().GetButtonColor();

            colorButton.onClick.AddListener(() => CmdSetPlayerColor(color));
        }
    }

    //Se muestran los ajustos de la carrera al jugador host en su máquina
    private void ActivateRaceSettings()
    {
        if (isLocalPlayer && isLeader)
        {
            uIManager.ActivateRaceSettings();
        }
    }

    public void InstantiateCar()
    {
        //Crear coche

        //Modificar parámetros

        //Llamar a networkmanager para que instancie coche
    }

    #region Commands y RPCs
    [Command]

    //Se comprueba en el servidor si el nombre que el jugador ha introducido está disponible.
    //Si no está disponible, se coge ese mismo nombre, y se le añade el id del jugador al final
    private void CmdCheckPlayerName(string playerName)
    {
        isLeader = lobbyNetworkBehaviour.CheckLeader();

        string checkedPlayerName = playerName;

        if (lobbyNetworkBehaviour.ContainsPlayerName(playerName) || playerName.Trim() == "")
        {
            checkedPlayerName = playerName + "_" + id.ToString();
        }

        this.playerName = checkedPlayerName;

        lobbyNetworkBehaviour.AddPlayer(this);
    }

    //Cuando se pulsa el botón de send en el chat, se manda el mensaje al servidor para procesarlo
    [Command]
    public void CmdSend(string message)
    {
        if (message.Trim() != "")
            RpcReceive(message.Trim());
    }

    //Cuando el mensaje del chat se ha procesado, se llama al Selegate On Message de los jugadores
    //para mostrar el mensaje procesado por el chat
    [ClientRpc]
    public void RpcReceive(string message)
    {
        OnMessage?.Invoke(this, message);
    }

    //Cuando se pulsa el botón de ready, se cambia el estado de ready del jugador, y se actualiza en el LobbyNetworkBehaviour
    [Command]
    public void CmdCheckPlayersReady()
    {
        isReady = !isReady;
        lobbyNetworkBehaviour.UpdatePlayerIsReady(id, isReady);
    }

    //Para la correcta sincronización del color entre los jugadores, se guarda en una SyncList de floats,
    //ya que el propio color que ofrece Unity no está soportado
    [Command]
    public void CmdSetPlayerColor(float[] c)
    {
        colorList.Clear();

        for (int i = 0; i < c.Length; i++)
        {
            colorList.Add(c[i]);
        }
    }
    #endregion

    #region Hooks

    //Se llama cuando se modifica la SyncList de floats que representa el color de los jugadores.
    //Su función es actualizar la información de color en el LobbyNetworkBehaviour, 
    //y actualiza también el color del coche mostrado en el menú.
    private void UpdatePlayerColor(SyncListFloat.Operation op, int index, float oldValue, float newValue)
    {
        playerColor[index] = newValue;

        // Si se ha terminado de sincronizar el color del jugador actualiza el modelo del coche
        if (index == 3 && isLocalPlayer)
        {
            lobbyNetworkBehaviour.UpdatePlayerColor(id, playerColor);
            uIManager.UpdateCarPreviewColor(playerColor);
        }
    }

    //Se muestra al jugador host el botón de iniciar la partida cuando la mayoría de jugadores están listos
    public void UpdateGoButtonState(bool allPlayersReady)
    {
        if (isLocalPlayer && hasAuthority)
        {
            if (allPlayersReady)
            {
                uIManager.ActivateGoButton();
            }
            else
            {
                uIManager.DeactivateGoButton();
            }
        }
    }
    #endregion

    //IMPORTANTE: Instanciar el prefab del coche con el nombre y el color introducidos
}