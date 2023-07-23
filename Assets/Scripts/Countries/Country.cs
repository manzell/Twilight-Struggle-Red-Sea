using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class Country : SerializedScriptableObject
{
    public System.Action UpdateCountryEvent; 

    public System.Action<Faction, int> InfluenceChangeEvent;
    public int Stability;
    [field: SerializeField] public bool Battleground { get; private set; }
    [field: SerializeField] public bool Flashpoint { get; private set; }
    [field: SerializeField] public List<Continent> Continents { get; private set; }
    [field: SerializeField] public List<Country> Neighbors { get; private set; }

    public Faction controllingFaction => Mathf.Abs(Influence.Max(kvp => kvp.Value) - Influence.Min(kvp => kvp.Value)) >= Stability ?
        Influence.OrderByDescending(kvp => kvp.Value).First().Key : null;

    public bool Controlled => controllingFaction != null;
    public Dictionary<Faction, int> Influence => Game.current.gameState.Influence(this);

    public override bool Equals(object other) => other is Country c && name == c.name;
    public override int GetHashCode() => base.GetHashCode();
}
