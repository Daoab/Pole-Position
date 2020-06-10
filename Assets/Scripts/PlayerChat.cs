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
    [SyncVar] [SerializeField] Color playerColor;

    ChatNetworkBehaviour chatNetworkBehaviour;

    InputField inputField;
    Button nextButton;
    Button readyButton;

    GameObject usernameUI;
    GameObject ingameUI;
    GameObject chatUI;
    GameObject colorChangeUI;
    GameObject playerListUI;

    public static event Action<PlayerChat, string> OnMessage;

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    public string GetPlayerName()
    {
        return this.playerName;
    }

    public void SetPlayerColor(Color color)
    {
        this.playerColor = color;
    }

    public Color GetPlayerColor()
    {
        return this.playerColor;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        id = connectionToClient.connectionId;
    }

    private void GetUIReferences()
    {
        UIManager uIManager = FindObjectOfType<UIManager>();

        usernameUI = uIManager.GetUsernameUIReference();
        ingameUI = uIManager.GetInGameUIReference();
        chatUI = uIManager.GetChatReference();

        inputField = uIManager.GetUsernameUIInputField();
        nextButton = uIManager.GetUsernameNextButton();

        readyButton = uIManager.GetReadyButton();

        inputField.onEndEdit.AddListener((text) => CmdCheckPlayerName(inputField.text));
        nextButton.onClick.AddListener(() => ActivateLobbyWindow());
        readyButton.onClick.AddListener(() => CmdCheckPlayersReady());

        colorChangeUI = uIManager.GetColorChangeButtons();
        Button[] colorChangeButtons = uIManager.GetColorChangeButtons().GetComponentsInChildren<Button>();

        foreach (Button colorButton in colorChangeButtons)
        {
            Color color = colorButton.GetComponent<ColorChangeButton>().GetButtonColor();
            colorButton.onClick.AddListener(() => SetPlayerColor(color));
            colorButton.onClick.AddListener(() => uIManager.UpdateCarPreviewColor(playerColor));
        }

        playerListUI = uIManager.GetPlayerListUI();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        chatNetworkBehaviour.RemovePlayerName(playerName);
    }

    public void Start()
    {
        if (isLocalPlayer)
        {
            GetUIReferences();
        }

        Debug.Log(usernameUI);

        chatNetworkBehaviour = FindObjectOfType<ChatNetworkBehaviour>();
    }

    public void ActivateLobbyWindow()
    {
        usernameUI.SetActive(false);
        colorChangeUI.SetActive(true);
        chatUI.SetActive(true);
        playerListUI.SetActive(true);
        readyButton.gameObject.SetActive(true);
    }

    [Command]
    private void CmdCheckPlayerName(string playerName)
    {
        isLeader = chatNetworkBehaviour.CheckLeader();

        string checkedPlayerName = playerName;

        if (chatNetworkBehaviour.ContainsPlayerName(playerName))
        {
            checkedPlayerName = playerName + "_" + id.ToString();
            SetPlayerName(checkedPlayerName);
        }
        else
        {
            SetPlayerName(checkedPlayerName);
        }

        this.playerName = checkedPlayerName;

        chatNetworkBehaviour.AddPlayerName(this.playerName);
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
        //chatNetworkBehaviour.RpcUpdateCheckReadyList(playerName, isReady);
    }

    private void OnDestroy()
    {
        //chatNetworkBehaviour.RemovePlayerName(this.playerName);
    }

    private void NotifyPlayerNameChange(string oldValue, string newValue)
    {
        chatNetworkBehaviour.RpcUpdatePlayerListUI();
    }

    private void NotifyIsReadyChange(bool oldValue, bool newValue)
    {
        chatNetworkBehaviour.RpcUpdateCheckReadyList(this.playerName, isReady);
    }

    //IMPORTANTE: Instanciar el prefab del coche con el nombre y el color introducidos
}