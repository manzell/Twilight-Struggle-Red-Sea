using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks; 

public class Event : GameAction, ICardAction
{
    public Card card { get; private set; }
    public TaskCompletionSource<Event> completion { get; private set; }

    public Card Card => card;

    public static System.Action<Event> StartPlayCardEvent, FinishPlayCardEvent; 

    public Event(Faction faction) => SetActingFaction(faction);
    public Event(Faction faction, Card card) : this(faction) => this.card = card; 
    public Event(KeyValuePair<Faction, Card> kvp) : this(kvp.Key, kvp.Value) { }

    public void SetCard(Card card)
    {
        this.card = card; 
    }

    protected override async Task Do()
    {
        Debug.Log($"{ActingFaction.name} Events {card.Name} [{card.Faction.name}]");

        UI_Game.SetPlayer(card.Faction ?? ActingFaction); 

        completion = new(); 
        StartPlayCardEvent?.Invoke(this); 

        await card.Execute(ActingFaction);

        if (card.RemovedEvent)
            await new GameState.RemoveCardFromGame(card).Execute(); 
        else
            await new GameState.Discard(card).Execute();

        FinishPlayCardEvent?.Invoke(this); 
        await completion.Task; 
    }

    public bool Can(Card card) => true; 
}