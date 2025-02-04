using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region State Machine

    public StateMachine playerStateMachine { get; private set; }

    public PlacingBoatState placingBoatState { get; private set; }
    public RollingDiceState rollingDiceState { get; private set; }
    public WaitingForOpponentState waitingForOpponentState { get; private set; }
    public AttackingState attackingState { get; private set; }
    public WinningState winningState { get; private set; }
    public MainMenuState mainMenuState { get; private set; }
    public LoosingState loosingState { get; private set; }

    #endregion

    private PlayerNetwork playerNetwork;
    private InGameUIController ui;
    

    private void Awake()
    {
        playerNetwork = GetComponent<PlayerNetwork>();
        playerStateMachine = new StateMachine();
        ui = new InGameUIController();
        placingBoatState = new PlacingBoatState(this, playerStateMachine, playerNetwork, ui);
        rollingDiceState = new RollingDiceState(this, playerStateMachine, playerNetwork, ui);
        waitingForOpponentState = new WaitingForOpponentState(this, playerStateMachine, playerNetwork, ui);
        attackingState = new AttackingState(this, playerStateMachine, playerNetwork, ui);
        winningState = new WinningState(this, playerStateMachine, playerNetwork, ui);
        mainMenuState = new MainMenuState(this, playerStateMachine, playerNetwork, ui);
        loosingState = new LoosingState(this, playerStateMachine, playerNetwork, ui);
    }

    private void Start()
    {
        playerStateMachine.Initialize(mainMenuState);
    }


    private void Update()
    {
        playerStateMachine.currentState.Update();
    }
    
}
