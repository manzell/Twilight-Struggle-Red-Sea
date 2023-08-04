using UnityEngine;

public class Turn : Phase
{
    [field: SerializeField] public Faction initiativeFaction { get; private set; }
    public GameState.WarEra Era { get; private set; }
    public CreateActionRounds CreateActionRoundsRule;
    [SerializeField] ActionRound actionRoundPrefab;

    public void CreateActionRounds() => CreateActionRoundsRule.Do(this, actionRoundPrefab);

}
