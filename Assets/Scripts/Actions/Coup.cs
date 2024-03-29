using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq; 

public class Coup : GameAction, IOpsAction
{
    public static System.Action<Coup> PrepareCoupEvent, AfterCoupEvent;

    [field: SerializeField] public List<Calculation<List<CountryData>>> targetRules { get; private set; } = new() { new StandardRealignTargets() };
    [field: SerializeField] public List<Modifier> Modifiers { get; private set; } = new();

    public CountrySelectionManager Selection { get; private set; }
    public IEnumerable<CountryData> Targets { get; private set; }
    public CountryData Target { get; private set; }
    public TaskCompletionSource<CountryData> Task { get; private set; }
    public Stat Ops { get; private set; }
    public Roll Roll { get; private set; }
    public int OpsUsed { get; private set; }

    public bool Successful => coupStrength > coupDefense;
    public int totalModifier => (int)Game.currentState.effects.SelectMany(effect => effect.Modifiers).Union(Modifiers).Sum(mod => mod.Value(this));
    public int coupStrength => Roll + Ops.Value(this) + totalModifier;
    public int coupDefense => Target.Stability * 2;

    public Coup(Faction faction)
    {
        Task = new();
        Roll = new(faction);

        SetActingFaction(faction); 
        foreach(StandardRealignTargets calc in targetRules)
            calc.SetFaction(faction);
    }
    public Coup(Faction faction, Card card) : this(faction) => SetOps(card.Ops); 

    public void SetOps(Stat stat) => Ops = stat;
    public void SetTargets(IEnumerable<CountryData> countries) => Targets = countries; 

    protected override async Task Do()
    {
        UI_Notification.SetNotification($"{ActingFaction}: Select a Coup Target");

        // TODO - Are Target Rules additive or subtractive? 
        if(Targets == null) 
            Targets = targetRules.Select(rule => rule.Value()).Aggregate((first, next) => first.Intersect(next).ToList());

        Selection = new(ActingFaction, Targets, this, PrepareCoup, OnCoup, 1, 1);

        await Selection.task;

        UI_Notification.ClearNotification($"{ActingFaction}: Select a Coup Target"); 

        await Task.Task; 
    }

    void PrepareCoup(CountryData country)
    {
        PrepareCoupEvent?.Invoke(this);
    }

    public async void OnCoup(CountrySelectionManager selection)
    {
        Target = selection.Selected.First(); 

        Debug.Log($"COUP - {Target} // BG: {Target.Battleground} FP: {Target.Flashpoint} Coup Def: {coupDefense} " +
            $"Ops: {Ops.Value(this)} Modifiers: {totalModifier} " +
            $"Roll: {(int)Roll} COUP STRENGTH: {coupStrength}");

        if (Target.Battleground)
            await new GameState.AdjustDEFCON(-1).Execute();

        if (Target.Flashpoint)
        {
            GameState.DrawCard revealCardAction = new GameState.DrawCard();
            await revealCardAction.Execute();
            Card card = revealCardAction.card;

            Debug.Log($"Flashpoint: {card.name} ({card.Ops.Value(revealCardAction)} Ops)");

            if (card.Ops.Value(revealCardAction) <= 2)
                await new GameState.AdjustDEFCON(-1).Execute();

            await new GameState.Discard(card).Execute();
        }

        if (coupStrength > coupDefense)
        {
            int OpponentInfluenceToRemove = Mathf.Min(Game.current.gameState.Influence(Target)[ActingFaction.Opponent], coupStrength - coupDefense);
            int InfluenceToAdd = coupStrength - coupDefense - OpponentInfluenceToRemove;

            if (OpponentInfluenceToRemove > 0)
                await new GameState.AdjustInfluence(ActingFaction.Opponent, Target, -OpponentInfluenceToRemove).Execute();
            if (InfluenceToAdd > 0)
                await new GameState.AdjustInfluence(ActingFaction, Target, InfluenceToAdd).Execute();
        }
        else
            Debug.Log($"{ActingFaction} Coup in {Target} Failed");

        AfterCoupEvent?.Invoke(this);

        await new GameState.AdjustMilOps(ActingFaction, Ops.Value(this)).Execute();
    }

    public void CompleteCoup()
    {
        Task.SetResult(Target); 
    }
}
