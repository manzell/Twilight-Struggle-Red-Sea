using UnityEngine;

public class Turn : Phase
{
    [field: SerializeField] public Faction initiativeFaction { get; private set; }
    public GameState.WarEra Era { get; private set; }
    public CreateActionRounds CreateActionRoundsRule;
    [SerializeField] ActionRound actionRoundPrefab;

    protected override void OnPhase()
    {
        CreateActionRoundsRule.Do(this, actionRoundPrefab);
        new GameState.StartPhase(transform.parent.GetComponentInChildren<ActionRound>()).Execute();
    }

    public override void EndPhase()
    {

        base.EndPhase();
    }
}
