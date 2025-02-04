using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RollingDiceState : GameState
{
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("rolling baby" + playerController.GetComponent<PlayerNetwork>().OwnerClientId);
        
        //not sure if this will cause some bug
        ShipPlacementManager.Instance.gameObject.SetActive(false);

        ui = Object.FindObjectOfType<InGameUIController>();
        ui.SetDicePanelState(true);
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


    public void CheckDices()
    {
        //check if both player threw the dice
        if (!playerNetwork.hasThrownDice.Value || !playerNetwork.GetOpponent().hasThrownDice.Value) return;
        DiceController d = Object.FindObjectOfType<DiceController>();
        
        // if there is a tie
        if (playerNetwork.diceValue.Value == playerNetwork.GetOpponent().diceValue.Value)
        {
            //reset the local
            playerNetwork.hasThrownDice.Value = false;
            d.EnableDiceButton();
            
            //reset remote
            playerNetwork.ResetRemotePlayerDiceServerRpc();
            return;
        }

        // set local turn
        CheckForWinner(playerNetwork.diceValue.Value, playerNetwork.GetOpponent().diceValue.Value);
        
        //set remote turn
        playerNetwork.SetRemotePlayerStateServerRPC(playerNetwork.diceValue.Value, playerNetwork.GetOpponent().diceValue.Value);
        

    }

    public void CheckForWinner(int local, int remote)
    {
        Debug.Log(local + " " + remote);
        
        //if player has a bigger dice number then start in attack mode
        if (local > remote)
        {
            playerStateMachine.ChangeState(playerController.attackingState);
            InGameUIController ui = Object.FindObjectOfType<InGameUIController>();
            ui.SetTurnText("Attacking");
        }
        //if player has a smaller dice number then start in waiting mode
        if (local < remote)
        {
            playerStateMachine.ChangeState(playerController.waitingForOpponentState);
            
            InGameUIController ui = Object.FindObjectOfType<InGameUIController>();
            ui.SetTurnText("Waiting for opponent");
        }

        DiceController d = Object.FindObjectOfType<DiceController>();
        d.ShowDiceResult(local, remote);
    }

    public RollingDiceState(PlayerController playerController, StateMachine playerStateMachine, PlayerNetwork playerNetwork, InGameUIController ui) : base(playerController, playerStateMachine, playerNetwork, ui)
    {
    }
}
