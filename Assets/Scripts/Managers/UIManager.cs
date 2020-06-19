using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool showGUI = true;
    private NetworkManagerPolePosition m_NetworkManager;

    #region UIReferences
    [Header("Main Menu")] [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private InputField inputFieldIP;

    [Header("Username")]
    [SerializeField] private GameObject userNameUI;
    [SerializeField] private InputField userNameInputField;
    [SerializeField] private Button userNameNextButton;

    [Header("Chat UI")]
    [SerializeField] private GameObject chatUI;

    [Header("Player List UI")]
    [SerializeField] Text[] playerNamesList;
    [SerializeField] Image[] playerReadyImage;
    [SerializeField] string defaultText = "Waiting...";
    [SerializeField] Color playerReadyColor = Color.green;
    [SerializeField] Color playerNotReadyColor = Color.red;

    [Header("Race Settings")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button goButton;
    [SerializeField] private GameObject lapsUI;
    [SerializeField] private Text lapsSettings;
    [SerializeField] private Button addButton;
    [SerializeField] private Button minusButton;

    [Header("In-Game HUD")] 
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
    [SerializeField] private Text textTime;
    [SerializeField] private Image turnBack;
    [SerializeField] private GameObject countdownText;
    [Space]

    [Header("End Race HUD")]
    [SerializeField] private GameObject endGameHUD;
    [SerializeField] private Text resultText;
    [SerializeField] private Text[] clasificationText;
    [SerializeField] private Camera endGameCamera;
    [Space]

    [SerializeField] Text DebugText;

    [SerializeField] GameObject playerListUI;
    [Space]
    [SerializeField] private GameObject colorChangeButtons;
    [Space]
    [SerializeField] private GameObject carBody;
    #endregion

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        ActivateMainMenu();
    }

    #region Setup Mirror
    private void StartHost()
    {
        m_NetworkManager = FindObjectOfType<NetworkManagerPolePosition>();

        m_NetworkManager.StartHost();
        ActivateUsernameUI();
    }

    private void StartClient()
    {
        m_NetworkManager = FindObjectOfType<NetworkManagerPolePosition>();

        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP.text;
        ActivateUsernameUI();
    }

    private void StartServer()
    {
        m_NetworkManager = FindObjectOfType<NetworkManagerPolePosition>();

        m_NetworkManager = FindObjectOfType<NetworkManagerPolePosition>();
        m_NetworkManager.StartServer();
        ActivateUsernameUI();
    }
    #endregion

    #region Activate/Deactivate UI
    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        userNameUI.SetActive(false);
        chatUI.SetActive(false);
    }

    private void ActivateUsernameUI()
    {
        mainMenu.SetActive(false);
        userNameUI.SetActive(true);
    }

    public void ActivateLobbyWindow()
    {
        userNameUI.SetActive(false);
        colorChangeButtons.SetActive(true);
        chatUI.SetActive(true);
        playerListUI.SetActive(true);
        readyButton.gameObject.SetActive(true);
    }

    public void ActivateRaceSettings()
    {
        lapsUI.gameObject.SetActive(true);
    }

    public void ActivateRaceUI()
    {
        mainMenu.SetActive(false);

        chatUI.SetActive(false);
        playerListUI.SetActive(false);
        colorChangeButtons.SetActive(false);
        lapsUI.SetActive(false);
        readyButton.gameObject.SetActive(false);
        goButton.gameObject.SetActive(false);

        inGameHUD.SetActive(true);
    }

    public void ActivateCountdown()
    {
        countdownText.SetActive(true);
        countdownText.GetComponent<Countdown>().StartCountdown();
    }

    public void ActivateEndGameHUD(List<PlayerInfo> players)
    {
        inGameHUD.SetActive(false);

        Camera.main.gameObject.SetActive(false);
        endGameCamera.gameObject.SetActive(true);
        endGameHUD.SetActive(true);

        string aux = "";

        for(int i = 0; i < players.Count; i++)
        {
            if(players[i].isLocalPlayer && players[i].CurrentPosition == 1)
                resultText.text = "YOU WIN!";

            aux = players[i].CurrentPosition + " — " + players[i].Name + " — ";

            //Si un jugador no ha terminado la carrear se muestra DNF (did not finish) en lugar del tiempo
            if (players[i].raceEnded)
                 aux += players[i].totalTime.ToString("0.00");
             else
                 aux += "DNF";

            clasificationText[players[i].CurrentPosition - 1].text = aux;
        }
    }
    #endregion

    #region Update UI
    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h";
    }

    //Actualizar le color del coche que aparece en el menú prinicipal
    public void UpdateCarPreviewColor(Color color)
    {
        Material carMaterial = carBody.GetComponent<MeshRenderer>().materials[1];
        carMaterial.color = color;
    }

    //Actualizar lista de jugadores en el lobby
    public void UpdatePlayerListUI(List<PlayerInfo> players)
    {
        for (int i = 0; i < playerNamesList.Length; i++)
        {
            playerNamesList[i].text = defaultText;
            playerReadyImage[i].color = playerNotReadyColor;
        }

        for (int i = 0; i < players.Count; i++)
        {
            playerNamesList[i].text = players[i].Name;
            if (players[i].isReady)
                playerReadyImage[i].color = playerReadyColor;
        }
    }

    //Actualizar la interfaz de ajustes de la carrera
    public void UpdateLapsSettingsUI(int numLaps)
    {
        lapsSettings.text = "Laps: " + numLaps.ToString();
    }

    //Actualizar orden de los jugadores durante la carrera
    public void UpdateRaceProgress(List<PlayerInfo> players)
    {
        textPosition.text = "";

        foreach (PlayerInfo p in players)
        {
            textPosition.text += p.CurrentPosition + " — " + p.Name + "\n";
        }
    }

    //Actualizar el contador de vueltas del jugador
    public void UpdateLapProgress(int lap)
    {
        textLaps.text = lap.ToString() + "/" + FindObjectOfType<PolePositionManager>().numLaps.ToString();
    }

    //Actualizar tiempo total y de cada vuelta
    public void UpdateTime(float currentTime, float totalTime)
    {
        textTime.text = "Total time: \n" + totalTime.ToString("0.00") + "\nCurrent time: \n" + currentTime.ToString("0.00");
    }

    //Actualizar el indicar de si un jugador va marcha atrás
    public void UpdateTurnBack(bool goingBackwards)
    {
        turnBack.gameObject.SetActive(goingBackwards);
    }
    #endregion

    //Se necesitan getters de los elementos de la interfaz en caso de que se quiera obtener alguno,
    //ya que Unity no devuelve objetos desactivados cuando son buscados
    #region Getters UI
    public GameObject GetChatReference()
    {
        return chatUI;
    }

    public GameObject GetUsernameUIReference()
    {
        return userNameUI;
    }

    public Button GetUsernameNextButton()
    {
        return userNameNextButton;
    }

    public Button GetReadyButton()
    {
        return readyButton;
    }

    public InputField GetUsernameUIInputField()
    {
        return userNameInputField;
    }

    public GameObject GetInGameUIReference()
    {
        return inGameHUD;
    }

    public GameObject GetColorChangeButtons()
    {
        return colorChangeButtons;
    }

    public GameObject GetPlayerListUI()
    {
        return playerListUI;
    }
    
    public Button GetGoButtonReference()
    {
        return goButton;
    }

    public Button GetMinusButton()
    {
        return minusButton;
    }

    public Button GetAddButton()
    {
        return addButton;
    }

    public Text GetDebugText()
    {
        return DebugText;
    }
    #endregion
}