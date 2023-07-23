using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEditor;
using static Realign;

public class Place : GameAction, IOpsAction
{
    [field: SerializeField] public List<Calculation<List<Country>>> targetRules { get; private set; } = new() { new StandardPlacementTargets() };
    public List<Placement> placements { get; private set; }
    public List<Modifier> Modifiers { get; private set; }
    public TaskCompletionSource<Country> placementTask { get; private set; }
    CountrySelectionManager selection;
    [field: SerializeField] public Stat Ops { get; private set; }
    public int OpsUsed { get; private set; }

    public void SetOps(Stat stat) => Ops = stat; 

    public Place(Faction faction) => SetActingFaction(faction);

    protected override async Task Do()
    {
        placements = new();

        UI_Notification.SetNotification($"Choose a country to place influence. {Ops.Value(this) - placements.Sum(placement => placement.Ops.Value(this))} Ops remaining.");

        selection = new(ActingFaction, targetRules.Select(rule => rule.Value()).Aggregate((first, next) => first.Intersect(next).ToList()), this,
            AttemptPlacement, null, 0, Ops.Value(this));

        void AttemptPlacement(Country country)
        {
            Placement placement = new Placement(ActingFaction, country);
            placements.Add(placement);
            placement.Do();

            if (Ops.Value(this) > placements.Sum(placement => placement.Ops.Value(this)))
                UI_Notification.SetNotification($"Choose a country to place influence. {Ops.Value(this) - placements.Sum(placement => placement.Ops.Value(this))} Ops remaining.");
            else
                selection.Complete(); 
        }

        await selection.task;
    }

    public class Placement
    {   
        public Stat Ops { get; private set; }
        public Faction faction { get; private set; }
        public Country country { get; private set; }

        public Placement(Faction faction, Country country)
        {
            this.country = country; 
            Ops = new(country.controllingFaction == faction.Opponent ? 2 : 1); 
        }

        public void Do() => new GameState.AdjustInfluence(faction, country, 1).Execute(); 
    }
}

// This is the counterpart to Place
public class RemoveInfluence : GameAction
{
    [SerializeField] int OpsToRemove;
    [SerializeField] List<Country> eligibleCountries;
    CountrySelectionManager selection;

    protected override Task Do()
    {
        selection = new(ActingFaction, eligibleCountries.Where(country => country.Influence[ActingFaction.Opponent] > 0).ToList(), this,
            RemoveInfluence, null, 0, OpsToRemove); 

        async void RemoveInfluence(Country country) =>
            await new GameState.AdjustInfluence(ActingFaction.Opponent, country, -1).Execute();

        return Task.CompletedTask; 
    }
}