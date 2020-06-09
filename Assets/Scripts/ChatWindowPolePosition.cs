using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ChatWindowPolePosition : MonoBehaviour
{
    public InputField chatMessage;
    public Text chatHistory;
    public Scrollbar scrollbar;

    public void Awake()
    {
        PlayerChat.OnMessage += OnPlayerMessage;
    }

    void OnPlayerMessage(PlayerChat player, string message)
    {
        string prettyMessage = player.isLocalPlayer ?
            $"<color=red>{player.playerName}: </color> {message}" :
            $"<color=blue>{player.playerName}: </color> {message}";

        AppendMessage(prettyMessage);
    }

    public void OnSend()
    {
        if (chatMessage.text.Trim() == "")
            return;

        // get our player
        PlayerChat player = NetworkClient.connection.identity.GetComponent<PlayerChat>();

        // send a message
        player.CmdSend(chatMessage.text.Trim());

        chatMessage.text = "";
    }

    internal void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
        chatHistory.text += message + "\n";

        // it takes 2 frames for the UI to update ?!?!
        yield return null;
        yield return null;

        // slam the scrollbar down
        scrollbar.value = 0;
    }
}
