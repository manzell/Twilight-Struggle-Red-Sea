using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public abstract class SpaceStage
{
    [field: SerializeField] public string name;
    [field: SerializeField] public int OpsRequired { get; private set; }
    [field: SerializeField] public int MaxRoll { get; private set; }
    [field: SerializeField] public Sprite icon;
    [field: SerializeField] public List<Faction> factions { get; private set; } = new(); 

    protected abstract void OnAchieve(Faction faction);

    public void Complete(Faction faction)
    {
        factions.Add(faction);
        OnAchieve(faction); 
    }
}

public class LeaveInnerSystem : SpaceStage
{
    protected override void OnAchieve(Faction faction)
    {
        Game.HeadlinePhaseEndEvent += RemoveOneInfluence; 
    }

    async void RemoveOneInfluence(HeadlinePhase headline)
    {
        if(factions.Count == 1)
        {
            UI_Notification.SetNotification($"Mission to the Outer Solar System: {factions[0].name} may remove 1 influence from any country");

            CountrySelectionManager selection = new(factions[0], Game.currentState.Countries
                .Where(country => country.Influence[factions[0].Opponent] > 0).ToList(), null);

            Country country = (await selection.task).Selected.First(); 

            await new GameState.AdjustInfluence(factions[0].Opponent, country, -1).Execute(); 
        }
    }
}

public class PhotoOtherPlanet : SpaceStage
{
    [SerializeField] int[] VPAward;

    protected override void OnAchieve(Faction faction) => new GameState.AdjustVP(faction, VPAward[factions.IndexOf(faction)]).Execute(); 
}

public class SpaceShuttle : SpaceStage
{
    [SerializeField] int VPAward;
    [SerializeField] int numCardDraw;

    protected override void OnAchieve(Faction faction)
    {
        if (factions.Count == 1)
        {
            new GameState.AdjustVP(faction, VPAward).Execute();
            
            for(int i = 0; i < numCardDraw; i++) 
                new GameState.DealCard(faction).Execute();
        }
    }
}
