using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 

public class ActionSelectionManager
{
    public List<IExecutableAction> AvailableActions { get; private set; }
    public Faction faction { get; private set; }
    public TaskCompletionSource<IExecutableAction> selectionTask { get; private set; }
    public Card card { get; private set; }

    public ActionSelectionManager(Faction faction, IEnumerable<IExecutableAction> actions)
    {
        selectionTask = new();
        AvailableActions = new(actions);
        this.faction = faction;

        GameObject.FindObjectOfType<UI_ActionSelection>().Setup(this);
    }

    public void Select(IExecutableAction action, Card card = null)
    {
        this.card = card; 
        action.Execute();
        selectionTask.SetResult(action);
    }
}