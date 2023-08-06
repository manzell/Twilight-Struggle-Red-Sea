using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateActionRounds
{       
    public virtual void Do(Turn turn, ActionRound actionRoundPrefab)
    { 
        List<ActionRound> actionRounds = new();
        IEnumerable<Faction> factions = Game.current.Players.Select(player => player.Faction).OrderByDescending(faction => faction == turn.initiativeFaction);

        for (int i = 0; i < Game.current.NumActionRounds; i++)
            foreach (Faction faction in factions)
            {
                ActionRound ar = GameObject.Instantiate(actionRoundPrefab, turn.transform);

                ar.name = $"AR {i + 1}: {faction}";
                ar.SetActions(new() { new SpaceRace(faction), new Event(faction), new Coup(faction), new Realign(faction), new Place(faction) });
                ar.SetPhasingFaction(faction);

                actionRounds.Add(ar);
            }

        for (int i = 0; i < actionRounds.Count - 1; i++)
            actionRounds[i].nextPhase = actionRounds[i + 1];
    }
}
