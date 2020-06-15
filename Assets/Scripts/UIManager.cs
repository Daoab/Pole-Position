using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool showGUI = true;

    [SerializeField] Text DebugText;

    private NetworkManagerPolePosition m_NetworkManager;
    private RaceNetworkBehaviour raceNetWorkBehaviour;

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

    [Header("Race Settings")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button goButton;
    [SerializeField] private GameObject lapsUI;

    [Header("In-Game HUD")] 
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
    [SerializeField] private Text textTime;
    [SerializeField] private Image turnBack;
    [Space]

    [SerializeField] GameObject playerListUI;
    [Space]

    [SerializeField] private GameObject colorChangeButtons;
    [Space]

    [SerializeField] private GameObject carBody;
    #endregion

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<NetworkManagerPolePosition>();
        raceNetWorkBehaviour = FindObjectOfType<RaceNetworkBehaviour>();
    }

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
        m_NetworkManager.StartHost();
        //ActivateUsernameUI();
        ActivateRaceUI();
    }

    private void StartClient()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP.text;
        //ActivateUsernameUI();
        ActivateRaceUI();
    }

    private void StartServer()
    {
        m_NetworkManager.StartServer();
        //ActivateUsernameUI();
        ActivateRaceUI();
    }
    #endregion

    #region Activate/Deactivate UI
    public void ActivateLobbyWindow()
    {
        userNameUI.SetActive(false);
        colorChangeButtons.SetActive(true);
        chatUI.SetActive(true);
        playerListUI.SetActive(true);
        readyButton.gameObject.SetActive(true);
    }

    private void ActivateUsernameUI()
    {
        mainMenu.SetActive(false);
        userNameUI.SetActive(true);
    }

    public void ActivateRaceSettings()
    {
        lapsUI.gameObject.SetActive(true);
    }

    public void ActivateGoButton()
    {
        goButton.gameObject.SetActive(true);

        goButton.onClick.AddListener(() => raceNetWorkBehaviour.StartRace());
    }

    public void DeactivateGoButton()
    {
        goButton.onClick.RemoveAllListeners();

        goButton.gameObject.SetActive(false);
    }

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        userNameUI.SetActive(false);
        chatUI.SetActive(false);
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
    #endregion

    #region Update UI
    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h";
    }

    public void UpdateLapsCounter(int numLaps)
    {
        lapsUI.GetComponentInChildren<Text>().text = "Laps: " + numLaps.ToString();
    }

    public void UpdateCarPreviewColor(Color color)
    {
        Material carMaterial = carBody.GetComponent<MeshRenderer>().materials[1];
        carMaterial.color = color;
    }

    public void UpdateRaceProgress(List<PlayerInfo> players)
    {
        textPosition.text = "";

        foreach(PlayerInfo p in players)
        {
            Debug.Log(p.Name + " " + p.CurrentPosition);
            textPosition.text += p.CurrentPosition + " — " + p.Name + "\n";
        }
    }

    public void UpdateLapProgress(PlayerInfo player)
    {
        textLaps.text = "LAP: " + player.CurrentLap + "/" + raceNetWorkBehaviour.numLaps;
    }

    public void UpdateTime(float currentTime, float totalTime)
    {
        textTime.text = "Total time: \n" + totalTime.ToString("0.00") + "\nCurrent time: \n" + currentTime.ToString("0.00");
    }

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
    
    public Text GetDebugText()
    {
        return DebugText;
    }

    #endregion
}