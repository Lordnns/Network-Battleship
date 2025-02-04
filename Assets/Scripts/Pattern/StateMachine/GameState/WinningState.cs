using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinningState : GameState
{

    public override void Enter()
    {
        base.Enter();
        Debug.Log("You won!!!!");

        ui = Object.FindObjectOfType<InGameUIController>();
        ui.SetTurnText("You won!!!!");
        ui.SetEndPanelState(true);

    }

    public override void Update()
    {
        base.Update();
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

    public WinningState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui) : base(playerController, playerStateMachine, playerNetwork, ui)
    {
    }
}
