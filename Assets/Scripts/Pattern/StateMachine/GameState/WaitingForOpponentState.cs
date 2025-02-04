using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitingForOpponentState : GameState
{
    public override void Enter()
    {
        base.Enter();
        Debug.Log("entering wait" + playerController.GetComponent<PlayerNetwork>().OwnerClientId);
       
        ui = Object.FindObjectOfType<InGameUIController>();

        ui.SetTurnText("Waiting for opponent");

    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ui.GetPauseMenuStatus())
                ui.SetPausePanelState(false);
            else
                ui.SetPausePanelState(true);
        }
        HandleClientDisconnect();
    }
    private void HandleClientDisconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (NetworkManager.Singleton.ConnectedClientsList.Count != 2)
                {
                    NetworkManager.Singleton.Shutdown();
                    SceneManager.LoadScene(0);
                }
            }
        }
    }


    public WaitingForOpponentState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui) : base(playerController, playerStateMachine, playerNetwork, ui)
    {
    }
}
