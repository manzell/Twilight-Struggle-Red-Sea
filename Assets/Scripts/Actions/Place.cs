using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class Place : GameAction, IOpsAction
{
    [field: SerializeField] public List<StandardPlacementTargets> targetRules { get; private set; } = new() { new StandardPlacementTargets() };
    public List<Placement> placements { get; private set; }
    public List<Modifier> Modifiers { get; private set; }
    public TaskCompletionSource<CountryData> placementTask { get; private set; }
    CountrySelectionManager selection;
    [field: SerializeField] public Stat Ops { get; private set; }
    public int OpsUsed { get; private set; }

    public void SetOps(Stat stat) => Ops = stat;

    public Place(Faction faction) => SetActingFaction(faction); 
    public Place(Faction faction, Card card) : this(faction) => SetOps(card.Ops);

    protected override async Task Do()
    {
        placements = new();

        UI_Notification.SetNotification($"Choose a country to place influence. {Ops.Value(this) - placements.Sum(placement => placement.OpCost.Value(this))} Ops remaining.");

        selection = new(ActingFaction, targetRules.Select(rule =>  rule.Value(ActingFaction))
            .Aggregate((first, next) => first.Intersect(next).ToList()), this, AttemptPlacement, null, 0, Ops.Value(this));

        await selection.task;

        void AttemptPlacement(CountryData country)
        {
            Placement placement = new Placement(ActingFaction, country);
            placements.Add(placement);

            if (Ops.Value(this) > placements.Sum(placement => placement.OpCost.Value(this)))
                UI_Notification.SetNotification($"Choose a country to place influence. {Ops.Value(this) - placements.Sum(placement => placement.OpCost.Value(this))} Ops remaining.");
            if (Ops.Value(this) >= placements.Sum(placement => placement.OpCost.Value(this)))
                placement.Do();
            else 
                placements.Remove(placement); 
        }
    }

    public class Placement
    {   
        public Stat OpCost { get; private set; }
        public Faction faction { get; private set; }
        public CountryData country { get; private set; }

        public Placement(Faction faction, CountryData country)
        {
            this.country = country; 
            this.faction = faction;
            OpCost = new(country.controllingFaction == faction.Opponent ? 2 : 1); 
        }

        public void Do() => new GameState.AdjustInfluence(faction, country, 1).Execute(); 
    }
}

// This is the counterpart to Place
public class RemoveInfluence : GameAction
{
    [SerializeField] int OpsToRemove;
    [SerializeField] List<CountryData> eligibleCountries;
    CountrySelectionManager selection;

    protected override Task Do()
    {
        selection = new(ActingFaction, eligibleCountries.Where(country => country.Influence[ActingFaction.Opponent] > 0).ToList(), this,
            RemoveInfluence, null, 0, OpsToRemove); 

        async void RemoveInfluence(CountryData country) =>
            await new GameState.AdjustInfluence(ActingFaction.Opponent, country, -1).Execute();

        return Task.CompletedTask; 
    }
}