using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AttackingState : GameState
{
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("entering attack" + playerController.GetComponent<PlayerNetwork>().OwnerClientId);

        ui = Object.FindObjectOfType<InGameUIController>();
        ui.SetTurnText("Attacking");
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

    public override void Exit()
    {
        base.Exit();
        ui.RemoveInfo();
    }
    
    public AttackingState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui) : base(playerController, playerStateMachine, playerNetwork, ui)
    {
    }
}
