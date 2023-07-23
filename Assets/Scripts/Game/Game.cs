using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Sirenix.OdinInspector;

public class Game : NetworkBehaviour
{
    public static Game current;
    public static GameState currentState => current.gameState;

    [SerializeField] GameState startingGameState;
    public GameState gameState { get; private set; }
    public List<Player> Players;

    #region Game Rules

    public int MaxDEFCON { get; private set; } = 5; 
    public int VPAutoDEFCONVictory { get; private set; } = 10; 
    public int NumActionRounds { get; private set; } = 7;
    public int HandSize => NumActionRounds + 2;

    #endregion

    #region Game Events

    public static System.Action StartGameEvent;
    public static System.Action<Phase> PhaseStartEvent, PhaseEndEvent;
    public static System.Action<Turn> TurnStartEvent, TurnEndEvent;
    public static System.Action<HeadlinePhase> HeadlinePhaseStartEvent, HeadlinePhaseEndEvent;
    public static System.Action<ActionRound> ActionRoundStartEvent, ActionRoundEndEvent;

    public static System.Action<Faction, int> AdjustMilOpsEvent, AdjustVPEvent;
    public static System.Action<int> AdjustDefconEvent; 

    public static System.Action<Effect> AddEffectEvent, CancelEffectEvent;

    public static System.Action<Faction, Card> DrawCardEvent, DiscardCardEvent, RevealCardEvent, PlayCardEvent;

    #endregion

    private void Start()
    {
        StartGame(); 
    }

    void StartGame()
    {
        current = this;
        gameState = Instantiate(startingGameState);
        StartGameEvent?.Invoke();
        //gameState.RefreshInfluence(); 

        new GameState.StartPhase(gameState.Turns.First()).Execute();
    }
}
