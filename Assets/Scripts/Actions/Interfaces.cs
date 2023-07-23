using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 

public interface IContext { }

public interface ITargetCountry
{
    public Country targetCountry { get; }
    public void SetTarget(Country targetCountry);
}

public interface IActingPlayerAction : IExecutableAction
{
    public Faction ActingFaction { get; } 
    public void SetActingFaction(Faction faction);
}

public interface IPlaceInfluence 
{ 
    public List<Place.Placement> Placements { get; }
}