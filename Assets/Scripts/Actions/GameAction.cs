using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
public abstract class GameAction : IExecutableAction
{
    public static System.Action<IExecutableAction> PrepareAction, CompleteAction;
    [SerializeField] List<Condition> conditions;

    public Faction ActingFaction { get; protected set; }

    public virtual async Task Execute()
    {
        PrepareAction?.Invoke(this); 
        if (Can())
        {
            await Do();
            Game.current.gameState.ExecutedActions.Add(this);
        }
        CompleteAction?.Invoke(this); 
    }

    public virtual bool Can() => true;
    protected abstract Task Do();
    public virtual void Undo() { }

    public virtual void SetActingFaction(Faction faction) => ActingFaction = faction;
}

public interface IExecutableAction : IContext
{
    public abstract Task Execute();
    public virtual void Undo() { }
}

public interface ICardAction : IActingPlayerAction
{
    public Card Card { get; }
    public void SetCard(Card card);
    public bool Can(Card card);
}

public interface IOpsAction : IActingPlayerAction
{
    public Stat Ops { get; }
    public int OpsUsed { get; }
    public List<Modifier> Modifiers { get; }
    public void SetOps(Stat stat); 
}

public abstract class CardAction : GameAction, ICardAction
{
    public Card Card { get; private set; }
    public void SetCard(Card card) => Card = card;

    public override Task Execute()
    {
        Debug.Log($"> {Card.name}: {Card.CardText}"); 
        return base.Execute();
    }

    public virtual bool Can(Card card) => true; 
}