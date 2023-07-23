using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using DG.Tweening; 

public class HeadlinePhase : Phase
{
    public System.Action<Faction, Card> SubmitHeadlineEvent, HeadlineEvent;
    public System.Action<Dictionary<Faction, Card>> HeadlinesReadyEvent;
    public CreateActionRounds CreateActionRoundsRule = new();

    [SerializeField] public Dictionary<Faction, Card> headlines; 
    Dictionary<Faction, TaskCompletionSource<Card>> headlineTasks = new();

    protected override async void OnPhase()
    {
        foreach(Player player in Game.current.Players)
            headlineTasks.Add(player.Faction, new TaskCompletionSource<Card>());

        // this isn't being properly awaited. It's called from Phase.StartPhase()
        await Task.WhenAll(headlineTasks.Values.Select(tsc => tsc.Task));

        Debug.Log($"Headlines Received - {string.Join(" & ", headlines.Select(kvp => $"{kvp.Key.name}: {kvp.Value.Name}"))}");
        HeadlinesReadyEvent?.Invoke(headlines);

        // Todo - we can't pass a context into Ops.Value() here... maybe a Phase context option? 

        foreach (KeyValuePair<Faction, Card> headline in
            headlines.OrderByDescending(kvp => kvp.Value.Ops.Value()).ThenByDescending(kvp => kvp.Key == Game.current.gameState.CurrentTurn.initiativeFaction))
        {
            await new GameState.SetPhasingPlayer(headline.Key).Execute();
            await new PlayCard(headline).Execute();
        }

        EndPhase();
    }

    public override void EndPhase()
    {
        CreateActionRoundsRule.Do(Game.currentState.CurrentTurn);

        new GameState.StartPhase(transform.parent.GetComponentInChildren<ActionRound>()).Execute(); 
    }

    public void SubmitHeadline(Faction faction, Card card)
    {
        new GameState.RemoveCardFromHand(faction, card).Execute(); 
        headlines.Add(faction, card);
        SubmitHeadlineEvent?.Invoke(faction, card);
        headlineTasks[faction].SetResult(card); 
    }

    public bool HasSubmittedHeadline(Player player) => headlines.ContainsKey(player.Faction);
    public bool HasSubmittedHeadline(Faction faction) => headlines.ContainsKey(faction);

    public class CreateActionRounds
    {
        [SerializeField] ActionRound actionRoundPrefab;
        public void Do(Turn turn)
        {
            List<ActionRound> actionRounds = new();

            for (int i = 0; i < Game.current.NumActionRounds; i++)
                foreach (Faction faction in Game.current.Players.Select(player => player.Faction).OrderByDescending(faction => faction == turn.initiativeFaction))
                {
                    ActionRound ar = Instantiate(actionRoundPrefab, turn.transform);

                    ar.name = $"AR{i + 1} {faction}";
                    ar.SetActions(new() { new SpaceRace(faction), new PlayCard(faction), new Coup(faction), new Realign(faction), new Place(faction) });
                    ar.SetPhasingFaction(faction);

                    actionRounds.Add(ar);
                }

            for(int i = 0; i < actionRounds.Count - 1; i++)
                actionRounds[i].nextPhase = actionRounds[i + 1];
        }
    }
}
