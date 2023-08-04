using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class Condition
{
    public abstract bool Can(IContext context); 
}

public class ControlCountry : Condition
{
    [SerializeField] CountryData country;
    [SerializeField] Faction faction;

    public override bool Can(IContext context) => country.controllingFaction == faction;
}

public class CountryHasInfluence : Condition
{
    [SerializeField] CountryData country;
    [SerializeField] Faction faction;

    public override bool Can(IContext context) => country.Influence[faction] > 0;
}
public class HasPlacedOpsInTheater : Condition
{
    [SerializeField] Continent continent;
    public HasPlacedOpsInTheater(Continent continent) => this.continent = continent;

    public override bool Can(IContext context) => context is Place placementAction && 
        placementAction.placements.Any(placement => placement.country.Continents.Contains(continent)); 
}

public class PlacesOps : Condition
{
    public override bool Can(IContext context) => context is Place; 
}

public class IsFactionCard : Condition
{
    [SerializeField] Faction faction;
    public override bool Can(IContext context) => context is ICardAction cardAction && cardAction.Card.Faction == faction; 
}

public class IsActingFaction : Condition
{
    [SerializeField] Faction faction;
    public override bool Can(IContext context) => context is IActingPlayerAction _context && _context.ActingFaction == faction; 
}

public class HasCardInHand : Condition
{
    [SerializeField] Card card;
    [SerializeField] Faction faction;

    public override bool Can(IContext context) => Game.currentState.hands[faction].Contains(card); 
}

public class IsCoupAction: Condition
{
    public override bool Can(IContext context) => context is Coup;
}  

public class EffectApplied : Condition
{
    [SerializeField] Effect effect;
    public override bool Can(IContext context) => Game.currentState.effects.Contains(effect);
}

public class EffectNotApplied : Condition
{
    [SerializeField] Effect effect;
    public override bool Can(IContext context) => !Game.currentState.effects.Contains(effect);
}

public class VPComparison : Condition
{
    enum ComparisonType { More, Less, Same }
    [SerializeField] ComparisonType comparisonType;

    public override bool Can(IContext context)
    {
        if (context is IActingPlayerAction action)
        {
            switch (comparisonType)
            {
                case ComparisonType.More:
                        return action.ActingFaction == Game.current.Players.First().Faction ? 
                            Game.currentState.VictoryPoints > 0 : Game.currentState.VictoryPoints < 0;
                case ComparisonType.Less:
                        return action.ActingFaction == Game.current.Players.First().Faction ?
                            Game.currentState.VictoryPoints < 0 : Game.currentState.VictoryPoints > 0;
                case ComparisonType.Same:
                    return Game.currentState.VictoryPoints == 0;
            }
        }
        return false; 
    }
}

public class HasPermanentCard : Condition
{
    [SerializeField] Card card;
    [SerializeField] Faction faction; 

    public HasPermanentCard(Card card, Faction faction)
    {
        this.card = card;
        this.faction = faction;
    }

    public override bool Can(IContext context) => Game.currentState.PermanentCards[card].faction == faction;
}

public class CardOpsIs : Condition
{
    public enum ComparisonType { greaterThan, lessThan, greaterThanEqualTo, lesserThanEqualTo}
    [SerializeField] ComparisonType comparisonType;
    [SerializeField] int amount; 

    public override bool Can(IContext context)
    {
        if (context is ICardAction cardAction)
        {
            switch (comparisonType)
            {
                case ComparisonType.greaterThan:
                    return cardAction.Card.Ops.Value() > amount; 
                case ComparisonType.lessThan:
                    return cardAction.Card.Ops.Value() < amount;
                case ComparisonType.greaterThanEqualTo:
                    return cardAction.Card.Ops.Value() >= amount;
                case ComparisonType.lesserThanEqualTo:
                    return cardAction.Card.Ops.Value() <= amount;
            }
        }

        return true; 
    }
}