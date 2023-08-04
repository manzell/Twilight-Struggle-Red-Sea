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

    [SerializeField] public Dictionary<Faction, Card> headlines; 
    Dictionary<Faction, TaskCompletionSource<Card>> headlineTasks = new();

    protected override async void OnPhase()
    {
        foreach(Player player in Game.current.Players)
            headlineTasks.Add(player.Faction, new TaskCompletionSource<Card>());

        await Task.WhenAll(headlineTasks.Values.Select(tsc => tsc.Task));

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
        Game.currentState.CurrentTurn.CreateActionRounds(); 

        new GameState.StartPhase(transform.parent.GetComponentInChildren<ActionRound>()).Execute(); 
    }

    public void SubmitHeadline(Faction faction, Card card)
    {
        Debug.Log($"{faction.name} Headline Received: {card.name}"); 
        headlines.Add(faction, card);
        SubmitHeadlineEvent?.Invoke(faction, card);
        headlineTasks[faction].SetResult(card); 
    }

    public bool HasSubmittedHeadline(Player player) => headlines.ContainsKey(player.Faction);
    public bool HasSubmittedHeadline(Faction faction) => headlines.ContainsKey(faction);
}
