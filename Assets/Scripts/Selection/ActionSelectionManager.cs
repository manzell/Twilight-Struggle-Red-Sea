using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 

public class ActionSelectionManager
{
    public List<IExecutableAction> AvailableActions { get; private set; }
    public Faction faction { get; private set; }
    public TaskCompletionSource<ActionSelectionManager> selectionTask { get; private set; }
    public Card card { get; private set; }

    System.Action<IExecutableAction> OnSelection, AfterSelection;
    public System.Action<Card> SetCardEvent; 

    public ActionSelectionManager(Faction faction, IEnumerable<IExecutableAction> actions, System.Action<IExecutableAction> OnSelection, System.Action<IExecutableAction> AfterSelection) :
        this(faction, actions, OnSelection) => this.AfterSelection += AfterSelection;

    public ActionSelectionManager(Faction faction, IEnumerable<IExecutableAction> actions, System.Action<IExecutableAction> OnSelection) :
        this(faction, actions) => this.OnSelection = OnSelection; 

    public ActionSelectionManager(Faction faction, IEnumerable<IExecutableAction> actions)
    {
        this.faction = faction;
        selectionTask = new();
        AvailableActions = new(actions);
        card = null; 

        GameObject.FindObjectOfType<UI_ActionSelection>().Setup(this);
    }

    public async void Select(IExecutableAction action, Card card = null)
    {
        Debug.Log($"Selecting {action} with {card}");

        if (card ?? this.card != null)
            SetCard(card ?? this.card); 

        OnSelection?.Invoke(action); 

        await action.Execute();

        AfterSelection?.Invoke(action);

        selectionTask.SetResult(this);
    }

    public void SetCard(Card card)
    {
        this.card = card;
        foreach(IExecutableAction action in AvailableActions)
        {
            if (action is IOpsAction opsAction)
                opsAction.SetOps(card.Ops);
            if (action is ICardAction cardAction)
                cardAction.SetCard(card); 
        }

        SetCardEvent?.Invoke(card); 
    }

    public void Open() => GameObject.FindObjectOfType<UI_ActionSelection>().Open(); 
}