using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Country
{
    CountryData countryData;

    public System.Action<Faction, int> InfluenceChangeEvent;
    public int Stability { get; private set; }
    public bool Battleground { get; private set; }
    public bool Flashpoint { get; private set; }
    public List<Continent> Continents { get; private set; }
    public List<Country> Neighbors { get; private set; }

    public bool Controlled => controllingFaction != null;
    public Dictionary<Faction, int> Influence => Game.current.gameState.Influence(countryData);
    public Faction controllingFaction => Mathf.Abs(Influence.Max(kvp => kvp.Value) - Influence.Min(kvp => kvp.Value)) >= Stability ?
        Influence.OrderByDescending(kvp => kvp.Value).First().Key : null;

    public void Setup(CountryData cData)
    {
        countryData = cData;
        Stability = cData.Stability;
        Battleground = cData.Battleground; 
        Flashpoint = cData.Flashpoint;
        Continents = new(cData.Continents); 
        // Neighbors = new(cData.Neighbors); // TODO - Repalce this with 
    }

    public override bool Equals(object other) => other is CountryData c && countryData.name == c.name;
    public override int GetHashCode() => base.GetHashCode();
}
