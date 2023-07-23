using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks; 

public class PlayCard : GameAction
{
    Faction faction;
    Card card; 

    public PlayCard(Faction faction) => this.faction = faction;
    public PlayCard(Faction faction, Card card) : this(faction) => this.card = card; 
    public PlayCard(KeyValuePair<Faction, Card> kvp) : this(kvp.Key, kvp.Value) { }

    public void SetCard(Card card)
    {
        this.card = card; 
    }

    public override void Undo()
    {
        throw new System.NotImplementedException();
    }

    protected override async Task Do()
    {
        Debug.Log($"{faction.name} plays {card.Name}");
        await card.Execute(faction);

        await new GameState.RemoveCardFromHand(faction, card).Execute();

        if (card.RemovedEvent)
            await new GameState.RemoveCardFromGame(card).Execute(); 
        else
            await new GameState.Discard(card).Execute();
    }
}