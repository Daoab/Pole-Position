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
    [SerializeField] Color playerColor;
    SyncListFloat colorList = new SyncListFloat();

    LobbyNetworkBehaviour lobbyNetworkBehaviour;

    InputField inputField;
    Button nextButton;
    Button readyButton;

    UIManager uIManager;
    GameObject usernameUI;
    GameObject ingameUI;
    GameObject chatUI;
    GameObject colorChangeUI;
    GameObject playerListUI;

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

    private void GetUIReferences()
    {
        uIManager = FindObjectOfType<UIManager>();

        usernameUI = uIManager.GetUsernameUIReference();
        ingameUI = uIManager.GetInGameUIReference();
        chatUI = uIManager.GetChatReference();

        inputField = uIManager.GetUsernameUIInputField();
        nextButton = uIManager.GetUsernameNextButton();

        readyButton = uIManager.GetReadyButton();

        nextButton.onClick.AddListener(() => CmdCheckPlayerName(inputField.text));
        nextButton.onClick.AddListener(() => uIManager.ActivateLobbyWindow());

        readyButton.onClick.AddListener(() => CmdCheckPlayersReady());

        colorChangeUI = uIManager.GetColorChangeButtons();
        Button[] colorChangeButtons = uIManager.GetColorChangeButtons().GetComponentsInChildren<Button>();

        foreach (Button colorButton in colorChangeButtons)
        {
            float[] color = colorButton.GetComponent<ColorChangeButton>().GetButtonColor();

            colorButton.onClick.AddListener(() => CmdSetPlayerColor(color));
        }

        playerListUI = uIManager.GetPlayerListUI();
    }

    #region Commands y RPCs
    [Command]
    private void CmdCheckPlayerName(string playerName)
    {
        isLeader = lobbyNetworkBehaviour.CheckLeader();

        string checkedPlayerName = playerName;

        if (lobbyNetworkBehaviour.ContainsPlayerName(playerName) || playerName.Trim() == "")
        {
            checkedPlayerName = playerName + "_" + id.ToString();
        }

        this.playerName = checkedPlayerName;

        lobbyNetworkBehaviour.AddPlayer(id, this.playerName, isReady, isLeader, playerColor);
    }

    [Command]
    public void CmdSend(string message)
    {
        if (message.Trim() != "")
            RpcReceive(message.Trim());
    }

    [ClientRpc]
    public void RpcReceive(string message)
    {
        OnMessage?.Invoke(this, message);
    }

    [Command]
    public void CmdCheckPlayersReady()
    {
        isReady = !isReady;
        lobbyNetworkBehaviour.UpdatePlayerIsReady(id, isReady);
    }

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
    #endregion

    //IMPORTANTE: Instanciar el prefab del coche con el nombre y el color introducidos
}