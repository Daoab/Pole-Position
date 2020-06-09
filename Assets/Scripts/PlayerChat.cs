using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerChat : NetworkBehaviour
{
    [SyncVar] public string playerName = "";
    [SyncVar] public int id = 0;
    [SyncVar] public bool isReady = false;
    [SyncVar] [SerializeField] Color playerColor;

    ChatNetworkBehaviour chatNetworkBehaviour;

    InputField inputField;
    Button nextButton;

    GameObject usernameUI;
    GameObject ingameUI;
    GameObject chatUI;
    GameObject colorChangeUI;
    Button[] colorChangeButtons;

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

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isLocalPlayer)
        {
            UIManager uIManager = FindObjectOfType<UIManager>();

            usernameUI = uIManager.GetUsernameUIReference();
            ingameUI = uIManager.GetInGameUIReference();
            chatUI = uIManager.GetChatReference();

            inputField = uIManager.GetUsernameUIInputField();
            nextButton = uIManager.GetUsernameNextButton();

            inputField.onEndEdit.AddListener((text) => CmdCheckPlayerName(inputField.text));
            nextButton.onClick.AddListener(() => ActivateChatWindow());

            colorChangeUI = uIManager.GetColorChangeButtons();
            colorChangeButtons = uIManager.GetColorChangeButtons().GetComponentsInChildren<Button>();

            foreach(Button colorButton in colorChangeButtons)
            {
                Color color = colorButton.GetComponent<ColorChangeButton>().GetButtonColor();
                colorButton.onClick.AddListener(() => SetPlayerColor(color));
                colorButton.onClick.AddListener(() => uIManager.UpdateCarPreviewColor(playerColor));
            }
        }

        chatNetworkBehaviour = FindObjectOfType<ChatNetworkBehaviour>();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        chatNetworkBehaviour.RemovePlayerName(playerName);
    }

 

    public void ActivateChatWindow()
    {
        usernameUI.SetActive(false);
        colorChangeUI.SetActive(true);
        chatUI.SetActive(true);
    }

    [Command]
    private void CmdCheckPlayerName(string playerName)
    {
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

        chatNetworkBehaviour.AddPlayerName(checkedPlayerName);
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

    //IMPORTANTE: Instanciar el prefab del coche con el nombre y el color introducidos
}