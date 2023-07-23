using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq; 

public class Coup : GameAction, IOpsAction
{
    public static System.Action<Coup> PrepareCoupEvent, AfterCoupEvent;

    [field: SerializeField] public List<Calculation<List<Country>>> targetRules { get; private set; } = new() { new StandardRealignTargets() };
    public Country target => Selection.Selected.FirstOrDefault(); 
    public IEnumerable<Country> targets { get; private set; }
    Roll roll; 

    public TaskCompletionSource<Country> coupTargetTask { get; private set; }
    public CountrySelectionManager Selection { get; private set; }
    public Stat Ops { get; private set; }
    public int OpsUsed { get; private set; }

    public bool Successful => coupStrength > coupDefense;
    public int totalModifier => (int)Game.currentState.effects.SelectMany(effect => effect.Modifiers).Union(Modifiers).Sum(mod => mod.Value(this));
    public int coupStrength => roll + Ops.Value(this) + totalModifier;
    public int coupDefense => target.Stability * 2;

    [field: SerializeField] public List<Modifier> Modifiers { get; private set; } = new(); 

    public Coup(Faction faction)
    {
        SetActingFaction(faction);
    }

    public Coup(Faction faction, Card card) : this(faction) => Ops = card.Ops;

    public override void SetActingFaction(Faction faction)
    {
        foreach(StandardRealignTargets calc in targetRules)
            calc.SetFaction(faction);

        base.SetActingFaction(faction);
    }
    public void SetOps(Stat stat) => Ops = stat;
    public void SetTargets(IEnumerable<Country> countries) => targets = countries; 

    protected override async Task Do()
    {
        UI_Notification.SetNotification($"{ActingFaction}: Select a Coup Target");

        // TODO - Are Target Rules additive or subtractive? 
        if(targets == null) 
            targets = targetRules.Select(rule => rule.Value()).Aggregate((first, next) => first.Intersect(next).ToList());

        Selection = new(ActingFaction, targets, this, PrepareCoup, OnCoup, 1, 1);

        await Selection.task;
    }

    void PrepareCoup(Country country)
    {
        PrepareCoupEvent?.Invoke(this);
    }

    public async Task OnCoup(CountrySelectionManager selection)
    {
        roll = new(ActingFaction);

        Debug.Log($"COUP - {target} // BG: {target.Battleground} FP: {target.Flashpoint} Coup Def: {coupDefense} " +
            $"Ops: {Ops.Value(this)} Modifiers: {totalModifier} " +
            $"Roll: {(int)roll} COUP STRENGTH: {coupStrength}");

        System.Text.StringBuilder str = new();

        foreach (Modifier mod in Game.currentState.effects.SelectMany(effect => effect.Modifiers).Union(Modifiers))
            str.Append($"{mod.source}: {(mod.Value(this) > 0 ? "+" : string.Empty) + mod.Value(this)}");

        if (str.Length > 0)
            Debug.Log(str);

        if (target.Battleground)
            await new GameState.AdjustDEFCON(-1).Execute();

        if (target.Flashpoint)
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
            int OpponentInfluenceToRemove = Mathf.Min(Game.current.gameState.Influence(target)[ActingFaction.Opponent], coupStrength - coupDefense);
            int InfluenceToAdd = coupStrength - coupDefense - OpponentInfluenceToRemove;

            if (OpponentInfluenceToRemove > 0)
                await new GameState.AdjustInfluence(ActingFaction.Opponent, target, -OpponentInfluenceToRemove).Execute();
            if (InfluenceToAdd > 0)
                await new GameState.AdjustInfluence(ActingFaction, target, InfluenceToAdd).Execute();
        }
        else
            Debug.Log($"{ActingFaction} Coup in {target} Failed");

        AfterCoupEvent?.Invoke(this);

        await new GameState.AdjustMilOps(ActingFaction, Ops.Value(this)).Execute();
    }
}
