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
        PlayerLobby.OnMessage += OnPlayerMessage;
    }

    //Muestra por el chat el mensaje que han mandado los jugadores, y cambia el nombre del jugador en función de si es el local o no
    void OnPlayerMessage(PlayerInfo player, string message)
    {
        string prettyMessage = player.isLocalPlayer ?
            $"<color=red>{player.Name}: </color> {message}" :
            $"<color=blue>{player.Name}: </color> {message}";

        AppendMessage(prettyMessage);
    }

    //Este método envía el mensaje escrito en el input field al pulsar el botón de send
    public void OnSend()
    {
        if (chatMessage.text.Trim() == "")
            return;

        // get our player
        PlayerLobby player = NetworkClient.connection.identity.GetComponent<PlayerLobby>();

        // send a message
        player.CmdSend(chatMessage.text.Trim());

        chatMessage.text = "";
    }

    //Unir el mensaje al resto de mensajes del chat
    internal void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
        chatHistory.text += message + "\n";

        //La UI necesita dos frames para refrescarse y poder usar la barra vertical
        yield return null;
        yield return null;

        //Bajar la barra vertical
        scrollbar.value = 0;
    }
}
