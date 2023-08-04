using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks; 

public class Realign : GameAction, IOpsAction
{
    [field: SerializeField] public List<Calculation<List<CountryData>>> targetRules { get; private set; } = new() { new StandardRealignTargets() };
    [field: SerializeField] public List<Modifier> Modifiers { get; private set; } = new();

    List<Attempt> attempts;

    public Stat Ops { get;  set; }
    public int OpsUsed { get; private set; }
    public void SetOps(Stat stat) => Ops = stat;

    public Realign(Faction faction) => SetActingFaction(faction);

    protected override async Task Do()
    {
        UI_Notification.SetNotification($"{ActingFaction}: Select a Realignment Target. {Ops.Value(this) - attempts.Count} attempts remaining");
        CountrySelectionManager selection = new(ActingFaction, targetRules.Select(rule => rule.Value()).Aggregate((first, next) => first.Intersect(next).ToList()), this,
            AttemptRealign, null, 0, Ops.Value(this));

        void AttemptRealign(CountryData country)
        {
            Attempt attempt = new Attempt(this, country);
            attempts.Add(attempt);
            attempt.Do();

            if (Ops.Value(this) > attempts.Count)
                UI_Notification.SetNotification($"{ActingFaction}: Select a Realignment Target. {Ops.Value(this) - attempts.Count} attempts remaining");
        }

        await selection.task; 
    }

    public class Attempt : IContext, ITargetCountry
    {
        public Dictionary<Faction, Roll> rolls = new();
        public Dictionary<Faction, int> adjacencyBonus = new();
        Dictionary<Faction, int> totals = new();
        public CountryData targetCountry { get; private set; }
        public Faction initiatingFaction { get; private set; }

        public void SetTarget(CountryData targetCountry) => this.targetCountry = targetCountry;

        public int OpsCost = 1;
        Realign realignment;

        public Attempt(Realign realign, CountryData targetCountry) : this(realign.ActingFaction, targetCountry) =>
            realignment = realign;

        public Attempt(Faction faction, CountryData targetCountry)
        {
            initiatingFaction = faction;
            this.targetCountry = targetCountry;
        }

        public void Do() { 

            string str = $"{initiatingFaction.name} attempts Coup in {targetCountry}. ";
            foreach (Player player in Game.current.Players)
            {
                rolls.Add(player.Faction, new Roll(player.Faction));
                adjacencyBonus.Add(player.Faction, targetCountry.Neighbors.Count(country => country.controllingFaction == player));

                if (targetCountry.Influence[player.Faction] > targetCountry.Influence[initiatingFaction])
                    adjacencyBonus[player.Faction] += 1; 
                
                str += $"{player.Faction.name} rolls {rolls[player.Faction]} {(adjacencyBonus[player.Faction] >= 0 ? string.Empty : "+")}{adjacencyBonus[player.Faction]}. ";
            }

            foreach (Faction f in rolls.Keys)
                totals.Add(f, rolls[f] + adjacencyBonus[f] + (int)realignment?.Modifiers.Sum(modifier => modifier.Value(this)));

            if (totals.Max(kvp => kvp.Value) > totals.Min(kvp => kvp.Value))
            {
                Faction losingFaction = totals.OrderBy(kvp => kvp.Value).First().Key;
                int AmountToRemove = Mathf.Min(targetCountry.Influence[losingFaction], totals.Max(kvp => kvp.Value) - totals.Min(kvp => kvp.Value));

                if (AmountToRemove > 0)
                    new GameState.AdjustInfluence(losingFaction, targetCountry, -AmountToRemove).Execute();
            }
        }
    }
}