using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

[CreateAssetMenu]
public class Continent : ScriptableObject
{
    [field: SerializeField] public int DefconRequirement { get; private set; }
    [field: SerializeField] public int presenceBonus { get; private set; }
    [field: SerializeField] public int dominationBonus { get; private set; }
    [field: SerializeField] public int controlBonus { get; private set; }
    [field: SerializeField] public Color Color { get; private set; }

    public bool Presence(Faction faction) => Game.currentState.Countries.Any(country => country.Continents.Contains(this) && country.controllingFaction == faction); 
    
    public bool Domination(Faction faction)
    {
        bool moreBattlegrounds =
            Game.currentState.Countries.Count(country => country.Continents.Contains(this) && country.Battleground && country.controllingFaction == faction) >
            Game.currentState.Countries.Count(country => country.Continents.Contains(this) && country.Battleground && country.controllingFaction == faction.Opponent); 

        bool moreCountries =
            Game.currentState.Countries.Count(country => country.Continents.Contains(this) && country.controllingFaction == faction) >
            Game.currentState.Countries.Count(country => country.Continents.Contains(this) && country.controllingFaction == faction.Opponent);

        bool anyNonBattlegrounds = Game.currentState.Countries.Any(country => country.Continents.Contains(this) && !country.Battleground && country.controllingFaction == faction);

        return moreBattlegrounds && moreCountries && anyNonBattlegrounds;
    }

    public bool Control(Faction faction)
    {
        bool allBattlegrounds =
            Game.currentState.Countries.Any(country => country.Continents.Contains(this) && country.Battleground && country.controllingFaction != faction);
        bool moreCountries =
            Game.currentState.Countries.Count(country => country.Continents.Contains(this) && country.controllingFaction == faction) >
            Game.currentState.Countries.Count(country => country.Continents.Contains(this) && country.controllingFaction == faction.Opponent);

        return allBattlegrounds && moreCountries;
    }
}