using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject dicePanel;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TMP_Text enemyHitInfo;
    [SerializeField] private GameObject background;

    private PlayerNetwork playerNetwork;

    private bool pauseMenuUp;

    private void Start()
    {
        PlayerNetwork[] playerNetworks = FindObjectsOfType<PlayerNetwork>();
        foreach (var player in playerNetworks)
        {
            if (player.IsOwner)
            {
                playerNetwork = player;
            }
        }
    }

    public void OnBoatPlaced(bool state)
    {
        readyButton.interactable = state;
    }

    public void SendReadyToPlayerNetwork()
    {
        readyButton.gameObject.SetActive(false);
        playerNetwork.OnPlayerShipReady();

    }

    public void SetDicePanelState(bool state)
    {
        dicePanel.SetActive(state);
    }

    public void SetTurnText(string text)
    {
        turnText.text = text;
    }
    
    public void SetEnemyHitInfo(string text)
    {
        enemyHitInfo.gameObject.SetActive(true);
        enemyHitInfo.text = text;
    }

    public void SetPausePanelState(bool state)
    {
        pausePanel.SetActive(state);
        background.SetActive(state);
        pauseMenuUp = state;
    }

    public bool GetPauseMenuStatus() { return pauseMenuUp; }
    public void SetEndPanelState(bool state)
    {
        endPanel.SetActive(state);
        background.SetActive(state);
    }
    public void RemoveInfo()
    {
        enemyHitInfo.gameObject.SetActive(false);
    }

}
