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

    ChatNetworkBehaviour chatNetworkBehaviour;

    InputField inputField;
    Button nextButton;

    GameObject usernameUI;
    GameObject ingameUI;
    GameObject chatUI;

    public static event Action<PlayerChat, string> OnMessage;

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    public string GetPlayerName()
    {
        return this.playerName;
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
        }

        chatNetworkBehaviour = FindObjectOfType<ChatNetworkBehaviour>();
    }

    public void ActivateChatWindow()
    {
        usernameUI.SetActive(false);
        chatUI.SetActive(true);
    }

    [Command]
    private void CmdCheckPlayerName(string playerName)
    {
        string checkedPlayerName = playerName;

        Debug.Log(chatNetworkBehaviour.ContainsPlayerName(playerName));
        if (chatNetworkBehaviour.ContainsPlayerName(playerName))
        {
            checkedPlayerName = playerName + "_" + id.ToString();
            SetPlayerName(checkedPlayerName);
        }
        else
        {
            SetPlayerName(checkedPlayerName);
        }

        Debug.Log(checkedPlayerName);
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
}