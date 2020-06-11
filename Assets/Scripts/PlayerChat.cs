using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerChat : NetworkBehaviour
{
    [SyncVar(hook = nameof(NotifyPlayerNameChange))] public string playerName = "";
    [SyncVar] public int id = 0;
    [SyncVar(hook = nameof(NotifyIsReadyChange))] public bool isReady = false;

    [SyncVar] public bool isLeader = false;
    [SerializeField] Color playerColor;
    SyncListFloat colorList = new SyncListFloat();

    ChatNetworkBehaviour chatNetworkBehaviour;

    InputField inputField;
    Button nextButton;
    Button readyButton;

    UIManager uIManager;
    GameObject usernameUI;
    GameObject ingameUI;
    GameObject chatUI;
    GameObject colorChangeUI;
    GameObject playerListUI;

    public static event Action<PlayerChat, string> OnMessage;

    #region Getters y setters

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    public string GetPlayerName()
    {
        return this.playerName;
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

    public Color GetPlayerColor()
    {
        return this.playerColor;
    }
    #endregion

    #region Setup Mirror
    public override void OnStartServer()
    {
        base.OnStartServer();
        id = connectionToClient.connectionId;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        chatNetworkBehaviour.RemovePlayerName(playerName, isReady);
    }
    #endregion

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
    
    public void Start()
    {
        if (isLocalPlayer)
        {
            GetUIReferences();
        }

        colorList.Callback += UpdatePlayerColor;
        chatNetworkBehaviour = FindObjectOfType<ChatNetworkBehaviour>();
    }

    #region Commands y RPCs
    [Command]
    private void CmdCheckPlayerName(string playerName)
    {
        isLeader = chatNetworkBehaviour.CheckLeader();

        string checkedPlayerName = playerName;

        if (chatNetworkBehaviour.ContainsPlayerName(playerName) || playerName.Trim() == "")
        {
            checkedPlayerName = playerName + "_" + id.ToString();
            SetPlayerName(checkedPlayerName);
        }
        else
        {
            SetPlayerName(checkedPlayerName);
        }

        chatNetworkBehaviour.AddPlayerName(checkedPlayerName);

        this.playerName = checkedPlayerName;
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

        chatNetworkBehaviour.UpdatePlayersReady(isReady);
    }

    [Command]
    public void CmdRemovePlayerName()
    {
        chatNetworkBehaviour.RemovePlayerName(this.playerName, this.isReady);
    }

    [Command]
    public void CmdUpdatePlayerListUI()
    {
        chatNetworkBehaviour.RpcUpdatePlayerListUI();
    }

    private void NotifyPlayerNameChange(string oldValue, string newValue)
    {
        CmdUpdatePlayerListUI();
    }

    [Command]
    public void CmdUpdateUpdateIsReady()
    {
        chatNetworkBehaviour.RpcUpdateCheckReadyList(this.playerName, isReady);
    }
    #endregion

    #region Hooks
    private void NotifyIsReadyChange(bool oldValue, bool newValue)
    {
        CmdUpdateUpdateIsReady();
    }

    private void UpdatePlayerColor(SyncListFloat.Operation op, int index, float oldValue, float newValue)
    {
        this.playerColor[index] = newValue;

        // Si se ha terminado de sincronizar el color del jugador actualiza el modelo del coche
        if(index == 3 && isLocalPlayer)
            uIManager.UpdateCarPreviewColor(playerColor);
    }
#endregion

    //IMPORTANTE: Instanciar el prefab del coche con el nombre y el color introducidos
}