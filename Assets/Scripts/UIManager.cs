using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
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

    [Header("Race Settings")]
    [SerializeField] private Button readyButton;

    [Header("In-Game HUD")] 
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
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
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        ActivateMainMenu();
    }

    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h";
    }

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        userNameUI.SetActive(false);
        chatUI.SetActive(false);
    }

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
        //inGameHUD.SetActive(true);
        //chatUI.SetActive(false);
        userNameUI.SetActive(true);
    }

    private void StartHost()
    {
        m_NetworkManager.StartHost();
        ActivateUsernameUI();
    }

    private void StartClient()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP.text;
        ActivateUsernameUI();
    }

    private void StartServer()
    {
        m_NetworkManager.StartServer();
        ActivateUsernameUI();
    }

    public void UpdateCarPreviewColor(Color color)
    {
        Material carMaterial = carBody.GetComponent<MeshRenderer>().materials[1];
        carMaterial.color = color;
    }

    #region UIReferences
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
    #endregion
}