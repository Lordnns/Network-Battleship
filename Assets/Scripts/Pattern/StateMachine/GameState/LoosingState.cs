using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoosingState : GameState
{
    public LoosingState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui) : base(playerController, playerStateMachine, playerNetwork, ui)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("You lost");

        ui = Object.FindObjectOfType<InGameUIController>();
        ui.SetTurnText("You lost");
        ui.SetEndPanelState(true);
    }

    public override void Update()
    {
        base.Update();
        HandleClientDisconect();    
    }

    private void HandleClientDisconect()
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
}
