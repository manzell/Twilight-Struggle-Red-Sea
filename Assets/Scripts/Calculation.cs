using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

[System.Serializable]
public abstract class Calculation<T>
{
    public abstract T Value();
}

public class StandardPlacementTargets 
{
    public List<CountryData> Value(Faction faction) => new(Game.currentState.Countries
            .Where(country => !country.AdjacencyRequired || country.Influence[faction] > 0 || country.Neighbors.Any(neighbor => neighbor.Influence[faction] > 0)));
}

public class StandardRealignTargets : Calculation<List<CountryData>>
{
    [field: SerializeField] public Faction faction { get; private set; }

    public override List<CountryData> Value() => new(Game.currentState.Countries
        .Where(country => country.name != "Strategic Sea Lanes" && country.Influence[faction.Opponent] > 0 
            && Game.current.gameState.defcon > country.Continents.Min(continent => continent.DefconRequirement)));

    public void SetFaction(Faction faction) => this.faction = faction;
}

public class CountriesInContinent : Calculation<List<CountryData>>
{
    [SerializeField] Continent continent;
    public CountriesInContinent(Continent continent) => this.continent = continent; 
    public override List<CountryData> Value() => new(Game.currentState.Countries.Where(country => country.Continents.Contains(continent))); 
}

public class NextSpaceStage : Calculation<SpaceStage> 
{
    [SerializeField] Faction faction;
    public NextSpaceStage(Faction faction) => this.faction = faction;

    public override SpaceStage Value() => Game.current.gameState.SpaceRaceStages.FirstOrDefault(stage => !stage.factions.Contains(faction));
}

public class CountryList : Calculation<List<CountryData>>
{
    [SerializeField] List<CountryData> countries;
    public CountryList(List<CountryData> countries) => this.countries = countries; 
    public override List<CountryData> Value() => countries; 
}

public class ContinentScore : Calculation<(Faction, int)> 
{
    [SerializeField] Continent continent;
    public ContinentScore(Continent continent) => this.continent = continent;

    public override (Faction, int) Value()
    {
        Faction faction = null;
        int score = 0; 

        foreach(Faction f in Game.current.Players.Select(player => player.Faction))
        {
            if (faction == null)
                faction = f;

            int points = 0;

            if (continent.Control(f))
                points += continent.controlBonus;
            else if (continent.Domination(f))
                points += continent.dominationBonus; 
            else if(continent.Presence(f))
                points += continent.presenceBonus;

            if (faction == f)
                score += points;
            else
                score -= points; 
        }

        if (score > 0)
            return (faction, score);
        else
            return (faction.Opponent, -score); 
    }
}