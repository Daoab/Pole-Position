using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerLobby : NetworkBehaviour
{
    [Header("Mirror")]
    NetworkManagerPolePosition networkManager;
    PolePositionManager polePositionManager;

    [Header("UI")]
    InputField inputField;
    Button nextButton;
    Button readyButton;
    UIManager uIManager;
    Button goButton;

    [Header("Player Scripts")]
    PlayerInfo playerInfo;

    public static event Action<PlayerInfo, string> OnMessage;

    public static event Action<PlayerInfo, string> OnName;
    public static event Action<PlayerInfo, bool> OnIsReady;
    public static event Action<PlayerInfo, Color> OnColor;
    public static event Action<PlayerInfo, bool> OnIsLeader;

    public void Start()
    {
        playerInfo = GetComponent<PlayerInfo>();
        uIManager = FindObjectOfType<UIManager>();
        polePositionManager = FindObjectOfType<PolePositionManager>();
        networkManager = FindObjectOfType<NetworkManagerPolePosition>();

        //Se asigna como líder al jugador que entre primero a la partida
        //El líder puede acceder a los ajustes de la carrera e iniciar la misma
        CmdChangeIsLeader(playerInfo.isLeader);

        if (isLocalPlayer)
        {
            GetUIReferences();
        }
    }

    //Se asocian listeners a los elementos de la interfaz y se obtienen referencias necesarias
    private void GetUIReferences()
    {
        inputField = uIManager.GetUsernameUIInputField();

        nextButton = uIManager.GetUsernameNextButton();

        nextButton.onClick.AddListener(() => CmdChangePlayerName(inputField.text));
        nextButton.onClick.AddListener(() => uIManager.ActivateLobbyWindow());
        nextButton.onClick.AddListener(() => ActivateRaceSettings());

        readyButton = uIManager.GetReadyButton();
        readyButton.onClick.AddListener(() => CmdChangePlayerReady());

        //Se recorren cada uno de los botones de cambio de color,
        //y se le asocia un listener que cambia el color del coche mostrado en el menú
        Button[] colorChangeButtons = uIManager.GetColorChangeButtons().GetComponentsInChildren<Button>();

        foreach (Button colorButton in colorChangeButtons)
        {
            Color color = colorButton.GetComponent<ColorChangeButton>().GetButtonColor();
            colorButton.onClick.AddListener(() =>CmdChangePlayerColor(color));
        }

        goButton = uIManager.GetGoButtonReference();
    }

    //Se muestran los ajustos de la carrera al jugador host en su máquina
    private void ActivateRaceSettings()
    {
        if (isLocalPlayer && playerInfo.isLeader)
        {
            uIManager.ActivateRaceSettings();
        }
    }

    //El jugador del lobby coloca su coche en la pista
    public void InstantiateCar()
    {
        GetComponent<SetupPlayer>().PlaceCar();
    }

    #region Commands y Rpcs Chat
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
        OnMessage?.Invoke(playerInfo, message);
    }
    #endregion

    #region Commands PlayerInfo data
    //Command y Rpc para cambiar isReady
    [Command]
    public void CmdChangePlayerReady()
    {
        RpcChangePlayerReady(!playerInfo.isReady);
    }

    [ClientRpc]
    public void RpcChangePlayerReady(bool isReady)
    {
        OnIsReady?.Invoke(playerInfo, isReady);
    }

    //Command y Rpc para cambiar nombre
    [Command]
    public void CmdChangePlayerName(string name)
    {
        RpcChangePlayerName(name);
    }

    [ClientRpc]
    public void RpcChangePlayerName(string name)
    {
        OnName?.Invoke(playerInfo, name);
    }

    //Command y Rpc para cambiar isLeader
    [Command]
    public void CmdChangeIsLeader(bool isLeader)
    {
        RpcChangeIsLeader(isLeader);
    }

    public void RpcChangeIsLeader(bool isLeader)
    {
        OnIsLeader?.Invoke(playerInfo, isLeader);
    }

    //Command y Rpc para cambiar Color
    [Command]
    public void CmdChangePlayerColor(Color color)
    {
        RpcChangePlayerColor(color);
    }

    [ClientRpc]
    public void RpcChangePlayerColor(Color color)
    {
        OnColor?.Invoke(playerInfo, color);
    }
    #endregion

     //Se muestra al jugador host el botón de iniciar la partida cuando la mayoría de jugadores están listos
    public void UpdateGoButtonState(bool allPlayersReady)
    {
        if (isLocalPlayer && hasAuthority)
        {
            if (allPlayersReady)
            {
                goButton.gameObject.SetActive(true);

                goButton.onClick.AddListener(() => CmdStartRace());
            }
            else
            {
                goButton.onClick.RemoveAllListeners();

                goButton.gameObject.SetActive(false);
            }
        }
    }

    //Command y RPC para indicar a todos los jugadores que comiencen la carrera (colocan los coches y esperan a la cuenta atrás)
    [Command]
    public void CmdStartRace()
    {
        polePositionManager.RpcStartRace();
    }

    //Command y RPc para indicar a todos los jugadores que actualicen la interfaz del orden de la carrera
    [Command]
    public void CmdUpdateUI()
    {
        RpcUpdateUI();
    }

    [ClientRpc]
    public void RpcUpdateUI()
    {
        polePositionManager.UpdatePlayerListUI();
    }
}