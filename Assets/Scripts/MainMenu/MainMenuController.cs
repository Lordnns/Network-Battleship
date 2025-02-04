using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MainMenuController : MonoBehaviour
{

    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private AudioClip test;

    private const string LEVEL_NAME = "GameLevel";

    private void Start()
    {
        // Subscribe HandleClientConnected method to the OnClientConnectedCallback
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
        playPanel.SetActive(false);
        waitingPanel.SetActive(true);

    }

    public void JoinGame()
    {
        string ipAddress = ipAddressInputField.text; // ipAddressInputField is a reference to your UI input field for the IP address
        ushort port;
        if (!ushort.TryParse(portInputField.text, out port))
        {
            Debug.LogError("Invalid port number!");
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, port);
        NetworkManager.Singleton.StartClient();
        playPanel.SetActive(false);
        waitingPanel.SetActive(true);
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("Successfully connected to the server.");
        if (NetworkManager.Singleton.IsHost)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
            {
                Debug.Log("All players connected, loading scene.");
                LoadSceneIfAllConnected();
            }
        }
    }

    private void LoadSceneIfAllConnected()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(LEVEL_NAME, LoadSceneMode.Single);   
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        playPanel.SetActive(true);
    }

    public void BackToMain()
    {
        playPanel.SetActive(false);
    }

    public void StopHosting()
    {
        NetworkManager.Singleton.Shutdown();
        waitingPanel.SetActive(false);
    }


}